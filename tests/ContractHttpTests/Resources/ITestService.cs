namespace ContractHttpTests.Resources
{
    using System.Net.Http;
    using ContractHttp;
    using ContractHttpTests.Resources.Models;

    [HttpClientContract(Route = "api/test")]
    public interface ITestService
    {
        [HttpCallContract(HttpCallMethod.HttpGet, "")]
        HttpResponseMessage Get();

        [HttpCallContract(HttpCallMethod.HttpGet, "{name}")]
        TestData Get(string name);
    }
}