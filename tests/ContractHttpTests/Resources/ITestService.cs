namespace ContractHttpTests.Resources
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using ContractHttp;
    using ContractHttpTests.Resources.Models;

    [HttpClientContract(Route = "api/test")]
    public interface ITestService
    {
        [Get("")]
        HttpResponseMessage Get();

        [Get("{name}")]
        TestData Get(string name);

        [Get("")]
        IEnumerable<TestData> GetAll();

        [Post("")]
        HttpResponseMessage Create(CreateModel model);

        [Post("form")]
        HttpResponseMessage CreateFormUrlEncoded(
            [SendAsFormUrl]Dictionary<string, string> parms);

        [Post("form")]
        HttpResponseMessage CreateFormUrlEncoded(
            [SendAsFormUrl(Name = "Name")]string name,
            [SendAsFormUrl(Name = "Value")]string value);

        [Delete("{name}")]
        HttpResponseMessage Delete(string name);

        [Delete("")]
        HttpResponseMessage DeleteUsingQueryString([SendAsQuery("name")] string name);

        [Get("id/{id}")]
        [return: FromJson("", ReturnType = typeof(ServiceResult<TestData>))]
        IServiceResult<TestData> GetById(string id);
    }
}