namespace ContractHttpTests.Resources
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using ContractHttp;
    using ContractHttpTests.Resources.Models;

    /// <summary>
    /// Defines a test service.
    /// </summary>
    [HttpClientContract(Route = "api/test")]
    public interface ITestService
    {
        /// <summary>
        /// Get.
        /// </summary>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [Get("")]
        HttpResponseMessage Get();

        /// <summary>
        /// Get by name.
        /// </summary>
        /// <param name="name">The of the data to return.</param>
        /// <returns>A <see cref="TestData"/> instance if found; otherwise null.</returns>
        [Get("{name}")]
        TestData Get(string name);

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>A list of <see cref="TestData"/> instances if found; otherwise null.</returns>
        [Get("")]
        IEnumerable<TestData> GetAll();

        /// <summary>
        /// Create using a model.
        /// </summary>
        /// <param name="model">The model containing the data for the create.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [Post("")]
        HttpResponseMessage Create(CreateModel model);

        /// <summary>
        /// Create form Url encoded.
        /// </summary>
        /// <param name="parms">A dictionary containing the parameters to be encoded.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [Post("form")]
        HttpResponseMessage CreateFormUrlEncoded(
            [SendAsFormUrl]Dictionary<string, string> parms);

        /// <summary>
        /// Create form Url encoded.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [Post("form")]
        HttpResponseMessage CreateFormUrlEncoded(
            [SendAsFormUrl(Name = "Name")]string name,
            [SendAsFormUrl(Name = "Value")]string value);

        /// <summary>
        /// Delete by name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [Delete("{name}")]
        HttpResponseMessage Delete(string name);

        /// <summary>
        /// Delete using query string.
        /// </summary>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [Delete("")]
        HttpResponseMessage DeleteUsingQueryString([SendAsQuery("name")] string name);

        /// <summary>
        /// Get by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Get("id/{id}")]
        [return: FromJson("", ReturnType = typeof(ServiceResult<TestData>))]
        IServiceResult<TestData> GetById(string id);

        /// <summary>
        /// Get by id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [Get("id/{id}")]
        HttpResponseMessage GetById(
            string id,
            [FromJson("name")]out string name);

        /// <summary>
        /// Get address by id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="address"></param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        [Get("id/{id}")]
        HttpResponseMessage GetAddressById(
            string id,
            [FromModel(typeof(TestData), "Address")]out string address);
    }
}