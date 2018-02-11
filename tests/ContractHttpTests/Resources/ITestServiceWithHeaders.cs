namespace ContractHttpTests.Resources
{
    using System.Net.Http;
    using ContractHttp;
    using Microsoft.AspNetCore.Mvc;

    [HttpClientContract(Route = "api/testheaders")]
    public interface ITestServiceWithHeaders
    {
        [HttpGet()]
        HttpResponseMessage Get([SendAsHeader("x-test-header")]string header);
    }
}