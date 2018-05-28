namespace ContractHttp
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the request sender interface.
    /// </summary>
    public interface IHttpRequestSender
    {
        /// <summary>
        /// Sends a request to the server.
        /// </summary>
        /// <param name="requestBuilder">A request builder.</param>
        /// <param name="completionOption">A completion option.</param>
        /// <returns>A <see cref="HttpResponseMessage"/> instance.</returns>
        Task<HttpResponseMessage> SendAsync(
            IHttpRequestBuilder requestBuilder,
            HttpCompletionOption completionOption);
    }
}