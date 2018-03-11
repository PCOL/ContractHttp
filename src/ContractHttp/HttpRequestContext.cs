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
    {
        /// <summary>
        /// A reference to the method.
        /// </summary>
        private readonly MethodInfo methodInfo;

        /// <summary>
        /// A reference to the methods arguments.
        /// </summary>
        private readonly object[] arguments;

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
            this.methodInfo = methodInfo;
            this.arguments = arguments;
            this.contentType = contentType;
            this.options = options;
        }

        /// <summary>
        /// Gets or sets the cancellation token.
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// Gets a value indicating whether or not content is expected.
        /// </summary>
        public bool IsContentExpected
        {
            get
            {
                return (this.methodInfo.ReturnType != typeof(HttpResponseMessage) &&
                    this.methodInfo.ReturnType != typeof(void)) ||
                    this.dataArg != -1;
            }
        }

        /// <summary>
        /// Checks the arguments for specific ones.
        /// </summary>
        /// <param name="requestBuilder">A request builder.</param>
        /// <returns>An array of argument names.</returns>
        public string[] CheckArgsAndBuildRequest(
            HttpRequestBuilder requestBuilder)
        {
            var formUrlAttrs = this.methodInfo.GetCustomAttributes<AddFormUrlEncodedPropertyAttribute>();
            if (formUrlAttrs.Any() == true)
            {
                foreach (var attr in formUrlAttrs)
                {
                    requestBuilder.AddFormUrlProperty(attr.Key, attr.Value);
                }
            }

            Type returnType = this.methodInfo.ReturnType;
            if (this.IsAsync(returnType) == true)
            {
                returnType = returnType.GetGenericArguments()[0];
            }

            bool formUrlContent = false;
            ParameterInfo[] parms = this.methodInfo.GetParameters();
            string[] names = new string[parms.Length];

            if (parms != null)
            {
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

                        var fromModel = parms[i].GetCustomAttribute<FromModelAttribute>();
                        if (fromModel != null)
                        {
                            this.fromModels = this.fromModels ?? new Dictionary<int, Tuple<string, Type>>();
                            this.fromModels.Add(i, Tuple.Create(fromModel.PropertyName, fromModel.ModelType));
                        }

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

                        continue;
                    }

                    if (parmType == typeof(Func<,>).MakeGenericType(typeof(HttpResponseMessage), returnType))
                    {
                        this.responseFuncArg = i;
                        continue;
                    }

                    if (parmType == typeof(Action<HttpRequestMessage>))
                    {
                        this.requestAction = (Action<HttpRequestMessage>)this.arguments[i];
                        continue;
                    }

                    if (parmType == typeof(Action<HttpResponseMessage>))
                    {
                        this.responseAction = (Action<HttpResponseMessage>)this.arguments[i];
                        continue;
                    }

                    if (parmType == typeof(CancellationToken))
                    {
                        this.CancellationToken = (CancellationToken)this.arguments[i];
                    }

                    var attrs = parms[i].GetCustomAttributes();
                    if (attrs != null &&
                        attrs.Any() == true &&
                        this.arguments[i] != null)
                    {
                        if (requestBuilder.IsContentSet == false)
                        {
                            //if (this.HasAttribute(attrs, typeof(SendAsContentAttribute), typeof(FromBodyAttribute)) == true)

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
                                    serializer.SerializeObject(this.arguments[i]),
                                    sendAsAttr.Encoding ?? Encoding.UTF8,
                                    serializer.ContentType));
                            }

                            var formUrlAttr = attrs.OfType<SendAsFormUrlAttribute>().FirstOrDefault();
                            if (formUrlAttr != null)
                            {
                                formUrlContent = true;
                                var argType = this.arguments[i].GetType();
                                if (typeof(Dictionary<string, string>).IsAssignableFrom(argType) == true)
                                {
                                    requestBuilder.SetContent(new FormUrlEncodedContent((Dictionary<string, string>)this.arguments[i]));
                                }
                                else if (typeof(Dictionary<string, object>).IsAssignableFrom(argType) == true)
                                {
                                    var list = ((Dictionary<string, object>)this.arguments[i]).Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value?.ToString()));
                                    requestBuilder.SetContent(new FormUrlEncodedContent(list));
                                }
                                else if (this.IsModelObject(parmType) == true)
                                {
                                    var list = new Dictionary<string, string>();
                                    var properties = this.arguments[i].GetType().GetProperties();
                                    foreach (var property in properties)
                                    {
                                        list.Add(
                                            property.Name,
                                            property.GetValue(this.arguments[i])?.ToString());
                                    }

                                    requestBuilder.SetContent(new FormUrlEncodedContent(list));
                                }
                                else
                                {
                                    requestBuilder.AddFormUrlProperty(
                                        formUrlAttr.Name ?? parms[i].Name,
                                        this.arguments[i].ToString());
                                }
                            }
                        }

                        foreach (var query in attrs.OfType<SendAsQueryAttribute>())
                        {
                            var name = query.Name.IsNullOrEmpty() == false ? query.Name : parms[i].Name;
                            var value = this.arguments[i].ToString();
                            if (query.Format.IsNullOrEmpty() == false)
                            {
                                if (parmType == typeof(short))
                                {
                                    value = ((short)this.arguments[i]).ToString(query.Format);
                                }
                                else if (parmType == typeof(int))
                                {
                                    value = ((int)this.arguments[i]).ToString(query.Format);
                                }
                                else if (parmType == typeof(long))
                                {
                                    value = ((long)this.arguments[i]).ToString(query.Format);
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
                                    string.Format(attr.Format, this.arguments[i].ToString()));
                            }
                            else
                            {
                                requestBuilder.AddHeader(
                                    attr.Name,
                                    this.arguments[i].ToString());
                            }
                        }
                    }

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
                            serializer.SerializeObject(this.arguments[i]),
                            Encoding.UTF8,
                            this.contentType));
                    }
                }
            }

            return names;
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
        public bool IsAsync(Type returnType)
        {
            return typeof(Task).IsAssignableFrom(returnType);
        }

        /// <summary>
        /// Checks if a list of attributes contains any of a provided list.
        /// </summary>
        /// <param name="attrs">The attribute listto check.</param>
        /// <param name="attrTypes">The attributes to check for.</param>
        /// <returns>True if any are found; otherwise false.</returns>
        private bool HasAttribute(IEnumerable<Attribute> attrs, params Type[] attrTypes)
        {
            foreach (var attr in attrs)
            {
                if (attrTypes.FirstOrDefault(t => t == attr.GetType()) != null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Processes the result.
        /// </summary>
        /// <param name="response">A <see cref="HttpResponseMessage"/>.</param>
        /// <param name="returnType">The return type.</param>
        /// <returns>The result.</returns>
        public object ProcessResult(
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
                        this.methodInfo.ReturnParameter);
                }

                if (this.dataArg != -1)
                {
                    this.arguments[this.dataArg] = this.DeserialiseObject(
                        content,
                        this.contentType,
                        this.dataArgType,
                        this.methodInfo.GetParameters()[this.dataArg]);
                }
            }

            if (this.responseArg == -1 &&
                returnType != typeof(HttpResponseMessage))
            {
                response.EnsureSuccessStatusCode();
            }

            if (this.responseArg != -1)
            {
                this.arguments[this.responseArg] = response;
            }

            if (this.fromHeaders != null)
            {
                foreach (var item in this.fromHeaders)
                {
                    if (response.Headers.TryGetValues(item.Value, out IEnumerable<string> values) == true)
                    {
                        this.arguments[item.Key] = values.FirstOrDefault();
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
                        arguments[item.Key] = this.GetModelProperty(result, item.Value.Item2, item.Value.Item1);
                    }
                }
            }

            if (returnType == typeof(HttpResponseMessage))
            {
                return response;
            }

            if (this.responseFuncArg != -1 &&
                this.arguments[this.responseFuncArg] != null)
            {
                var interceptorType = typeof(Func<,>).MakeGenericType(typeof(HttpResponseMessage), returnType);
                var interceptorMethod = interceptorType.GetMethod("Invoke", new[] { typeof(HttpResponseMessage) });
                result = interceptorMethod.Invoke(this.arguments[this.responseFuncArg], new object[] {response });
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
                    return fromJsonAttr.JsonToObject(content, dataType);
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
        }

        /// <summary>
        /// Invokes the response action if it has been defined.
        /// </summary>
        /// <param name="response">A <see cref="HttpResponseMessage"/>.</param>
        public void InvokeResponseAction(HttpResponseMessage response)
        {
            this.responseAction?.Invoke(response);
        }
    }
}