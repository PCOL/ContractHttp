namespace ContractHttp
{
    using System;
    using System.Net.Http;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Represents client proxy options.
    /// </summary>
    public class HttpClientProxyOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientProxyOptions"/> class.
        /// </summary>
        public HttpClientProxyOptions()
        {
            this.ObjectSerializer = new TextJsonObjectSerializer();
        }

        /// <summary>
        /// Gets or sets the base uri.
        /// </summary>
        public string BaseUri { get; set; }

        /// <summary>
        /// Gets or sets the object serializer.
        /// </summary>
        public IObjectSerializer ObjectSerializer { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="HttpClient"/>.
        /// </summary>
        public HttpClient HttpClient { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IServiceProvider"/>.
        /// </summary>
        public IServiceProvider Services { get; set; }

        /// <summary>
        /// Gets or sets the timeout value.
        /// </summary>
        public TimeSpan? Timeout { get; set; }

        /// <summary>
        /// Gets or sets the authorization scheme.
        /// </summary>
        public string AuthorzationScheme { get; set; }

        /// <summary>
        /// Gets or sets the function used to obtain an authorization value.
        /// </summary>
        public Func<IServiceProvider, string> GetAuthorzationValue { get; set; }

        /// <summary>
        /// Gets or sets the function to obtain a correlation id.
        /// </summary>
        public Func<IServiceProvider, string> GetCorrelationId { get; set; }

        /// <summary>
        /// Gets or sets a request modifier.
        /// </summary>
        public IHttpRequestModifier RequestModifier { get; set; }

        /// <summary>
        /// Gets or sets the http version.
        /// </summary>
        public Version HttpVersion { get; set; } = new Version(1, 1);

        /// <summary>
        /// Gets or sets a value indicating whether or not debug output is on.
        /// </summary>
        public bool DebugOutputEnabled { get; set; }

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
                httpClient = httpClientFactory.GetClient();
                if (httpClient != null)
                {
                    return httpClient;
                }
            }

            return HttpClientSingleton.Instance;
        }

        /// <summary>
        /// Gets the object serializer.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        /// <returns>The object serializer.</returns>
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