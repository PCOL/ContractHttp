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
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents a proxy class for accessing an Http end point.
    /// </summary>
    /// <typeparam name="T">The proxy type.</typeparam>
    public class HttpClientProxy<T>
        : Proxy<T>
    {
        /// <summary>
        /// The base uri.
        /// </summary>
        private string baseUri;

        /// <summary>
        /// The types client contract attribute.
        /// </summary>
        private HttpClientContractAttribute clientContractAttribute;

        private HttpClientProxyOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientProxy{T}"/> class.
        /// </summary>
        /// <param name="baseUri">The base uri.</param>
        /// <param name="options">Client proxy options.</param>
        public HttpClientProxy(string baseUri, HttpClientProxyOptions options = null)
        {
            this.ThrowIfNotInterface(typeof(T), "T");
            Utility.ThrowIfArgumentNullOrEmpty(baseUri, nameof(baseUri));

            this.baseUri = baseUri;
            this.options = options ?? new HttpClientProxyOptions();

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
                    this.options.Timeout = this.clientContractAttribute.Timeout.Value;
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
            TimeSpan? timeout = this.options.Timeout;

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

            var client = this.options.GetHttpClient();
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
        /// Expands a string from a name/value list.
        /// </summary>
        /// <param name="str">The str to expand.</param>
        /// <param name="names">An array of names.</param>
        /// <param name="values">A matching array of values.</param>
        /// <returns>The expanded <see cref="string"/>.</returns>
        private string ExpandString(string str, string[] names, object[] values)
        {
            Utility.ThrowIfArgumentNullOrEmpty(str, nameof(str));

            int end = 0;
            int start = str.IndexOf('{');
            if (start != -1)
            {
                string result = string.Empty;
                while (start != -1)
                {
                    result += str.Substring(end, start - end);

                    end = str.IndexOf('}', start);
                    if (end == -1)
                    {
                        throw new Exception();
                    }

                    object value = null;
                    string name = str.Substring(start + 1, end - start - 1);

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

                    start = str.IndexOf('{', ++end);
                }

                result += str.Substring(end);
                return result;
            }

            return str;
        }

        private void AddMethodHeaders(HttpRequestBuilder requestBuilder, MethodInfo method, string[] names, object[] values)
        {
            var headerAttrs = method
                .GetCustomAttributes<AddHeaderAttribute>()
                .Union(
                    method.DeclaringType.GetCustomAttributes<AddHeaderAttribute>());

            if (headerAttrs.Any() == true)
            {
                foreach (var attr in headerAttrs)
                {
                    requestBuilder.AddHeader(
                        attr.Header,
                        this.ExpandString(attr.Value, names, values));
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
            var serializer = this.options.GetObjectSerializer(contentType);
            if (serializer == null)
            {
                throw new NotSupportedException($"Serializer for {contentType} not found");
            }

            var httpContext = new HttpRequestContext(method, inArgs, serializer);

            var requestBuilder = new HttpRequestBuilder()
                .SetMethod(httpMethod);

            var names = httpContext.CheckArgsAndBuildRequest(requestBuilder);

            this.AddMethodHeaders(requestBuilder, method, names, inArgs);

            requestBuilder.SetUri(this.ExpandString(uri, names, inArgs));

            Type returnType = method.ReturnType;
            var authAttr = method.GetCustomAttribute<AddAuthorizationHeaderAttribute>() ??
                method.DeclaringType.GetCustomAttribute<AddAuthorizationHeaderAttribute>();

            if (authAttr != null)
            {
                if (authAttr.HeaderValue != null)
                {
                    requestBuilder.AddHeader(
                        "Authorization",
                        this.ExpandString(authAttr.HeaderValue, names, inArgs));
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

            var request = requestBuilder.Build();

            httpContext.InvokeRequestAction(request);

            if (this.IsAsync(returnType) == true)
            {
                var genericReturnTypes = returnType.GetGenericArguments();
                Type asyncType = typeof(AsyncCall<>).MakeGenericType(genericReturnTypes[0]);
                object obj = Activator.CreateInstance(asyncType, client, httpContext);
                var mi = asyncType.GetMethod("SendAsync", new Type[] { typeof(HttpRequestMessage) });
                return mi.Invoke(obj, new object[] { request });
            }

            HttpResponseMessage response = client.SendAsync(request).Result;

            httpContext.InvokeResponseAction(response);

            return httpContext.ProcessResult(
                response,
                returnType);
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
