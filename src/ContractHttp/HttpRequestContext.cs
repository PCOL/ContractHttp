namespace ContractHttp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

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
        /// A dictionary of from model parameters.
        /// </summary>
        private Dictionary<int, Tuple<string, Type>> fromModels;

        /// <summary>
        /// A cancellation token source.
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Initialises a new instance of the <see cref="HttpRequestContext"/> class.
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
        /// A reference to the method.
        /// </summary>
        public MethodInfo MethodInfo { get; }

        /// <summary>
        /// A reference to the methods arguments.
        /// </summary>
        public object[] Arguments { get; }


        /// <summary>
        /// Gets a value indicating whether or not content is expected.
        /// </summary>
        public bool IsContentExpected
        {
            get
            {
                return (this.MethodInfo.ReturnType != typeof(HttpResponseMessage) &&
                    this.MethodInfo.ReturnType != typeof(void)) ||
                    this.dataArg != -1;
            }
        }

        /// <summary>
        /// Builds a request, sends it, and proecesses the response.
        /// </summary>
        /// <param name="httpMethod">The http method.</param>
        /// <param name="uri">The request Uri.</param>
        /// <param name="contentType">The content type.</param>
        /// <returns>The result of the request.</returns>
        public async Task<object> BuildAndSendRequestAsync(
            HttpMethod httpMethod,
            string uri,
            string contentType,
            TimeSpan? timeout)
        {
            var requestBuilder = new HttpRequestBuilder()
                .SetMethod(httpMethod);

            var names = this.CheckArgsAndBuildRequest(requestBuilder);

            requestBuilder
                .AddMethodHeaders(this.MethodInfo, names, this.Arguments)
                .SetUri(uri.ExpandString(names, this.Arguments));

            Type returnType = this.MethodInfo.ReturnType;
            var authAttr = this.MethodInfo.GetCustomAttribute<AddAuthorizationHeaderAttribute>() ??
                this.MethodInfo.DeclaringType.GetCustomAttribute<AddAuthorizationHeaderAttribute>();

            if (authAttr != null)
            {
                if (authAttr.HeaderValue != null)
                {
                    requestBuilder.AddHeader(
                        "Authorization",
                        authAttr.HeaderValue.ExpandString(names, this.Arguments));
                }
                else
                {
                    var authFactoryType = authAttr.AuthorizationFactoryType ?? typeof(IAuthorizationHeaderFactory);
                    var authFactory = this.options.Services?.GetService(authFactoryType) as IAuthorizationHeaderFactory;
                    if (authFactory != null)
                    {
                        requestBuilder.AddAuthorizationHeader(
                            authFactory.GetAuthorizationHeaderScheme(),
                            authFactory.GetAuthorizationHeaderValue());
                    }
                }
            }

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

            var requestSender = new HttpRequestSender(
                this.options.GetHttpClient(),
                this);

            if (this.IsAsync(returnType, out Type taskType) == true &&
                taskType != typeof(void))
            {
                Type asyncType = typeof(AsyncCall<>).MakeGenericType(taskType);
                object obj = Activator.CreateInstance(asyncType, requestSender, this);
                var mi = asyncType.GetMethod("SendAsync", new Type[] { typeof(HttpRequestBuilder), typeof(HttpCompletionOption) });
                return mi.Invoke(obj, new object[] { requestBuilder, completionOption });
            }

            var response = await requestSender.SendAsync(
                requestBuilder,
                completionOption);

            string content = null;
            if (this.IsContentExpected == true)
            {
                content = await response.Content?.ReadAsStringAsync();
            }

            return this.ProcessResult(
                response,
                content,
                returnType);
        }


        /// <summary>
        /// Checks the arguments for specific ones.
        /// </summary>
        /// <param name="requestBuilder">A request builder.</param>
        /// <returns>An array of argument names.</returns>
        private string[] CheckArgsAndBuildRequest(
            HttpRequestBuilder requestBuilder)
        {
            var formUrlAttrs = this.MethodInfo.GetCustomAttributes<AddFormUrlEncodedPropertyAttribute>();
            if (formUrlAttrs.Any() == true)
            {
                foreach (var attr in formUrlAttrs)
                {
                    requestBuilder.AddFormUrlProperty(attr.Key, attr.Value);
                }
            }

            Type returnType = this.MethodInfo.ReturnType;
            if (this.IsAsync(returnType, out Type taskType) == true)
            {
                returnType = taskType;
            }

            bool formUrlContent = false;
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
                        var fromModel = parms[i].GetCustomAttribute<FromModelAttribute>();
                        if (fromModel != null)
                        {
                            this.fromModels = this.fromModels ?? new Dictionary<int, Tuple<string, Type>>();
                            this.fromModels.Add(i, Tuple.Create(fromModel.PropertyName, fromModel.ModelType));
                        }
                        else
                        {
                            var elemParmType = parms[i].ParameterType.GetElementType();
                            if (elemParmType == typeof(HttpResponseMessage))
                            {
                                responseArg = i;
                            }
                            else
                            {
                                dataArg = i;
                                dataArgType = parmType;
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
                else
                {
                    formUrlContent = this.CheckParameterAttributes(
                        requestBuilder,
                        parms[i],
                        this.Arguments[i]);

                    if (formUrlContent == false &&
                        requestBuilder.IsContentSet == false &&
                        this.IsModelObject(parms[i].ParameterType) == true)
                    {
                        var serializer = this.options.GetObjectSerializer(this.contentType);
                        if (serializer == null)
                        {
                            throw new NotSupportedException($"Serializer for {this.contentType} not found");
                        }

                        requestBuilder.SetContent(new StringContent(
                            serializer.SerializeObject(this.Arguments[i]),
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
        /// <returns>A value indicating whether or not there is form url content.</returns>
        private bool CheckParameterAttributes(
            HttpRequestBuilder requestBuilder,
            ParameterInfo parm,
            object argument)
        {
            bool formUrlContent = false;
            var attrs = parm.GetCustomAttributes();
            if (attrs != null &&
                attrs.Any() == true &&
                argument != null)
            {
                if (requestBuilder.IsContentSet == false)
                {
                    var sendAsAttr = attrs.OfType<SendAsContentAttribute>().FirstOrDefault();
                    if (sendAsAttr != null)
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
                    else
                    {
                        var formUrlAttr = attrs.OfType<SendAsFormUrlAttribute>().FirstOrDefault();
                        if (formUrlAttr != null)
                        {
                            formUrlContent = true;
                            var argType = argument.GetType();
                            if (typeof(Dictionary<string, string>).IsAssignableFrom(argType) == true)
                            {
                                requestBuilder.SetContent(
                                    new FormUrlEncodedContent(
                                        (Dictionary<string, string>)argument));
                            }
                            else if (typeof(Dictionary<string, object>).IsAssignableFrom(argType) == true)
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
                            else if (this.IsModelObject(parm.ParameterType) == true)
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
                        }
                    }
                }

                foreach (var query in attrs.OfType<SendAsQueryAttribute>())
                {
                    var name = query.Name.IsNullOrEmpty() == false ? query.Name : parm.Name;
                    var value = argument.ToString();
                    if (query.Format.IsNullOrEmpty() == false)
                    {
                        if (parm.ParameterType == typeof(short))
                        {
                            value = ((short)argument).ToString(query.Format);
                        }
                        else if (parm.ParameterType == typeof(int))
                        {
                            value = ((int)argument).ToString(query.Format);
                        }
                        else if (parm.ParameterType == typeof(long))
                        {
                            value = ((long)argument).ToString(query.Format);
                        }
                    }

                    if (query.Base64 == true)
                    {
                        value = Convert.ToBase64String(query.Encoding.GetBytes(value));
                    }

                    requestBuilder.AddQueryString(name, value);
                }

                foreach (var attr in attrs.OfType<SendAsHeaderAttribute>())
                {
                    if (string.IsNullOrEmpty(attr.Format) == false)
                    {
                        requestBuilder.AddHeader(
                            attr.Name,
                            string.Format(attr.Format, argument.ToString()));
                    }
                    else
                    {
                        requestBuilder.AddHeader(
                            attr.Name,
                            argument.ToString());
                    }
                }
            }

            return formUrlContent;
        }

        /// <summary>
        /// Checks if the type is a model object.
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>True if the type is a model object; otherwise false.</returns>
        private bool IsModelObject(Type type)
        {
            return type.IsPrimitive == false &&
                type.IsClass == true &&
                type != typeof(string);
        }

        /// <summary>
        /// Checks if the return type is a task.
        /// </summary>
        /// <param name="returnType">The return type.</param>
        /// <returns>True if the return type is a <see cref="Task"/> and therefore asynchronous; otherwise false.</returns>
        private bool IsAsync(Type returnType, out Type taskType)
        {
            taskType = null;
            if (typeof(Task).IsAssignableFrom(returnType) == true)
            {
                taskType = returnType.GetGenericArguments().FirstOrDefault() ?? typeof(void);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Processes the result.
        /// </summary>
        /// <param name="response">A <see cref="HttpResponseMessage"/>.</param>
        /// <param name="content">The response content.</param>
        /// <param name="returnType">The return type.</param>
        /// <returns>The result.</returns>
        internal object ProcessResult(
            HttpResponseMessage response,
            string content,
            Type returnType)
        {
            object result = null;
            if (content.IsNullOrEmpty() == false)
            {
                if (returnType != typeof(HttpResponseMessage) &&
                    returnType != typeof(void))
                {
                    result = this.DeserialiseObject(
                        content,
                        this.contentType,
                        returnType,
                        this.MethodInfo.ReturnParameter);
                }

                if (this.dataArg != -1)
                {
                    this.Arguments[this.dataArg] = this.DeserialiseObject(
                        content,
                        this.contentType,
                        this.dataArgType,
                        this.MethodInfo.GetParameters()[this.dataArg]);
                }
            }

            if (this.responseArg == -1 &&
                returnType != typeof(HttpResponseMessage))
            {
                response.EnsureSuccessStatusCode();
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

            if (this.fromModels != null &&
                content.IsNullOrEmpty() == false)
            {
                foreach (var item in this.fromModels)
                {
                    if (result != null &&
                        result.GetType() == item.Value.Item2)
                    {
                        this.Arguments[item.Key] = this.GetModelProperty(result, item.Value.Item2, item.Value.Item1);
                    }
                }
            }

            if (returnType == typeof(HttpResponseMessage))
            {
                return response;
            }

            if (this.responseFuncArg != -1 &&
                this.Arguments[this.responseFuncArg] != null)
            {
                var interceptorType = typeof(Func<,>).MakeGenericType(typeof(HttpResponseMessage), returnType);
                var interceptorMethod = interceptorType.GetMethod("Invoke", new[] { typeof(HttpResponseMessage) });
                result = interceptorMethod.Invoke(this.Arguments[this.responseFuncArg], new object[] {response });
            }

            return result;
        }

        /// <summary>
        /// Deserializes an object.
        /// </summary>
        /// <param name="content">The request content.</param>
        /// <param name="contType">The content Type.</param>
        /// <param name="dataType">The return data type.</param>
        /// <param name="parameterInfo">The parameter that the content is to returned via.</param>
        /// <returns></returns>
        private object DeserialiseObject(string content, string contType, Type dataType, ParameterInfo parameterInfo)
        {
            var serializer = this.options.GetObjectSerializer(contType);
            if (serializer == null)
            {
                throw new NotSupportedException($"Serializer for {contType} not found");
            }

            if (parameterInfo != null)
            {
                var fromJsonAttr = parameterInfo.GetCustomAttribute<FromJsonAttribute>();
                if (fromJsonAttr != null)
                {
                    return fromJsonAttr.ToObject(content, dataType);
                }

                var fromModelAttr = parameterInfo.GetCustomAttribute<FromModelAttribute>();
                if (fromModelAttr != null)
                {
                    var model = serializer.DeserializeObject(content, fromModelAttr.ModelType);
                    return this.GetModelProperty(model, fromModelAttr.ModelType, fromModelAttr.PropertyName);
                }
            }

            return serializer.DeserializeObject(content, dataType);
        }

        /// <summary>
        /// Gets a model property.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="modelType">The model type.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns></returns>
        private object GetModelProperty(object model, Type modelType, string propertyName)
        {
            var property = modelType.GetProperty(propertyName);
            return property?.GetValue(model);
        }

        /// <summary>
        /// Sends the request with retry if configured.
        /// </summary>
        /// <param name="httpClient">The http client to use.</param>
        /// <param name="requestBuilder">The http request builder.</param>
        /// <param name="completionOption">The completion option.</param>
        /// <returns></returns>
        internal async Task<HttpResponseMessage> SendAsync(
            HttpRequestBuilder requestBuilder,
            HttpCompletionOption completionOption)
        {
            var httpClient = this.options.GetHttpClient();

            var retryAttribute = this.MethodInfo.GetCustomAttribute<RetryAttribute>() ??
                this.MethodInfo.DeclaringType.GetCustomAttribute<RetryAttribute>();

            if (retryAttribute != null)
            {
                RetryHandler retry = new RetryHandler()
                    .RetryCount(retryAttribute.RetryCount)
                    .WaitTime(TimeSpan.FromMilliseconds(retryAttribute.WaitTime))
                    .MaxWaitTime(TimeSpan.FromMilliseconds(retryAttribute.MaxWaitTime))
                    .DoubleWaitTimeOnRetry(retryAttribute.DoubleWaitTimeOnRetry);

                return await retry.RetryAsync<HttpResponseMessage>(
                    () =>
                    {
                        return this.SendAsync(
                            httpClient,
                            requestBuilder.Build(),
                            completionOption);
                    },
                    (r) =>
                    {
                        if (retryAttribute.HttpStatusCodesToRetry != null)
                        {
                            return retryAttribute.HttpStatusCodesToRetry.Contains(r.StatusCode);
                        }

                        return false;
                    });
            }

            return await this.SendAsync(
                httpClient,
                requestBuilder.Build(),
                completionOption);
        }

        /// <summary>
        /// Sends the request to the service calling pre and post actions if they are configured. 
        /// </summary>
        /// <param name="httpClient">The http client to use.</param>
        /// <param name="request">The http request to send.</param>
        /// <param name="completionOption">The completion option.</param>
        /// <returns>A <see cref="HttpResponseMessage"/>.</returns>
        private async Task<HttpResponseMessage> SendAsync(
            HttpClient httpClient,
            HttpRequestMessage request,
            HttpCompletionOption completionOption)
        {

            this.InvokeRequestAction(request);

            var response = await httpClient.SendAsync(
                request,
                completionOption,
                this.GetCancellationToken());

            this.InvokeResponseAction(response);

            return response;
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
        /// Sets the call timeout
        /// </summary>
        /// <param name="timeout">The timeout value.</param>
        private void SetTimeout(TimeSpan timeout)
        {
            cancellationTokenSource = cancellationTokenSource ?? new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(timeout);
        }

        /// <summary>
        /// Gets a cancellation token.
        /// </summary>
        /// <returns>The cancellation token.</returns>
        public CancellationToken GetCancellationToken()
        {
            if (this.cancellationTokenSource != null)
            {
                return this.cancellationTokenSource.Token;
            }

            return CancellationToken.None;
        }
    }
}