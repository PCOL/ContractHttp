namespace ContractHttpTests.Resources
{
    using System.Net.Http;
    using ContractHttp;
    using Microsoft.AspNetCore.Mvc;

    [HttpClientContract(Route = "api/testheaders")]
    public interface ITestServiceWithHeaders
    {
        [Get()]
        HttpResponseMessage Get([SendAsHeader("x-test-header")]string header);

        [Get("{name}")]
        HttpResponseMessage GetByName(string name, [FromHeader(Name = "x-test-header")]out string header);
    }
}