namespace ContractHttpTests.Resources
{
    using System.Collections.Generic;
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

        [HttpCallContract(HttpCallMethod.HttpPost, "")]
        HttpResponseMessage Create(CreateModel model);


        [HttpCallContract(HttpCallMethod.HttpPost, "form")]
        HttpResponseMessage CreateFormUrlEncoded([SendAsFormUrl]Dictionary<string, string> parms);

        [HttpCallContract(HttpCallMethod.HttpPost, "form")]
        HttpResponseMessage CreateFormUrlEncoded(
            [SendAsFormUrl(Name = "Name")]string name,
            [SendAsFormUrl(Name = "Value")]string value);

        [HttpCallContract(HttpCallMethod.HttpDelete, "{name}")]
        HttpResponseMessage Delete(string name);

    }
}