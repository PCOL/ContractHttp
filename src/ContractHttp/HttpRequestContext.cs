namespace ContractHttp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using ContractHttp.Reflection.Emit;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Represents a request call context.
    /// </summary>
    internal class HttpRequestContext
        : IHttpRequestContext
    {
        /// <summary>
        /// A reference to the client proxy options.
        /// </summary>
        private readonly HttpClientProxyOptions options;

        /// <summary>
        /// A reference to the content type.
        /// </summary>
        private readonly string contentType;

        /// <summary>
        /// The index of the response argument.
        /// </summary>
        private int responseArg = -1;

        /// <summary>
        /// The index of the data argument.
        /// </summary>
        private int dataArg = -1;

        /// <summary>
        /// The data argument type.
        /// </summary>
        private Type dataArgType;

        /// <summary>
        /// The response processor type.
        /// </summary>
        private Type responseProcessorType;

        /// <summary>
        /// The request action.
        /// </summary>
        private Action<HttpRequestMessage> requestAction;

        /// <summary>
        /// The response action.
        /// </summary>
        private Action<HttpResponseMessage> responseAction;

        /// <summary>
        /// The response function argument.
        /// </summary>
        private int responseFuncArg = -1;

        /// <summary>
        /// A dictionary of from headers.
        /// </summary>
        private Dictionary<int, string> fromHeaders;

        /// <summary>
        /// A dictionary of from response parameters.
        /// </summary>
        private Dictionary<int, Tuple<FromResponseAttribute, Type>> fromResponses;

        /// <summary>
        /// A cancellation token source.
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// A cancellation token.
        /// </summary>
        private CancellationToken cancellationToken = CancellationToken.None;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestContext"/> class.
        /// </summary>
        /// <param name="methodInfo">The methods <see cref="MethodInfo"/>.</param>
        /// <param name="arguments">The methods arguments.</param>
        /// <param name="contentType">The content type.</param>
        /// <param name="options">The proxy options.</param>
        public HttpRequestContext(
            MethodInfo methodInfo,
            object[] arguments,
            string contentType,
            HttpClientProxyOptions options)
        {
            this.MethodInfo = methodInfo;
            this.Arguments = arguments;
            this.contentType = contentType;
            this.options = options;
        }

        /// <summary>
        /// Gets the methods info.
        /// </summary>
        public MethodInfo MethodInfo { get; }

        /// <summary>
        /// Gets the methods arguments.
        /// </summary>
        public object[] Arguments { get; }

        /// <summary>
        /// Builds a request, sends it, and proecesses the response.
        /// </summary>
        /// <param name="httpMethod">The http method.</param>
        /// <param name="baseUri">The base Uri.</param>
        /// <param name="uriPath">The Uri path.</param>
        /// <param name="contentType">The content type.</param>
        /// <param name="timeout">A value representing the request timeout.</param>
        /// <returns>The result of the request.</returns>
        public async Task<object> BuildAndSendRequestAsync(
            HttpMethod httpMethod,
            string baseUri,
            string uriPath,
            string contentType,
            TimeSpan? timeout)
        {
            var requestBuilder = new HttpRequestBuilder(this.options)
                .SetMethod(httpMethod)
                .SetHttpVersion(this.options.HttpVersion);

            var names = this.CheckArgsAndBuildRequest(requestBuilder, out string uri);

            if (string.IsNullOrWhiteSpace(uri) == false)
            {
                baseUri = uri;
            }

            requestBuilder
                .AddMethodHeaders(this.MethodInfo, names, this.Arguments)
                .SetUri(Utility.CombineUri(baseUri, uriPath.ExpandString(names, this.Arguments)));

            Type returnType = this.MethodInfo.ReturnType;

            await requestBuilder.AddAuthorizationHeaderAsync(
                this.MethodInfo,
                names,
                this.Arguments,
                this.options.Services);

            HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead;
            if (returnType == typeof(HttpResponseMessage) ||
                returnType == typeof(Task<HttpResponseMessage>) ||
                returnType == typeof(Task<Stream>))
            {
                completionOption = HttpCompletionOption.ResponseHeadersRead;
            }

            if (timeout.HasValue == true)
            {
                this.SetTimeout(timeout.Value);
            }

            var httpRequestSenderFactory = this.options.Services?.GetService<IHttpRequestSenderFactory>();

            var httpClient = this.options.GetHttpClient();
            var requestSender = httpRequestSenderFactory?.CreateRequestSender(httpClient, this) ??
                new HttpRequestSender(httpClient, this);

            if (returnType.IsAsync(out Type taskType) == true &&
                taskType != typeof(void))
            {
                Type asyncType = typeof(AsyncCall<>).MakeGenericType(taskType);
                object obj = Activator.CreateInstance(asyncType, requestSender, this);
                var mi = asyncType.GetMethod("SendAsync", new Type[] { typeof(HttpRequestBuilder), typeof(HttpCompletionOption) });
                return mi.Invoke(obj, new object[] { requestBuilder, completionOption });
            }

            var response = await requestSender
                .SendAsync(
                    requestBuilder,
                    completionOption)
                .ConfigureAwait(false);

            return await this.ProcessResultAsync(
                response,
                returnType);
        }

        /// <summary>
        /// Checks the arguments for specific ones.
        /// </summary>
        /// <param name="requestBuilder">A request builder.</param>
        /// <param name="uri">A variable to receive a Uri.</param>
        /// <returns>An array of argument names.</returns>
        private string[] CheckArgsAndBuildRequest(
            HttpRequestBuilder requestBuilder,
            out string uri)
        {
            uri = null;

            var responseAttr = this.MethodInfo.GetMethodOrTypeAttribute<HttpResponseProcessorAttribute>();
            if (responseAttr != null)
            {
                this.responseProcessorType = responseAttr.ResponseProcesorType;
            }

            var formUrlAttrs = this.MethodInfo.GetCustomAttributes<AddFormUrlEncodedPropertyAttribute>();
            if (formUrlAttrs.Any() == true)
            {
                foreach (var attr in formUrlAttrs)
                {
                    requestBuilder.AddFormUrlProperty(attr.Key, attr.Value);
                }
            }

            Type returnType = this.MethodInfo.ReturnType;
            if (returnType.IsAsync(out Type taskType) == true)
            {
                returnType = taskType;
            }

            bool formUrlContent = false;
            bool contentDisposition = false;
            ParameterInfo[] parms = this.MethodInfo.GetParameters();
            if (parms == null)
            {
                return new string[0];
            }

            string[] names = new string[parms.Length];
            for (int i = 0; i < parms.Length; i++)
            {
                names[i] = parms[i].Name;
                Type parmType = parms[i].ParameterType;

                if (parms[i].IsOut == true)
                {
                    var fromHeader = parms[i].GetCustomAttribute<FromHeaderAttribute>();
                    if (fromHeader != null)
                    {
                        this.fromHeaders = this.fromHeaders ?? new Dictionary<int, string>();
                        this.fromHeaders.Add(i, fromHeader.Name);
                    }
                    else
                    {
                        var fromResponse = parms[i].GetCustomAttribute<FromResponseAttribute>();
                        if (fromResponse != null)
                        {
                            this.fromResponses = this.fromResponses ?? new Dictionary<int, Tuple<FromResponseAttribute, Type>>();
                            this.fromResponses.Add(i, Tuple.Create(fromResponse, parmType.GetElementType()));
                        }
                        else
                        {
                            var elemParmType = parms[i].ParameterType.GetElementType();
                            if (elemParmType == typeof(HttpResponseMessage))
                            {
                                this.responseArg = i;
                            }
                            else
                            {
                                this.dataArg = i;
                                this.dataArgType = parmType;
                            }
                        }
                    }

                    continue;
                }

                if (returnType != typeof(void) &&
                    parmType == typeof(Func<,>).MakeGenericType(typeof(HttpResponseMessage), returnType))
                {
                    this.responseFuncArg = i;
                }
                else if (parmType == typeof(Action<HttpRequestMessage>))
                {
                    this.requestAction = (Action<HttpRequestMessage>)this.Arguments[i];
                }
                else if (parmType == typeof(Action<HttpResponseMessage>))
                {
                    this.responseAction = (Action<HttpResponseMessage>)this.Arguments[i];
                }
                else if (parmType == typeof(CancellationTokenSource))
                {
                    this.cancellationTokenSource = (CancellationTokenSource)this.Arguments[i];
                }
                else if (parmType == typeof(CancellationToken))
                {
                    this.cancellationToken = (CancellationToken)this.Arguments[i];
                }
                else if (parmType == typeof(IUriBuilder))
                {
                    uri = ((IUriBuilder)this.Arguments[i]).BuildUri().ToString();
                }
                else
                {
                    this.CheckParameterAttributes(
                        requestBuilder,
                        parms[i],
                        this.Arguments[i],
                        out string parmUri,
                        out formUrlContent,
                        out contentDisposition);

                    if (parmUri != null)
                    {
                        uri = parmUri;
                    }

                    if (formUrlContent == false &&
                        requestBuilder.IsContentSet == false &&
                        parms[i].ParameterType.IsModelObject() == true)
                    {
                        var serializer = this.options.GetObjectSerializer(this.contentType);
                        if (serializer == null)
                        {
                            throw new NotSupportedException($"Serializer for {this.contentType} not found");
                        }

                        var content = serializer.SerializeObject(this.Arguments[i]);

                        if (this.options.DebugOuputEnabled == true)
                        {
                            Console.WriteLine(content);
                        }

                        requestBuilder.SetContent(new StringContent(
                            content,
                            Encoding.UTF8,
                            this.contentType));
                    }
                }
            }

            return names;
        }

        /// <summary>
        /// Checks a parameter for attributes.
        /// </summary>
        /// <param name="requestBuilder">The request builder.</param>
        /// <param name="parm">The parameter.</param>
        /// <param name="argument">The parameters value.</param>
        /// <param name="uri">A variable to receive a Uri.</param>
        /// <param name="formUrlContent">A variable to receive the a value indicating whether or not there is form url content.</param>
        /// <param name="contentDisposition">A variable to receive the a value indicating whether or not there is content disposition.</param>
        internal void CheckParameterAttributes(
            HttpRequestBuilder requestBuilder,
            ParameterInfo parm,
            object argument,
            out string uri,
            out bool formUrlContent,
            out bool contentDisposition)
        {
            uri = null;
            formUrlContent = false;
            contentDisposition = false;

            var attrs = parm.GetCustomAttributes();
            if (attrs == null ||
                attrs.Any() == false ||
                argument == null)
            {
                return;
            }

/*
            if (requestBuilder.IsContentSet == true)
            {
                return;
            }
*/

            var urlAttr = attrs.OfType<UriAttribute>().FirstOrDefault();
            if (urlAttr != null)
            {
                uri = argument.ToString();
            }

            var sendAsAttr = attrs.OfType<SendAsContentAttribute>().FirstOrDefault();
            if (sendAsAttr != null)
            {
                if (typeof(HttpContent).IsAssignableFrom(parm.ParameterType) == true)
                {
                    requestBuilder.SetContent((HttpContent)argument);
                }
                else if (typeof(Stream).IsAssignableFrom(parm.ParameterType) == true)
                {
                    requestBuilder.SetContent(new StreamContent((Stream)argument));
                }
                else
                {
                    string contType = sendAsAttr.ContentType ?? this.contentType;
                    var serializer = this.options.GetObjectSerializer(contType);
                    if (serializer == null)
                    {
                        throw new NotSupportedException($"Serializer for {contType} not found");
                    }

                    requestBuilder.SetContent(new StringContent(
                        serializer.SerializeObject(argument),
                        sendAsAttr.Encoding ?? Encoding.UTF8,
                        serializer.ContentType));
                }

                return;
            }

            var formUrlAttr = attrs.OfType<SendAsFormUrlAttribute>().FirstOrDefault();
            if (formUrlAttr != null)
            {
                formUrlContent = true;
                var argType = argument.GetType();
                if (argument is Dictionary<string, string> strStrDictionayArgument)
                {
                    requestBuilder.SetContent(
                        new FormUrlEncodedContent(
                            (Dictionary<string, string>)argument));
                }
                else if (argument is Dictionary<string, object> strObjDictionaryArgument)
                {
                    requestBuilder.SetContent(
                        new FormUrlEncodedContent(
                            ((Dictionary<string, object>)argument)
                                .Select(
                                    kvp =>
                                    {
                                        return new KeyValuePair<string, string>(
                                            kvp.Key,
                                            kvp.Value?.ToString());
                                    })));
                }
                else if (parm.ParameterType.IsModelObject() == true)
                {
                    var list = new Dictionary<string, string>();
                    var properties = argument.GetType().GetProperties();
                    foreach (var property in properties)
                    {
                        list.Add(
                            property.Name,
                            property.GetValue(argument)?.ToString());
                    }

                    requestBuilder.SetContent(new FormUrlEncodedContent(list));
                }
                else
                {
                    requestBuilder.AddFormUrlProperty(
                        formUrlAttr.Name ?? parm.Name,
                        argument.ToString());
                }

                return;
            }

            var contentDispAttr = attrs.OfType<SendAsContentDispositionAttribute>().FirstOrDefault();
            if (contentDispAttr != null)
            {
                contentDisposition = true;
                requestBuilder.SetMultipartContent(true);

                if (contentDispAttr.IsName == true)
                {
                    requestBuilder.SetContentDispositionHeader(c => c.Name = (string)argument);
                }
                else if (contentDispAttr.IsFileName == true)
                {
                    requestBuilder.SetContentDispositionHeader(c => c.FileName = (string)argument);
                }
                else if (contentDispAttr.IsFileNameStar)
                {
                    requestBuilder.SetContentDispositionHeader(c => c.FileNameStar = (string)argument);
                }

                return;
            }

            requestBuilder
                .CheckParameterForSendAsQuery(attrs, parm, argument)
                .CheckParameterForSendAsHeader(attrs, argument);
        }

        /// <summary>
        /// Processes the result.
        /// </summary>
        /// <param name="response">A <see cref="HttpResponseMessage"/>.</param>
        /// <param name="returnType">The return type.</param>
        /// <returns>The result.</returns>
        internal async Task<object> ProcessResultAsync(
            HttpResponseMessage response,
            Type returnType)
        {
            object result = null;
            if (this.responseProcessorType != null)
            {
                return this.ExecuteResponseProcessor(
                    this.responseProcessorType,
                    returnType,
                    response);
            }

            if (this.responseFuncArg != -1 &&
                this.Arguments[this.responseFuncArg] != null)
            {
                var interceptorType = typeof(Func<,>).MakeGenericType(typeof(HttpResponseMessage), returnType);
                var interceptorMethod = interceptorType.GetMethod("Invoke", new[] { typeof(HttpResponseMessage) });
                return interceptorMethod.Invoke(this.Arguments[this.responseFuncArg], new object[] { response });
            }

            if (this.responseArg == -1 &&
                returnType != typeof(HttpResponseMessage))
            {
                response.EnsureSuccessStatusCode();
            }

            if (response.Content?.Headers?.ContentLength != 0)
            {
                if (returnType != typeof(HttpResponseMessage) &&
                    returnType != typeof(void))
                {
                    result = await this.DeserialiseObjectAsync(
                        response,
                        returnType,
                        this.MethodInfo.ReturnParameter);
                }

                if (this.dataArg != -1)
                {
                    this.Arguments[this.dataArg] = await this.DeserialiseObjectAsync(
                        response,
                        this.dataArgType,
                        this.MethodInfo.GetParameters()[this.dataArg]);
                }
            }

            if (this.responseArg != -1)
            {
                this.Arguments[this.responseArg] = response;
            }

            if (this.fromHeaders != null)
            {
                foreach (var item in this.fromHeaders)
                {
                    if (response.Headers.TryGetValues(item.Value, out IEnumerable<string> values) == true)
                    {
                        this.Arguments[item.Key] = values.FirstOrDefault();
                    }
                }
            }

            if (this.fromResponses != null)
            {
                string lastContentType = null;
                IObjectSerializer serializer = null;

                foreach (var fromResponse in this.fromResponses.OrderBy(a => a.Value.Item1.ContentType))
                {
                    string responseContentType = fromResponse.Value.Item1.ContentType ?? this.contentType;
                    if (responseContentType != lastContentType)
                    {
                        lastContentType = responseContentType;
                        serializer = this.GetObjectSerializer(responseContentType);
                    }

                    this.Arguments[fromResponse.Key] = fromResponse.Value.Item1.ToObject(response, fromResponse.Value.Item2, serializer);
                }
            }

            if (returnType == typeof(HttpResponseMessage))
            {
                return response;
            }

            if (result != null)
            {
                if (returnType.IsGenericType == true)
                {
                    if (returnType.IsSubclassOfGeneric(typeof(IEnumerable<>)) == true &&
                        typeof(IEnumerable<object>).IsAssignableFrom(returnType) == true)
                    {
                        var properties = returnType.GetGenericArguments()[0].GetProperties();
                        var responseProperty = properties?.FirstOrDefault(p => p.PropertyType == typeof(HttpResponseMessage));
                        var statusCodeProperty = properties?.FirstOrDefault(p => p.PropertyType == typeof(HttpStatusCode));
                        foreach (var item in (IEnumerable<object>)result)
                        {
                            responseProperty?.SetValue(item, response);
                            statusCodeProperty?.SetValue(item, response.StatusCode);
                        }
                    }
                }
                else
                {
                    result.SetObjectProperty<HttpResponseMessage>(response);
                    result.SetObjectProperty<HttpStatusCode>(response.StatusCode);
                }
            }

            return result;
        }

        /// <summary>
        /// Executes a response processor.
        /// </summary>
        /// <param name="responseProcessorType">The response processor type.</param>
        /// <param name="returnType">The return type.</param>
        /// <param name="response">The http response.</param>
        /// <returns>The response processors result.</returns>
        private object ExecuteResponseProcessor(
            Type responseProcessorType,
            Type returnType,
            HttpResponseMessage response)
        {
            var responseProcessor = this.options.Services?.GetService(responseProcessorType);
            if (responseProcessor == null)
            {
                responseProcessor = this.options.Services.CreateInstance(this.responseProcessorType);
            }

            if (responseProcessor != null)
            {
                var taskType = typeof(Task<>).MakeGenericType(returnType);
                var resultProperty = taskType.GetProperty("Result");
                var processMethod = this.responseProcessorType.GetMethod("ProcessResponseAsync", new[] { typeof(HttpResponseMessage) });
                var task = processMethod.Invoke(responseProcessor, new object[] { response });
                return resultProperty.GetValue(task);
            }

            return null;
        }

        /// <summary>
        /// Deserializes an object.
        /// </summary>
        /// <param name="response">The Http response.</param>
        /// <param name="dataType">The return data type.</param>
        /// <param name="parameterInfo">The parameter that the content is to returned via.</param>
        /// <returns>The deserialised object.</returns>
        private async Task<object> DeserialiseObjectAsync(
            HttpResponseMessage response,
            Type dataType,
            ParameterInfo parameterInfo)
        {
            if (response.Content.Headers.ContentLength == 0)
            {
                return null;
            }

            var responseContentType = response.Content.Headers.ContentType.MediaType;

            var attr = parameterInfo.GetCustomAttributes<FromResponseAttribute>()?.FirstOrDefault();
            if (attr != null)
            {
                if (attr.ContentType == null ||
                    attr.ContentType == responseContentType)
                {
                    var serializer = this.GetObjectSerializer(responseContentType);

                    return attr.ToObject(
                        response,
                        dataType,
                        serializer);
                }
            }

            var responseSerializer = this.GetObjectSerializer(responseContentType);
            var content = await response.Content
                .ReadAsStringAsync()
                .ConfigureAwait(false);

            return responseSerializer.DeserializeObject(content, dataType);
        }

        /// <summary>
        /// Gets the object serializer for a given content type.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        /// <returns>An object serializer.</returns>
        private IObjectSerializer GetObjectSerializer(string contentType)
        {
            var serializer = this.options.GetObjectSerializer(contentType);
            if (serializer == null)
            {
                throw new NotSupportedException($"Serializer for {contentType} not found");
            }

            return serializer;
        }

        /// <summary>
        /// Gets a model property.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="modelType">The model type.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The model property value.</returns>
        private object GetModelProperty(object model, Type modelType, string propertyName)
        {
            var property = modelType.GetProperty(propertyName);
            return property?.GetValue(model);
        }

        /// <summary>
        /// Invokes the request action if it has been defined.
        /// </summary>
        /// <param name="request">A <see cref="HttpRequestMessage"/>.</param>
        public void InvokeRequestAction(HttpRequestMessage request)
        {
            this.requestAction?.Invoke(request);

            this.options.RequestModifier?.ModifyRequest(request);
        }

        /// <summary>
        /// Invokes the response action if it has been defined.
        /// </summary>
        /// <param name="response">A <see cref="HttpResponseMessage"/>.</param>
        public void InvokeResponseAction(HttpResponseMessage response)
        {
            this.responseAction?.Invoke(response);
        }

        /// <summary>
        /// Sets the call timeout.
        /// </summary>
        /// <param name="timeout">The timeout value.</param>
        private void SetTimeout(TimeSpan timeout)
        {
            this.cancellationTokenSource = this.cancellationTokenSource ?? new CancellationTokenSource();
            this.cancellationTokenSource.CancelAfter(timeout);
        }

        /// <summary>
        /// Gets a cancellation token.
        /// </summary>
        /// <returns>The cancellation token.</returns>
        public CancellationToken GetCancellationToken()
        {
            if (this.cancellationTokenSource != null)
            {
                if (this.cancellationToken != CancellationToken.None)
                {
                    var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(this.cancellationToken, this.cancellationTokenSource.Token);
                    return linkedSource.Token;
                }

                return this.cancellationTokenSource.Token;
            }

            return this.cancellationToken;
        }
    }
}