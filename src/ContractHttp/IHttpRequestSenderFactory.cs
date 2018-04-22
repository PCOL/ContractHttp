namespace ContractHttp
{
    using System.Net.Http;

    /// <summary>
    /// Defines the <see cref="IHttpRequestSender"/> factory.
    /// </summary>
    public interface IHttpRequestSenderFactory
    {
        /// <summary>
        /// Creates a <see cref="IHttpRequestSender"/> instance.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> to use.</param>
        /// <param name="httpRequestContext">The current http request context.</param>
        /// <returns>A request sender.</returns>
        IHttpRequestSender CreateRequestSender(HttpClient httpClient, IHttpRequestContext httpRequestContext);
    }
}