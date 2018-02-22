namespace ContractHttp
{
    using System;
    using System.Net.Http;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Http client proxy extension methods.
    /// </summary>
    public static class HttpClientProxyExtensionMethods
    {
        public static IServiceCollection AddHttpClient(
            this IServiceCollection services,
            HttpClient httpClient)
        {
            services.AddSingleton<HttpClient>(httpClient);
            return services;
        }

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
                        sp);

                    return proxy.GetProxyObject();
                });

            return services;
        }

        public static IServiceCollection AddAuthorizationHeaderFactory(
            this IServiceCollection services,
            string scheme,
            Func<string> getAuthHeaderValue)
        {
            services.AddSingleton<IAuthorizationHeaderFactory>(
                new AuthorizationHeaderFactory(scheme, getAuthHeaderValue));

            return services;
        }
    }
}
