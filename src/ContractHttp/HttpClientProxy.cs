namespace ContractHttp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using DynProxy;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a proxy class for accessing an Http end point.
    /// </summary>
    /// <typeparam name="T">The proxy type.</typeparam>
    public class HttpClientProxy<T>
        : Proxy<T>
    {
        /// <summary>
        /// A reference to the services.
        /// </summary>
        private readonly IServiceProvider services;

        /// <summary>
        /// The base uri.
        /// </summary>
        private string baseUri;

        /// <summary>
        /// The default timeout value.
        /// </summary>
        private TimeSpan timeout;

        /// <summary>
        /// The types client contract attribute.
        /// </summary>
        private HttpClientContractAttribute clientContractAttribute;

        /// <summary>
        /// The <see cref="HttpClient"/> to use.
        /// </summary>
        private HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientProxy{T}"/> class.
        /// </summary>
        /// <param name="baseUri">The base uri.</param>
        /// <param name="serviceProvider">The current dependency injection scope.</param>
        public HttpClientProxy(string baseUri, IServiceProvider serviceProvider = null)
            : this(baseUri, TimeSpan.FromMinutes(1), serviceProvider)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientProxy{T}"/> class.
        /// </summary>
        /// <param name="baseUri">The base uri.</param>
        /// <param name="timeout">The default timeout value.</param>
        /// <param name="serviceProvider">The current dependency injection scope.</param>
        public HttpClientProxy(string baseUri, TimeSpan timeout, IServiceProvider serviceProvider = null)
        {
            this.ThrowIfNotInterface(typeof(T), "T");
            Utility.ThrowIfArgumentNullOrEmpty(baseUri, nameof(baseUri));

            this.baseUri = baseUri;
            this.timeout = timeout;
            this.services = serviceProvider;

            this.CheckContractAttribute();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientProxy{T}"/> class.
        /// </summary>
        /// <param name="baseUri">The base uri.</param>
        /// <param name="httpClient">The <see cref="HttpClient"/> to use.</param>
        public HttpClientProxy(string baseUri, HttpClient httpClient = null)
            : this(baseUri, TimeSpan.FromMinutes(1), httpClient)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientProxy{T}"/> class.
        /// </summary>
        /// <param name="baseUri">The base uri.</param>
        /// <param name="timeout">The default timeout value.</param>
        /// <param name="httpClient">The <see cref="HttpClient"/> to use.</param>
        public HttpClientProxy(string baseUri, TimeSpan timeout, HttpClient httpClient = null)
        {
            this.ThrowIfNotInterface(typeof(T), "T");
            Utility.ThrowIfArgumentNullOrEmpty(baseUri, nameof(baseUri));

            this.baseUri = baseUri;
            this.timeout = timeout;
            this.httpClient = httpClient;

            this.CheckContractAttribute();
        }

        /// <summary>
        /// Checks the type for a contract attribute.
        /// </summary>
        private void CheckContractAttribute()
        {
            this.clientContractAttribute = typeof(T).GetCustomAttribute<HttpClientContractAttribute>();
            if (this.clientContractAttribute != null)
            {
                if (this.clientContractAttribute.Timeout.HasValue == true)
                {
                    this.timeout = this.clientContractAttribute.Timeout.Value;
                }

                if (this.baseUri != null &&
                    string.IsNullOrEmpty(this.clientContractAttribute.Route) == false)
                {
                    this.baseUri = this.CombineUri(this.baseUri, this.clientContractAttribute.Route);
                }
            }
        }

        /// <summary>
        /// Intercepts the invocation of methods on the proxied interface.
        /// </summary>
        /// <param name="method">The method being called.</param>
        /// <param name="arguments">The method arguments.</param>
        /// <returns>The return value.</returns>
        protected override object Invoke(MethodInfo method, object[] arguments)
        {
            return this.InvokeInternal(method, arguments);
        }

        /// <summary>
        /// Creates a <see cref="HttpRequestMessage"/> and adds headers for the correlation id and source id.
        /// </summary>
        /// <param name="method">The http method.</param>
        /// <param name="uri">The request uri.</param>
        /// <param name="content">The content.</param>
        /// <param name="contentType">The content type.</param>
        /// <returns>A <see cref="HttpRequestMessage"/>.</returns>
        internal static HttpRequestMessage CreateRequest(
            HttpMethod method,
            string uri,
            HttpContent content,
            string contentType,
            string correlationId = null)
        {
            var request = new HttpRequestMessage(method, uri);

            if (content != null)
            {
                request.Content = content;
            }

            if (contentType.IsNullOrEmpty() == false)
            {
                request.Headers.Add("Accept", contentType);
            }

            if (correlationId.IsNullOrEmpty() == false)
            {
                request.Headers.Add("X-Log-Correlation-Id", correlationId);
            }

            return request;
        }

        /// <summary>
        /// The proxy agnostic implementation of the invoke method.
        /// </summary>
        /// <param name="method">The method beign invoked.</param>
        /// <param name="arguments">The methods arguments.</param>
        /// <returns>The return value.</returns>
        private object InvokeInternal(MethodInfo method, object[] arguments)
        {
            string[] names = new string[arguments.Length];

            HttpMethod httpMethod = HttpMethod.Get;
            string localBaseUri = this.GetBaseUri();
            string uri = localBaseUri;
            string contentType = "application/json";
            TimeSpan timeout = this.timeout;

            // Gets the http call contract attribute from the method.
            var attr = method.GetCustomAttribute<HttpCallContractAttribute>(false);
            if (attr != null)
            {
                httpMethod = this.GetMethodFromAttribute(attr);
                uri = this.CombineUri(localBaseUri, attr.Uri);
                contentType = attr.ContentType;

                if (attr.Timeout.HasValue == true)
                {
                    timeout = attr.Timeout.Value;
                }
            }

            // Check for a http method attribute.
            HttpMethodAttribute methodAttr = method.GetCustomAttribute<HttpMethodAttribute>(false);
            if (methodAttr != null)
            {
                string route = this.GetMethodAndTemplateFromAttribute(methodAttr, ref httpMethod);
                uri = this.CombineUri(localBaseUri, route);
            }

            var client = this.GetHttpClient();
            //client.Timeout = timeout;

            return this.BuildAndSendRequest(client, method, httpMethod, uri, arguments, contentType);
        }

        /// <summary>
        /// Gets the <see cref="HttpMethod"/> from a <see cref="HttpCallContractAttribute"/>.
        /// </summary>
        /// <param name="attr">The <see cref="HttpCallContractAttribute"/></param>
        /// <returns>A <see cref="HttpMethod"/>.</returns>
        private HttpMethod GetMethodFromAttribute(HttpCallContractAttribute attr)
        {
            switch (attr.Method)
            {
                case HttpCallMethod.HttpPost:
                    return HttpMethod.Post;

                case HttpCallMethod.HttpPut:
                    return HttpMethod.Put;

                case HttpCallMethod.HttpPatch:
                    return new HttpMethod("Patch");

                case HttpCallMethod.HttpDelete:
                    return HttpMethod.Delete;
            }

            return HttpMethod.Get;
        }

        /// <summary>
        /// Gets the Http method and the template from a <see cref="HttpMethodAttribute"/>.
        /// </summary>
        /// <param name="attr">The <see cref="HttpMethodAttribute"/> instance.</param>
        /// <param name="httpMethod">A variable to receive the <see cref="HttpMethod"/>.</param>
        /// <returns>The attribute template.</returns>
        private string GetMethodAndTemplateFromAttribute(
            HttpMethodAttribute attr,
            ref HttpMethod httpMethod)
        {
            if (attr is HttpGetAttribute)
            {
                httpMethod = HttpMethod.Get;
            }
            else if (attr is HttpPostAttribute)
            {
                httpMethod = HttpMethod.Post;
            }
            else if (attr is HttpPutAttribute)
            {
                httpMethod = HttpMethod.Put;
            }
            else if (attr is HttpPatchAttribute)
            {
                httpMethod = new HttpMethod("Patch");
            }
            else if (attr is HttpDeleteAttribute)
            {
                httpMethod = HttpMethod.Delete;
            }

            return ((HttpMethodAttribute)attr).Template;
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
        /// Gets the base uri.
        /// </summary>
        /// <returns></returns>
        private string GetBaseUri()
        {
            if (this.baseUri != null)
            {
                return this.baseUri;
            }

            if (this.clientContractAttribute != null &&
                string.IsNullOrEmpty(this.clientContractAttribute.Route) == false)
            {
                return this.clientContractAttribute.Route;
            }

            return null;
        }

        /// <summary>
        /// Resolves a Uri from a name/value list.
        /// </summary>
        /// <param name="uri">The uri to resolve.</param>
        /// <param name="names">An array of names.</param>
        /// <param name="values">A matching array of values.</param>
        /// <returns>A <see cref="string"/> containing the resolved Uri.</returns>
        private string ResolveUri(string uri, string[] names, object[] values)
        {
            Utility.ThrowIfArgumentNullOrEmpty(uri, nameof(uri));

            int end = 0;
            int start = uri.IndexOf('{');
            if (start != -1)
            {
                string result = string.Empty;
                while (start != -1)
                {
                    result += uri.Substring(end, start - end);

                    end = uri.IndexOf('}', start);
                    if (end == -1)
                    {
                        throw new Exception();
                    }

                    object value = null;
                    string name = uri.Substring(start + 1, end - start - 1);

                    for (int i = 0; i < names.Length; i++)
                    {
                        if (names[i] == name)
                        {
                            value = values[i];
                            break;
                        }
                    }

                    if (value != null)
                    {
                        result += value.ToString();
                    }

                    start = uri.IndexOf('{', ++end);
                }

                result += uri.Substring(end);
                return result;
            }

            return uri;
        }

        /// <summary>
        /// Checks the arguments for specific ones.
        /// </summary>
        /// <param name="method">The method info.</param>
        /// <param name="contentArg">The index of the content argument, or -1 if one is not found.</param>
        /// <param name="responseArg">The index of the response argument, or -1 if one is not found.</param>
        /// <param name="dataArg">The index of the data argument, or -1 if one is not found.</param>
        /// <param name="dataArgType">The <see cref="Type"/> of the data argment, or null if one is not found.</param>
        /// <returns>A array of argument names.</returns>
        private HttpRequestMessage CheckArgsAndBuildRequest(
            HttpMethod httpMethod,
            string uri,
            string contentType,
            MethodInfo method,
            object[] inArgs,
            out int responseArg,
            out int dataArg,
            out Type dataArgType)
        {
            responseArg = -1;
            dataArg = -1;
            dataArgType = null;

            Dictionary<string, string> formUrl = null;
            Dictionary<string, string> queryStrings = null;
            Dictionary<string, string> headers = null;

            HttpContent content = null;

            var formUrlAttrs = method.GetCustomAttributes<AddFormUrlEncodedPropertyAttribute>();
            if (formUrlAttrs.Any() == true)
            {
                formUrl = formUrl ?? new Dictionary<string, string>();

                foreach (var attr in formUrlAttrs)
                {
                    formUrl.Add(attr.Key, attr.Value);
                }
            }

            ParameterInfo[] parms = method.GetParameters();
            string[] names = new string[parms.Length];

            if (parms != null)
            {
                for (int i = 0; i < parms.Length; i++)
                {
                    names[i] = parms[i].Name;

                    if (parms[i].IsOut == true)
                    {
                        Type parmType = parms[i].ParameterType.GetElementType();
                        if (parmType == typeof(HttpResponseMessage))
                        {
                            responseArg = i;
                        }
                        else
                        {
                            dataArg = i;
                            dataArgType = parmType;
                        }
                    }

                    var attrs = parms[i].GetCustomAttributes();
                    if (attrs != null &&
                        attrs.Any() == true &&
                        inArgs[i] != null)
                    {
                        if (content == null)
                        {
                            if (this.HasAttribute(attrs, typeof(SendAsContentAttribute), typeof(FromBodyAttribute)) == true)
                            {
                                content = new StringContent(
                                    JsonConvert.SerializeObject(inArgs[i]),
                                    Encoding.UTF8,
                                    contentType);
                            }

                            var formUrlAttr = attrs.OfType<SendAsFormUrlAttribute>().FirstOrDefault();
                            if (formUrlAttr != null)
                            {
                                formUrl = formUrl ?? new Dictionary<string, string>();

                                if (typeof(Dictionary<string, string>).IsAssignableFrom(inArgs[i].GetType()) == true)
                                {
                                    content = new FormUrlEncodedContent((Dictionary<string, string>)inArgs[i]);
                                }
                                else if (typeof(Dictionary<string, object>).IsAssignableFrom(inArgs[i].GetType()) == true)
                                {
                                    var list = ((Dictionary<string, object>)inArgs[i]).Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value?.ToString()));
                                    content = new FormUrlEncodedContent(list);
                                }
                                else if (this.IsModelObject(parms[i].ParameterType) == true)
                                {
                                    var list = new Dictionary<string, string>();
                                    var properties = inArgs[i].GetType().GetProperties();
                                    foreach (var property in properties)
                                    {
                                        list.Add(property.Name, property.GetValue(inArgs[i])?.ToString());
                                    }

                                    content = new FormUrlEncodedContent(list);
                                }
                                else
                                {
                                    formUrl.Add(formUrlAttr.Name ?? parms[i].Name, inArgs[i].ToString());
                                }
                            }
                        }

                        foreach (var query in attrs.OfType<SendAsQueryAttribute>().Select(a => a.Name))
                        {
                            queryStrings = queryStrings ?? new Dictionary<string, string>();
                            queryStrings.Add(query, inArgs[i].ToString());
                        }

                        foreach (var attr in attrs.OfType<SendAsHeaderAttribute>())
                        {
                            headers = headers ?? new Dictionary<string, string>();
                            if (string.IsNullOrEmpty(attr.Format) == false)
                            {
                                headers.Add(attr.Name, string.Format(attr.Format, inArgs[i].ToString()));
                            }
                            else
                            {
                                headers.Add(attr.Name, inArgs[i].ToString());
                            }
                        }
                    }

                    if (content == null &&
                        this.IsModelObject(parms[i].ParameterType) == true)
                    {
                        content = new StringContent(
                            JsonConvert.SerializeObject(inArgs[i]),
                            Encoding.UTF8,
                            contentType);
                    }
                }
            }

            if (content == null &&
                formUrl != null)
            {
                content = new FormUrlEncodedContent(formUrl);
            }

            // Build Uri
            uri = this.ResolveUri(uri, names, inArgs);
            if (queryStrings != null)
            {
                var query = string.Join("&", queryStrings.Select(q => $"{q.Key}={q.Value}"));
                uri += $"?{query}";
            }

            // Build request
            var request = new HttpRequestMessage(httpMethod, uri);
            request.Content = content;
            this.AddHeaders(request, method);
            this.AddHeaders(request, headers);

            return request;
        }

        private bool IsModelObject(Type type)
        {
            return type.IsPrimitive == false &&
                type.IsClass == true &&
                type != typeof(string);
        }

        private void AddHeaders(HttpRequestMessage request, MethodInfo method)
        {
            var headerAttrs = method
                .GetCustomAttributes<AddHeaderAttribute>()
                .Union(
                    method.DeclaringType.GetCustomAttributes<AddHeaderAttribute>());

            if (headerAttrs.Any() == true)
            {
                foreach (var attr in headerAttrs)
                {
                    request.Headers.TryAddWithoutValidation(attr.Header, attr.Value);
                }
            }
        }

        private void AddHeaders(HttpRequestMessage request, IDictionary<string, string> headers)
        {
            if (headers == null)
            {
                return;
            }

            foreach (var item in headers)
            {
                request.Headers.TryAddWithoutValidation(item.Key, item.Value.ToString());
            }
        }

        /// <summary>
        /// Checks if the return type is a task.
        /// </summary>
        /// <param name="returnType">The return type.</param>
        /// <returns>True if the return type is a <see cref="Task"/> and therefore asynchronous; otherwise false.</returns>
        private bool IsAsync(Type returnType)
        {
            return typeof(Task).IsAssignableFrom(returnType);
        }

        /// <summary>
        /// Builds a request, sends it, and proecesses the response.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> to use.</param>
        /// <param name="method">The method info.</param>
        /// <param name="httpMethod">The http method.</param>
        /// <param name="uri">The request Uri.</param>
        /// <param name="inArgs">The method calls arguments.</param>
        /// <param name="contentType">The content type.</param>
        /// <returns>The result of the request.</returns>
        private object BuildAndSendRequest(
            HttpClient client,
            MethodInfo method,
            HttpMethod httpMethod,
            string uri,
            object[] inArgs,
            string contentType)
        {
            var request = this.CheckArgsAndBuildRequest(
                httpMethod,
                uri,
                contentType,
                method,
                inArgs,
                out int responseArg,
                out int dataArg,
                out Type dataArgType);

            Type returnType = method.ReturnType;
            if (method.GetCustomAttribute<AddAuthorizationHeaderAttribute>() != null ||
                method.DeclaringType.GetCustomAttribute<AddAuthorizationHeaderAttribute>() != null)
            {
                var authFactory = this.services.GetService<IAuthorizationHeaderFactory>();
                if (authFactory != null)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue(
                        authFactory.GetAuthorizationHeaderScheme(),
                        authFactory.GetAuthorizationHeaderValue());
                }
            }

            if (this.IsAsync(returnType) == true)
            {
                var genericReturnTypes = returnType.GetGenericArguments();
                if (genericReturnTypes[0] == typeof(HttpResponseMessage))
                {
                    return client.SendAsync(request);
                }

                Type asyncType = typeof(AsyncCall<>).MakeGenericType(genericReturnTypes[0]);
                object obj = Activator.CreateInstance(asyncType, client);
                var mi = asyncType.GetMethod("SendAsync", new Type[] { typeof(HttpRequestMessage) });
                return mi.Invoke(obj, new object[] { request });
            }

            Task<HttpResponseMessage> result = client.SendAsync(request);
            return this.ProcessResult(result, returnType, inArgs, responseArg, dataArg, dataArgType);
        }

        /// <summary>
        /// Processes the result.
        /// </summary>
        /// <param name="responseTask">A <see cref="Task"/> of type <see cref="HttpResponseMessage"/>.</param>
        /// <param name="returnType">The return type.</param>
        /// <param name="inArgs">The in arguments.</param>
        /// <param name="responseArg">The response argument index.</param>
        /// <param name="dataArg">The data argument index.</param>
        /// <param name="dataArgType">The data argument type.</param>
        /// <returns>The result.</returns>
        private object ProcessResult(
            Task<HttpResponseMessage> responseTask,
            Type returnType,
            object[] inArgs,
            int responseArg,
            int dataArg,
            Type dataArgType)
        {
            HttpResponseMessage response = responseTask.Result;
            object result = null;

            string content = response.Content.ReadAsStringAsync().Result;
            if (content.IsNullOrEmpty() == false)
            {
                if (returnType != typeof(HttpResponseMessage) &&
                    returnType != typeof(void))
                {
                    result = JsonConvert.DeserializeObject(content, returnType);
                }
                else if (dataArg != -1)
                {
                    inArgs[dataArg] = JsonConvert.DeserializeObject(content, dataArgType);
                }
            }

            if (responseArg == -1 &&
                returnType != typeof(HttpResponseMessage))
            {
                response.EnsureSuccessStatusCode();
            }

            if (responseArg != -1)
            {
                inArgs[responseArg] = response;
            }

            if (returnType == typeof(HttpResponseMessage))
            {
                return response;
            }

            return result;
        }

        /// <summary>
        /// Gets a <see cref="HttpClient"/> instance.
        /// </summary>
        /// <returns>An <see cref="HttpClient"/> instance.</returns>
        private HttpClient GetHttpClient()
        {
            if (this.httpClient != null)
            {
                return this.httpClient;
            }

            // Can we get a client?
            var httpClient = this.services.GetService<HttpClient>();
            if (httpClient != null)
            {
                return httpClient;
            }

            // Can we get a client factory?
            var httpClientFactory = this.services.GetService<IHttpClientFactory>();
            if (httpClientFactory != null)
            {
                httpClient = httpClientFactory.CreateClient();
                if (httpClient != null)
                {
                    return httpClient;
                }
            }

            // Create a client.
            return new HttpClient();
        }

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> if the type is not an interface.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The types name.</param>
        private void ThrowIfNotInterface(Type type, string name)
        {
            if (type.IsInterface == false)
            {
                throw new InvalidOperationException(string.Format("{0} must be an interface", name));
            }
        }

        /// <summary>
        /// Combines a uri and a path.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="path">The path.</param>
        /// <returns>The combined uri.</returns>
        private string CombineUri(string uri, string path)
        {
            if (uri.EndsWith("/") == false)
            {
                uri += "/";
            }

            if (path.IsNullOrEmpty() == false)
            {
                uri += path.TrimStart('/');
            }

            return uri;
        }
    }
}
