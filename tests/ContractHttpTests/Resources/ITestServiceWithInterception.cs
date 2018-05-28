namespace ContractHttpTests
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using ContractHttp;
    using ContractHttpTests.Resources.Models;

    /// <summary>
    /// Defines the test service with interception.
    /// </summary>
    [HttpClientContract(Route = "api/test", ContentType = "application/json")]
    public interface ITestServiceWithInterception
    {
        /// <summary>
        /// Get by name async.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="requestAction">An action to execute prior to sending the request.</param>
        /// <returns>A data model.</returns>
        [Get("{name}")]
        Task<TestData> GetAsync(string name, Action<HttpRequestMessage> requestAction);

        /// <summary>
        /// Get by name async.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="responseAction">An action to execute prior to returning the repsonse.</param>
        /// <returns>A result.</returns>
        [Get("{name}")]
        Task<TestData> GetAsync(string name, Action<HttpResponseMessage> responseAction);

        /// <summary>
        /// Get by name async.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="responseFunc">A function to execute to build the return type from the response.</param>
        /// <returns>A result.</returns>
        [Get("{name}")]
        Task<TestData> GetAsync(string name, Func<HttpResponseMessage, TestData> responseFunc);
    }
}