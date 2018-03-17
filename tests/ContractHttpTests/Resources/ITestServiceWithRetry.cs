namespace ContractHttpTests
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using ContractHttp;

    [Retry(RetryCount = 3, HttpStatusCodesToRetry = new[] { HttpStatusCode.BadGateway })]
    [HttpClientContract(Route = "api")]
    public interface ITestServiceWithRetry
    {
        [Get("{status}")]
        Task<HttpResponseMessage> GetAsync(int status);
    }
}