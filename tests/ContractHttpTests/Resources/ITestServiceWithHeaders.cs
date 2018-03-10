namespace ContractHttpTests.Resources
{
    using System.Net.Http;
    using ContractHttp;

    [HttpClientContract(Route = "api/testheaders")]
    public interface ITestServiceWithHeaders
    {
        [Get()]
        HttpResponseMessage Get([SendAsHeader("x-test-header")]string header);
    }
}