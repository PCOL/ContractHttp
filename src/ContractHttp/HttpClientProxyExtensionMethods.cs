namespace ContractHttp
{
    using System;
    using System.Net.Http;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Http client proxy extension methods.
    /// </summary>
    public static class HttpClientProxyExtensionMethods
    {
        /// <summary>
        /// Adds a <see cref="HttpClient"/> singleton instance to dependency injection.
        /// </summary>
        /// <param name="services">A <see cref="IServiceCollection"/> instance.</param>
        /// <param name="httpClient">A <see cref="HttpClient"/> instance.</param>
        /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection AddHttpClient(
            this IServiceCollection services,
            HttpClient httpClient)
        {
            services.AddSingleton<HttpClient>(httpClient);
            return services;
        }

        /// <summary>
        /// Adds a <see cref="HttpClient"/> factory to dependency injection.
        /// </summary>
        /// <param name="services">A <see cref="IServiceCollection"/> instance.</param>
        /// <param name="httpClientFactory">A <see cref="HttpClient"/> factory.</param>
        /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection AddHttpClientFactory(
            this IServiceCollection services,
            IHttpClientFactory httpClientFactory)
        {
            services.AddSingleton<IHttpClientFactory>(httpClientFactory);
            return services;
        }

        /// <summary>
        /// Adds a <see cref="HttpClientProxy{T}"/> instance to a <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">A <see cref="IServiceCollection"/> instance.</param>
        /// <param name="baseUri">The base uri.</param>
        /// <param name="httpClient">A <see cref="HttpClient"/> instance.</param>
        /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection AddHttpClientProxy<T>(
            this IServiceCollection services,
            string baseUri,
            HttpClient httpClient)
            where T : class
        {
            services.AddHttpClientProxy<T>(baseUri);
            services.AddHttpClient(httpClient);
            return services;
        }

        /// <summary>
        /// Adds a <see cref="HttpClientProxy{T}"/> instance to a <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">A <see cref="IServiceCollection"/> instance.</param>
        /// <param name="baseUri">The base uri.</param>
        /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection AddHttpClientProxy<T>(
            this IServiceCollection services,
            string baseUri)
            where T : class
        {
            services.AddSingleton<T>(
                sp =>
                {
                    var proxy = new HttpClientProxy<T>(
                        baseUri,
                        new HttpClientProxyOptions()
                        {
                            Services = sp
                        });

                    return proxy.GetProxyObject();
                });

            return services;
        }

        /// <summary>
        /// Adds a <see cref="HttpClientProxy{T}"/> instance to a <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">A <see cref="IServiceCollection"/> instance.</param>
        /// <param name="baseUrlFunc">A function to return the base url.</param>
        /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection AddHttpClientProxy<T>(
            this IServiceCollection services,
            Func<IServiceProvider, string> baseUrlFunc)
            where T : class
        {
            services.AddSingleton<T>(
                sp =>
                {
                    var baseUri = baseUrlFunc(sp);
                    var proxy = new HttpClientProxy<T>(
                        baseUri,
                        new HttpClientProxyOptions()
                        {
                            Services = sp
                        });

                    return proxy.GetProxyObject();
                });

            return services;
        }

        /// <summary>
        /// Adds an authorization header factory.
        /// </summary>
        /// <param name="services">A <see cref="IServiceCollection"/> instance.</param>
        /// <param name="scheme">The Authorization header scheme.</param>
        /// <param name="getAuthHeaderValue">A function to get the authorization header value.</param>
        /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection AddAuthorizationHeaderFactory(
            this IServiceCollection services,
            string scheme,
            Func<IServiceProvider, string> getAuthHeaderValue)
        {
            services.AddScoped<IAuthorizationHeaderFactory>(
                sp =>
                {
                    return new AuthorizationHeaderFactory(
                        scheme,
                        () =>
                        {
                            return getAuthHeaderValue?.Invoke(sp);
                        });
                });

            return services;
        }

        /// <summary>
        /// Gets a http method and path template from the <see cref="MethodInfo"/> instance.
        /// </summary>
        /// <param name="methodInfo">A <see cref="MethodInfo"/> instance.</param>
        /// <param name="httpMethod">A variable to receive the http method.</param>
        /// <returns>The path template; otherwise null.</returns>
        public static string GetHttpMethodAndTemplate(this MethodInfo methodInfo, out HttpMethod httpMethod)
        {
            httpMethod = null;

            // Check for a http method attribute.
            var httpMethodAttr = methodInfo.GetCustomAttribute<HttpMethodAttribute>(true);
            if (httpMethodAttr != null)
            {
                return httpMethodAttr.GetMethodAndTemplateFromAttribute(out httpMethod);
            }

            // Check for a method attribute.
            var methodAttr = methodInfo.GetCustomAttribute<MethodAttribute>(true);
            if (methodAttr != null)
            {
                return methodAttr.GetMethodAndTemplateFromAttribute(out httpMethod);
            }

            return null;
        }

        /// <summary>
        /// Gets the Http method and the template from a <see cref="HttpMethodAttribute"/>.
        /// </summary>
        /// <param name="attr">The <see cref="HttpMethodAttribute"/> instance.</param>
        /// <param name="httpMethod">A variable to receive the <see cref="HttpMethod"/>.</param>
        /// <returns>The attribute template.</returns>
        private static string GetMethodAndTemplateFromAttribute(
            this HttpMethodAttribute attr,
            out HttpMethod httpMethod)
        {
            httpMethod = null;

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
        /// Gets the Http method and the template from a <see cref="MethodAttribute"/>.
        /// </summary>
        /// <param name="attr">The <see cref="MethodAttribute"/> instance.</param>
        /// <param name="httpMethod">A variable to receive the <see cref="HttpMethod"/>.</param>
        /// <returns>The attribute template.</returns>
        private static string GetMethodAndTemplateFromAttribute(
            this MethodAttribute attr,
            out HttpMethod httpMethod)
        {
            httpMethod = null;

            if (attr is GetAttribute)
            {
                httpMethod = HttpMethod.Get;
            }
            else if (attr is PostAttribute)
            {
                httpMethod = HttpMethod.Post;
            }
            else if (attr is PutAttribute)
            {
                httpMethod = HttpMethod.Put;
            }
            else if (attr is PatchAttribute)
            {
                httpMethod = new HttpMethod("Patch");
            }
            else if (attr is DeleteAttribute)
            {
                httpMethod = HttpMethod.Delete;
            }

            return ((MethodAttribute)attr).Template;
        }
    }
}
