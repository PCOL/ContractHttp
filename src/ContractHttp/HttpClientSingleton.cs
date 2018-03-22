namespace ContractHttp
{
    using System;
    using System.Net.Http;

    /// <summary>
    /// Represents a <see cref="HttpClient"/> singleton.
    /// </summary>
    public class HttpClientSingleton
    {
        private static Lazy<HttpClient> instance =
            new Lazy<HttpClient>(() => new HttpClient(), true);

        /// <summary>
        /// Gets the <see cref="HttpClient"/> singleton.
        /// </summary>
        public static HttpClient Instance => instance.Value;
    }
}