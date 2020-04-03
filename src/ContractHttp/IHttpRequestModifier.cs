namespace ContractHttp
{
    using System.Net.Http;

    /// <summary>
    /// Defines an interface for adding headers to a <see cref="HttpRequestMessage"/> instance.
    /// </summary>
    public interface IHttpRequestModifier
    {
        /// <summary>
        /// Allows for a request to be modified prior to being sent.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> object.</param>
        void ModifyRequest(HttpRequestMessage request);
    }
}
