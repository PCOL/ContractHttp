namespace ContractHttpTests.Resources
{
    using System.Net.Http;
    using ContractHttp;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Defines the test service with headers.
    /// </summary>
    [HttpClientContract(Route = "api/testheaders")]
    public interface ITestServiceWithHeaders
    {
        /// <summary>
        /// Get.
        /// </summary>
        /// <param name="header">A header value.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [Get]
        HttpResponseMessage Get([SendAsHeader("x-test-header")]string header);

        /// <summary>
        /// Get by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="header">A variable to receive the header value.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [Get("{name}")]
        HttpResponseMessage GetByName(string name, [FromHeader(Name = "x-test-header")]out string header);
    }
}