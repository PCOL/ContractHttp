namespace ContractHttpTests.Resources
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using ContractHttp;

    /// <summary>
    /// Defines the dynamic uri client.
    /// </summary>
    [HttpClientContract(Route = "api/test")]
    public interface IDynamicUriClient
    {
        /// <summary>
        /// Gets a widget.
        /// </summary>
        /// <param name="url">The base url to use.</param>
        /// <param name="id">The id of the widget.</param>
        /// <returns>A response.</returns>
        [Get("widgets/{id}")]
        Task<HttpResponseMessage> GetWidgetAsync([Uri]string url, string id);

        /// <summary>
        /// Gets a widget.
        /// </summary>
        /// <param name="url">The base url to use.</param>
        /// <param name="id">The id of the widget.</param>
        /// <returns>A response.</returns>
        [Get("widgets/{id}")]
        Task<HttpResponseMessage> GetWidgetAsync(IUriBuilder url, string id);
    }
}