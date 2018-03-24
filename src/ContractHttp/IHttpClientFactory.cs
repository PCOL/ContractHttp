namespace ContractHttp
{
    using System.Net.Http;

    /// <summary>
    /// Defines a <see cref="HttpClient"/> factory interface.
    /// </summary>
    public interface IHttpClientFactory
    {
        /// <summary>
        /// Gets a <see cref="HttpClient"/> instance.
        /// </summary>
        /// <returns>A <see cref="HttpClient"/> instance.</returns>
        HttpClient GetClient();
    }
}