namespace ContractHttp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Text;
    using System.Threading;
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

            string route = method.GetHttpMethodAndTemplate(out HttpMethod httpAttrMethod);
            if (route != null)
            {
                uri = this.CombineUri(localBaseUri, route);
            }

            httpMethod = httpAttrMethod ?? httpMethod;

            try
            {
                return this.BuildAndSendRequestAsync(
                    method,
                    httpMethod,
                    uri,
                    arguments,
                    contentType,
                    timeout).Result;
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                {
                    Console.WriteLine(e.ToString());
                }

                throw ex.Flatten().InnerException;
            }
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
        /// Builds a request, sends it, and proecesses the response.
        /// </summary>
        /// <param name="method">The method info.</param>
        /// <param name="httpMethod">The http method.</param>
        /// <param name="uri">The request Uri.</param>
        /// <param name="inArgs">The method calls arguments.</param>
        /// <param name="contentType">The content type.</param>
        /// <returns>The result of the request.</returns>
        private Task<object> BuildAndSendRequestAsync(
            MethodInfo method,
            HttpMethod httpMethod,
            string uri,
            object[] inArgs,
            string contentType,
            TimeSpan? timeout)
        {
            var httpContext = new HttpRequestContext(method, inArgs, contentType, this.options);

            return httpContext.BuildAndSendRequestAsync(
                httpMethod,
                uri,
                contentType,
                timeout);
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
