namespace ContractHttpTests.Resources
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using ContractHttp;
    using ContractHttpTests.Resources.Models;
    using Microsoft.AspNetCore.Mvc;

    [HttpClientContract(Route = "api/test")]
    public interface ITestService
    {
        [HttpCallContract(HttpCallMethod.HttpGet, "")]
        HttpResponseMessage Get();

        [HttpCallContract(HttpCallMethod.HttpGet, "{name}")]
        TestData Get(string name);

        [HttpCallContract(HttpCallMethod.HttpDelete, "{name}")]
        HttpResponseMessage Delete(string name);

        [HttpGet("")]
        HttpResponseMessage GetUsingMvcAttribute();

        [HttpGet("")]
        Task<HttpResponseMessage> GetUsingMvcAttributeAsync();


        [HttpGet("{name}")]
        TestData GetUsingMvcAttribute(string name);

        [HttpDelete("{name}")]
        HttpResponseMessage DeleteUsingMvcAttribute(string name);

        [HttpDelete("{name}")]
        Task<HttpResponseMessage> DeleteUsingMvcAttributeAsync(string name);

        [HttpPost()]
        CreateResponseModel CreateUsingMvcAttibute([SendAsContent]CreateModel model);

        [HttpPost()]
        Task<CreateResponseModel> CreateUsingMvcAttibuteAsync([SendAsContent]CreateModel model);

        [HttpPost()]
        CreateResponseModel CreateWithHttpResponseUsingMvcAttibute([SendAsContent]CreateModel model, out HttpResponseMessage response);

        [HttpPut("{name}")]
        HttpResponseMessage UpdateModelUsingMvcAttribute(string name, [SendAsContent]TestData value);
    }
}