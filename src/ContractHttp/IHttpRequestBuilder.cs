namespace ContractHttp
{
    using System.Net.Http;

    /// <summary>
    /// Defines the request builder interface.
    /// </summary>
    public interface IHttpRequestBuilder
    {
        /// <summary>
        /// Builds a <see cref="HttpRequestMessage"/> instance.
        /// </summary>
        /// <returns>A <see cref="HttpRequestMessage"/> instance.</returns>
        HttpRequestMessage Build();
    }
}