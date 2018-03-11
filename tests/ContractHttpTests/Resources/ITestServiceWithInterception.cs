namespace ContractHttpTests
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using ContractHttp;
    using ContractHttpTests.Resources.Models;

    [HttpClientContract(Route = "api/test", ContentType = "application/json")]
    public interface ITestServiceWithInterception
    {
        [Get("{name}")]
        Task<TestData> GetAsync(string name, Action<HttpRequestMessage> requestAction);

        [Get("{name}")]
        Task<TestData> GetAsync(string name, Action<HttpResponseMessage> responseAction);

        [Get("{name}")]
        Task<TestData> GetAsync(string name, Func<HttpResponseMessage, TestData> responseFunc);
    }
}