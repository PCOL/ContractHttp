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
    }
}