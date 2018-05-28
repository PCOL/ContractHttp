namespace ContractHttpTests.Resources
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using ContractHttp;
    using ContractHttpTests.Resources.Models;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Defines a test service interface.
    /// </summary>
    [HttpClientContract(Route = "api/test")]
    public interface ITestServiceUsingMvcAttributes
    {
        /// <summary>
        /// Get.
        /// </summary>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [HttpGet("")]
        HttpResponseMessage Get();

        /// <summary>
        /// Get async.
        /// </summary>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [HttpGet("")]
        Task<HttpResponseMessage> GetAsync();

        /// <summary>
        /// Get by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A data model.</returns>
        [HttpGet("{name}")]
        TestData Get(string name);

        /// <summary>
        /// Delete by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [HttpDelete("{name}")]
        HttpResponseMessage Delete(string name);

        /// <summary>
        /// Delete by name async.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [HttpDelete("{name}")]
        Task<HttpResponseMessage> DeleteAsync(string name);

        /// <summary>
        /// Create.
        /// </summary>
        /// <param name="model">The data model.</param>
        /// <returns>A response model.</returns>
        [HttpPost()]
        CreateResponseModel Create([SendAsContent]CreateModel model);

        /// <summary>
        /// Create async.
        /// </summary>
        /// <param name="model">The data model.</param>
        /// <returns>A response model.</returns>
        [HttpPost()]
        Task<CreateResponseModel> CreateAsync([SendAsContent]CreateModel model);

        /// <summary>
        /// Create.
        /// </summary>
        /// <param name="model">The data model.</param>
        /// <param name="response">A variable to receive the <see cref="HttpResponseMessage"/>.</param>
        /// <returns>A response model.</returns>
        [HttpPost()]
        CreateResponseModel CreateWithHttpResponse([SendAsContent]CreateModel model, out HttpResponseMessage response);

        /// <summary>
        /// Update by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [HttpPut("{name}")]
        HttpResponseMessage UpdateModel(string name, [SendAsContent]TestData value);
    }
}