namespace ContractHttp
{
    using System;
    using System.Net.Http;
    using Microsoft.Extensions.DependencyInjection;

    public class HttpClientProxyOptions
    {
        public HttpClientProxyOptions()
        {
            this.ObjectSerializer = new JsonObjectSerializer();
        }

        public IObjectSerializer ObjectSerializer { get; set; }

        public HttpClient HttpClient { get; set; }

        public IServiceProvider Services { get; set; }

        public TimeSpan? Timeout { get; set; }

        public string AuthorzationScheme { get; set; }

        public Func<IServiceProvider, string> GetAuthorzationValue { get; set; }

        public Func<IServiceProvider, string> GetCorrelationId { get; set; }


        /// <summary>
        /// Gets a <see cref="HttpClient"/> instance.
        /// </summary>
        /// <returns>An <see cref="HttpClient"/> instance.</returns>
        internal HttpClient GetHttpClient()
        {
            if (this.HttpClient != null)
            {
                return this.HttpClient;
            }

            // Can we get a client?
            var httpClient = this.Services?.GetService<HttpClient>();
            if (httpClient != null)
            {
                return httpClient;
            }

            // Can we get a client factory?
            var httpClientFactory = this.Services?.GetService<IHttpClientFactory>();
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
        /// Gets the object serializer.
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        internal IObjectSerializer GetObjectSerializer(string contentType)
        {
            var serializerFactory = this.Services?.GetService<Func<string, IObjectSerializer>>();
            var serializer = serializerFactory?.Invoke(contentType);
            if (serializer == null &&
                this.ObjectSerializer?.ContentType == contentType)
            {
                serializer = this.ObjectSerializer;
            }

            return serializer;
        }
    }
}