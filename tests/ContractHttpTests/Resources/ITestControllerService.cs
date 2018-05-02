namespace ContractHttpTests.Resources
{
    using System.Collections.Generic;
    using ContractHttp;
    using ContractHttpTests.Resources.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;

    [HttpController(ControllerTypeName = "DataController", RoutePrefix = "api/data")]
    public interface ITestControllerService
    {
        [HttpGet("")]
        IEnumerable<TestData> GetAll();

        [HttpGet("{name}")]
        TestData GetByName(string name);

        [HttpDelete("{name}")]
        [ServiceCallFilterTest]
        void Delete(string name);
    }
}