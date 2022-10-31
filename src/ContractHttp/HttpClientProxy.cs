namespace ContractHttp
{
    using System;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;
    using DynProxy;

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
        /// The base uri path.
        /// </summary>
        private string baseUriPath;

        /// <summary>
        /// The types client contract attribute.
        /// </summary>
        private HttpClientContractAttribute clientContractAttribute;

        /// <summary>
        /// The client proxy options.
        /// </summary>
        private HttpClientProxyOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientProxy{T}"/> class.
        /// </summary>
        /// <param name="options">Client proxy options.</param>
        public HttpClientProxy(HttpClientProxyOptions options)
        {
            Utility.ThrowIfNotInterface(typeof(T), "T");
            Utility.ThrowIfArgumentNull(options, nameof(options));

            this.options = options ?? new HttpClientProxyOptions();
            this.baseUri = options.BaseUri;

            this.CheckContractAttribute();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientProxy{T}"/> class.
        /// </summary>
        /// <param name="baseUri">The base uri.</param>
        /// <param name="options">Client proxy options.</param>
        public HttpClientProxy(string baseUri, HttpClientProxyOptions options = null)
        {
            Utility.ThrowIfNotInterface(typeof(T), "T");
            Utility.ThrowIfArgumentNullOrEmpty(baseUri, nameof(baseUri));

            this.options = options ?? new HttpClientProxyOptions();
            this.baseUri = baseUri ?? options.BaseUri;

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

                if (string.IsNullOrEmpty(this.clientContractAttribute.Route) == false)
                {
                    this.baseUriPath = this.clientContractAttribute.Route;
                }
            }
        }

        /// <summary>
        /// Intercepts the invocation of methods on the proxied interface.
        /// </summary>
        /// <param name="methodInfo">The method being called.</param>
        /// <param name="arguments">The method arguments.</param>
        /// <returns>The return value.</returns>
        protected override object Invoke(MethodInfo methodInfo, object[] arguments)
        {
            try
            {
                return this.InvokeInternalAsync(methodInfo, arguments).Result;
            }
            catch (AggregateException ex)
            {
                throw ex.Flatten().InnerException;
            }
        }

        /// <summary>
        /// Intercepts the invocation of async methods on the proxied interface.
        /// </summary>
        /// <param name="methodInfo">The method being called.</param>
        /// <param name="arguments">The method arguments.</param>
        /// <returns>The return value.</returns>
        protected override Task<object> InvokeAsync(MethodInfo methodInfo, object[] arguments)
        {
            return this.InvokeInternalAsync(methodInfo, arguments);
        }

        /// <summary>
        /// The proxy agnostic implementation of the invoke method.
        /// </summary>
        /// <param name="method">The method beign invoked.</param>
        /// <param name="arguments">The methods arguments.</param>
        /// <returns>The return value.</returns>
        private async Task<object> InvokeInternalAsync(MethodInfo method, object[] arguments)
        {
            string[] names = new string[arguments.Length];

            HttpMethod httpMethod = HttpMethod.Get;
            string uri = this.baseUriPath ?? string.Empty;
            string contentType = "application/json";
            TimeSpan? timeout = this.options.Timeout;

            // Gets the http call contract attribute from the method.
            var attr = method.GetCustomAttribute<HttpCallContractAttribute>(false);
            if (attr != null)
            {
                httpMethod = this.GetMethodFromAttribute(attr);
                uri = Utility.CombineUri(uri, attr.Uri);
                contentType = attr.ContentType;

                if (attr.Timeout.HasValue == true)
                {
                    timeout = attr.Timeout.Value;
                }
            }

            string route = method.GetHttpMethodAndTemplate(out HttpMethod httpAttrMethod);
            if (route != null)
            {
                uri = Utility.CombineUri(uri, route);
            }

            httpMethod = httpAttrMethod ?? httpMethod;

            return await this.BuildAndSendRequestAsync(
                method,
                httpMethod,
                uri,
                arguments,
                contentType,
                timeout);
        }

        /// <summary>
        /// Gets the <see cref="HttpMethod"/> from a <see cref="HttpCallContractAttribute"/>.
        /// </summary>
        /// <param name="attr">The <see cref="HttpCallContractAttribute"/>.</param>
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
        /// Gets the base uri.
        /// </summary>
        /// <returns>The base uri.</returns>
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
        /// Builds a request, sends it, and proecesses the response.
        /// </summary>
        /// <param name="method">The method info.</param>
        /// <param name="httpMethod">The http method.</param>
        /// <param name="uriPath">The Uri path.</param>
        /// <param name="inArgs">The method calls arguments.</param>
        /// <param name="contentType">The content type.</param>
        /// <param name="timeout">The request timeout value.</param>
        /// <returns>The result of the request.</returns>
        private Task<object> BuildAndSendRequestAsync(
            MethodInfo method,
            HttpMethod httpMethod,
            string uriPath,
            object[] inArgs,
            string contentType,
            TimeSpan? timeout)
        {
            var httpContext = new HttpRequestContext(method, inArgs, contentType, this.options);

            return httpContext.BuildAndSendRequestAsync(
                httpMethod,
                this.baseUri,
                uriPath,
                contentType,
                timeout);
        }
    }
}
