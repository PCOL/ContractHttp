namespace ContractHttp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
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
        /// <returns>A <see cref="HttpRequestMessage"/>.</returns>
        private static HttpRequestMessage CreateRequest(System.Net.Http.HttpMethod method, string uri, string correlationId = null)
        {
            var request = new HttpRequestMessage(method, uri);

            if (string.IsNullOrEmpty(correlationId) == false)
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
            object returnObj = null;

            HttpCallMethod httpMethod = HttpCallMethod.HttpGet;
            string localBaseUri = this.GetBaseUri();
            string uri = localBaseUri;
            string contentType = "application/json";
            TimeSpan timeout = this.timeout;

            // Gets the http call contract attribute from the method.
            HttpCallContractAttribute attr = method.GetCustomAttribute<HttpCallContractAttribute>(false);
            if (attr != null)
            {
                httpMethod = attr.Method;
                uri = this.CombineUri(localBaseUri, attr.Uri);
                contentType = attr.ContentType;

                if (attr.Timeout.HasValue == true)
                {
                    timeout = attr.Timeout.Value;
                }
            }
            else
            {
                // Check for a http method attribute.
                HttpMethodAttribute methodAttr = method.GetCustomAttribute<HttpMethodAttribute>(false);
                if (methodAttr != null)
                {
                    string route;
                    this.GetMethodAndTemplateFromAttribute(methodAttr, out httpMethod, out route);
                    uri = this.CombineUri(localBaseUri, route);
                }
            }

            var client = this.GetHttpClient();
            //client.Timeout = timeout;

            if (httpMethod == HttpCallMethod.HttpGet)
            {
                returnObj = this.GetAsync(client, method, uri, arguments, contentType);
            }
            else if (httpMethod == HttpCallMethod.HttpPost)
            {
                returnObj = this.PostAsync(client, method, uri, arguments, contentType);
            }
            else if (httpMethod == HttpCallMethod.HttpPut)
            {
                ////returnObj = this.PutAsync(client, method, uri, arguments, contentType);
            }
            else if (httpMethod == HttpCallMethod.HttpPatch)
            {
            }
            else if (httpMethod == HttpCallMethod.HttpDelete)
            {
            }

            return returnObj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attrs"></param>
        /// <param name="httpMethod"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        private bool GetMethodAndTemplateFromAttribute(
            HttpMethodAttribute attr,
            out HttpCallMethod httpMethod,
            out string template)
        {
            httpMethod = HttpCallMethod.HttpGet;
            template = ((HttpMethodAttribute)attr).Template;

            if (attr is HttpGetAttribute)
            {
                httpMethod = HttpCallMethod.HttpGet;
            }
            else if (attr is HttpPostAttribute)
            {
                httpMethod = HttpCallMethod.HttpPost;
            }
            else if (attr is HttpPutAttribute)
            {
                httpMethod = HttpCallMethod.HttpPut;
            }
            else if (attr is HttpPatchAttribute)
            {
                httpMethod = HttpCallMethod.HttpPatch;
            }
            else if (attr is HttpDeleteAttribute)
            {
                httpMethod = HttpCallMethod.HttpDelete;
            }

            return true;
        }

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
        private string[] CheckArgs(
            MethodInfo method,
            out int contentArg,
            out int responseArg,
            out int dataArg,
            out Type dataArgType,
            out IDictionary<string, int> queryStrings,
            out IDictionary<string, int> headers)
        {
            contentArg = -1;
            responseArg = -1;
            dataArg = -1;
            dataArgType = null;
            queryStrings = null;
            headers = null;

            ParameterInfo[] parms = method.GetParameters();
            string[] names = new string[parms.Length];

            if (parms != null)
            {
                for (int i = 0; i < parms.Length; i++)
                {
                    names[i] = parms[i].Name;

                    if (parms[i].IsOut == true)
                    {
                        Type pType = parms[i].ParameterType.GetElementType();
                        if (pType == typeof(HttpResponseMessage))
                        {
                            responseArg = i;
                        }
                        else
                        {
                            dataArg = i;
                            dataArgType = pType;
                        }
                    }

                    var attrs = parms[i].GetCustomAttributes();
                    if (attrs != null &&
                        attrs.Any() == true)
                    {
                        if (contentArg == -1)
                        {
                            if (this.HasAttribute(attrs, typeof(SendAsContentAttribute), typeof(FromBodyAttribute)) == true)
                            {
                                contentArg = i;
                            }
                        }

                        foreach (var query in attrs.OfType<SendAsQueryAttribute>().Select(a => a.Name))
                        {
                            queryStrings = queryStrings ?? new Dictionary<string, int>();
                            queryStrings.Add(query, i);
                        }

                        foreach (var header in attrs.OfType<SendAsHeaderAttribute>().Select(a => a.Name))
                        {
                            headers = headers ?? new Dictionary<string, int>();
                            headers.Add(header, i);
                        }
                    }
                }
            }

            return names;
        }

        private void AddHeaders(HttpRequestMessage request, IDictionary<string, int> headers, object[] args)
        {
            if (headers == null)
            {
                return;
            }

            foreach (var item in headers)
            {
                if (args[item.Value] != null)
                {
                    request.Headers.TryAddWithoutValidation(item.Key, args[item.Value].ToString());
                }
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
        /// Executes a GET request for a method call.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> to use.</param>
        /// <param name="method">The methods info.</param>
        /// <param name="uri">The requests Uri.</param>
        /// <param name="inArgs">The method calls arguments.</param>
        /// <param name="contentType">The content type.</param>
        /// <returns>The result of the call.</returns>
        private object GetAsync(
            HttpClient client,
            MethodInfo method,
            string uri,
            object[] inArgs,
            string contentType)
        {
            int contentArg, responseArg, dataArg;
            Type dataArgType;
            IDictionary<string, int> queryStrings;
            IDictionary<string, int> headers;

            string[] names = this.CheckArgs(method, out contentArg, out responseArg, out dataArg, out dataArgType, out queryStrings, out headers);
            uri = this.ResolveUri(uri, names, inArgs);

            if (queryStrings != null)
            {
                var query = string.Join("&", queryStrings.Select(q => $"{q.Key}={inArgs[q.Value]}"));
                uri += $"?{query}";
            }

Console.WriteLine("Uri: {0}", uri);

            Type returnType = method.ReturnType;

            if (this.IsAsync(returnType) == true)
            {
                var genericReturnTypes = returnType.GetGenericArguments();
                if (genericReturnTypes[0] == typeof(HttpResponseMessage))
                {
                    var request = HttpClientProxy<T>.CreateRequest(System.Net.Http.HttpMethod.Get, uri);
                    request.Headers.Add("Accept", contentType);
                    this.AddHeaders(request, headers, inArgs);
                    return client.SendAsync(request);
                }

                Type asyncType = typeof(HttpClientProxy<>.AsyncCall<>).MakeGenericType(typeof(T), genericReturnTypes[0]);
                object obj = Activator.CreateInstance(asyncType, client);
                var mi = asyncType.GetMethod("GetAsync", new Type[] { typeof(string) });
                return mi.Invoke(obj, new object[] { uri });
            }

            var req = HttpClientProxy<T>.CreateRequest(System.Net.Http.HttpMethod.Get, uri);
            req.Headers.Add("Accept", contentType);
            this.AddHeaders(req, headers, inArgs);

Console.WriteLine("Send Request - {0}", req.Method);

            var result = client.SendAsync(req);

Console.WriteLine("Response: {0}", result.Result.StatusCode);
            return this.ProcessResult(result, returnType, inArgs, contentArg, responseArg, dataArg, dataArgType);
        }

        /// <summary>
        /// Executes a POST request for a method call.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> to use.</param>
        /// <param name="method">The method info.</param>
        /// <param name="uri">The request Uri.</param>
        /// <param name="inArgs">The method calls arguments.</param>
        /// <param name="contentType">The content type.</param>
        /// <returns>The result of the call.</returns>
        private object PostAsync(
            HttpClient client,
            MethodInfo method,
            string uri,
            object[] inArgs,
            string contentType)
        {
Console.WriteLine("PostAsync: {0}, {1}", uri, inArgs.Length);

            string[] names = this.CheckArgs(
                method,
                out int contentArg,
                out int responseArg,
                out int dataArg,
                out Type dataArgType,
                out IDictionary<string, int> queryStrings,
                out IDictionary<string, int> headers);

            uri = this.ResolveUri(uri, names, inArgs);

            if (queryStrings != null)
            {
                var query = string.Join("&", queryStrings.Select(q => $"{q.Key}={inArgs[q.Value]}"));
                uri += $"?{query}";
            }

Console.WriteLine("PostAsync: {0}, {1}, {2}, {3}", uri, contentArg, responseArg, dataArg);

            HttpContent content = null;
            if (contentArg != -1)
            {
                content = new StringContent(
                    JsonConvert.SerializeObject(inArgs[contentArg]),
                    Encoding.UTF8,
                    contentType);
            }

            Type returnType = method.ReturnType;

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = content;
            this.AddHeaders(request, headers, inArgs);

            Task<HttpResponseMessage> result = client.SendAsync(request);
            if (this.IsAsync(returnType) == true)
            {
                return result;
            }

            return this.ProcessResult(result, returnType, inArgs, contentArg, responseArg, dataArg, dataArgType);
        }

        /// <summary>
        /// Processes the result.
        /// </summary>
        /// <param name="result">A <see cref="Task"/> of type <see cref="HttpResponseMessage"/>.</param>
        /// <param name="returnType">The return type.</param>
        /// <param name="inArgs">The in arguments.</param>
        /// <param name="contentArg">The content argument index.</param>
        /// <param name="responseArg">The response argument index.</param>
        /// <param name="dataArg">The data argument index.</param>
        /// <param name="dataArgType">The data argument type.</param>
        /// <returns>The result.</returns>
        private object ProcessResult(
            Task<HttpResponseMessage> result,
            Type returnType,
            object[] inArgs,
            int contentArg,
            int responseArg,
            int dataArg,
            Type dataArgType)
        {
            HttpResponseMessage response = result.Result;

            if (responseArg != -1)
            {
                inArgs[responseArg] = response;
            }

            if (returnType == typeof(HttpResponseMessage))
            {
                return response;
            }
            else
            {
                response.EnsureSuccessStatusCode();
            }

            string content = response.Content.ReadAsStringAsync().Result;
            if (content.IsNullOrEmpty() == false)
            {
                if (returnType != typeof(HttpResponseMessage) &&
                    returnType != typeof(void))
                {
                    return JsonConvert.DeserializeObject(content, returnType);
                }
                else if (dataArg != -1)
                {
                    inArgs[dataArg] = JsonConvert.DeserializeObject(content, dataArgType);
                }
            }

            return null;
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
            if (type.GetTypeInfo().IsInterface == false)
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

            uri += path.TrimStart('/');

            return uri;
        }

        /// <summary>
        /// Represents a typed asynchronous call.
        /// </summary>
        /// <typeparam name="U">The calls data type.</typeparam>
        private class AsyncCall<U>
        {
            /// <summary>
            /// A reference to a client.
            /// </summary>
            private readonly HttpClient client;

            /// <summary>
            /// Initializes a new instance of the <see cref="AsyncCall{U}"/> class.
            /// </summary>
            /// <param name="client">The <see cref="HttpClient"/> to use.</param>
            public AsyncCall(HttpClient client)
            {
                this.client = client;
            }

            /// <summary>
            /// Represents a get call.
            /// </summary>
            /// <param name="uri">The uri to call.</param>
            /// <returns>A <see cref="Task"/>.</returns>
            public Task<U> GetAsync(string uri)
            {
                var request = HttpClientProxy<T>.CreateRequest(System.Net.Http.HttpMethod.Get, uri);
                return this.SendAsync(request);
            }

            /// <summary>
            /// Sends a request.
            /// </summary>
            /// <param name="request">The request to send.</param>
            /// <returns>A <see cref="Task"/>.</returns>
            public Task<U> SendAsync(HttpRequestMessage request)
            {
                Type dataType = typeof(U);
                Task<HttpResponseMessage> task = this.client.SendAsync(request);
                return task.ContinueWith<U>(
                    (t) =>
                    {
                        HttpResponseMessage response = ((Task<HttpResponseMessage>)t).Result;

                        response.EnsureSuccessStatusCode();
                        if (typeof(U) != typeof(void))
                        {
                            string json = response.Content.ReadAsStringAsync().Result;
                            object content = JsonConvert.DeserializeObject(json, dataType);

                            return (U)content;
                        }

                        return default(U);
                    });
            }
        }
    }
}
