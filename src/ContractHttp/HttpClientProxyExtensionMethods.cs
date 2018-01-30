namespace ContractHttp
{
    using System.Net.Http;
    using Microsoft.Extensions.DependencyInjection;

    public static class HttpClientProxyExtensionMethods
    {
        public static IServiceCollection AddHttpClient(
            this IServiceCollection builder,
            HttpClient httpClient)
        {
            builder.AddSingleton<HttpClient>(httpClient);
            return builder;
        }

        public static IServiceCollection AddHttpClientFactory(
            this IServiceCollection builder,
            IHttpClientFactory httpClientFactory)
        {
            builder.AddSingleton<IHttpClientFactory>(httpClientFactory);
            return builder;
        }
    }
}
