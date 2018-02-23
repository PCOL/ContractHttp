using System.Net.Http;
using System.Threading.Tasks;
using ContractHttp;
using ContractHttpTests.Resources.Models;
using Microsoft.AspNetCore.Mvc;

namespace ContractHttpTests.Resources
{
    [HttpClientContract(Route = "api/test")]
    public interface ITestServiceUsingMvcAttributes
    {
        [HttpGet("")]
        HttpResponseMessage Get();

        [HttpGet("")]
        Task<HttpResponseMessage> GetAsync();

        [HttpGet("{name}")]
        TestData Get(string name);

        [HttpDelete("{name}")]
        HttpResponseMessage Delete(string name);

        [HttpDelete("{name}")]
        Task<HttpResponseMessage> DeleteAsync(string name);

        [HttpPost()]
        CreateResponseModel Create([SendAsContent]CreateModel model);

        [HttpPost()]
        Task<CreateResponseModel> CreateAsync([SendAsContent]CreateModel model);

        [HttpPost()]
        CreateResponseModel CreateWithHttpResponse([SendAsContent]CreateModel model, out HttpResponseMessage response);

        [HttpPut("{name}")]
        HttpResponseMessage UpdateModel(string name, [SendAsContent]TestData value);
    }
}