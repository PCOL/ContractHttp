namespace ContractHttpTests.Resources
{
    using System.Collections.Generic;
    using System.Net.Http;
    using ContractHttp;

    /// <summary>
    /// Defines the test service with headers.
    /// </summary>
    [HttpClientContract(Route = "api/testquery")]
    public interface ITestServiceWithQueryParameters
    {
        /// <summary>
        /// Get.
        /// </summary>
        /// <param name="items">A list of items query parameters.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [Get]
        HttpResponseMessage Get([SendAsQuery("items")]IEnumerable<string> items);
    }
}