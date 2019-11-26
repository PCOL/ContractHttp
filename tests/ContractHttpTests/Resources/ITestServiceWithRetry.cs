namespace ContractHttpTests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using ContractHttp;

    /// <summary>
    /// Defines a test service with retry.
    /// </summary>
    [Retry(RetryCount = 3, HttpStatusCodesToRetry = new[] { HttpStatusCode.BadGateway, (HttpStatusCode)429 })]
    [HttpClientContract(Route = "api")]
    public interface ITestServiceWithRetry
    {
        /// <summary>
        /// Get async.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [Get("{status}")]
        Task<HttpResponseMessage> GetAsync(int status);

        /// <summary>
        /// Get async.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="responseFunc">A function to build the return data from the response.</param>
        /// <returns>A value.</returns>
        [Get("{status}")]
        Task<bool> GetAsync(int status, Func<HttpResponseMessage, bool> responseFunc);

        /// <summary>
        /// Post async
        /// </summary>
        /// <param name="responseFunc">A response function.</param>
        /// <returns>True or false.</returns>
        [Post]
        Task<bool> PostAsync(Func<HttpResponseMessage, bool> responseFunc);
    }
}