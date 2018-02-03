namespace ContractHttpTests.Resources
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;
    using ContractHttpTests.Resources.Models;

    [Route("api/test")]
    public class TestController
        : Controller
    {
        [HttpGet("")]
        public IActionResult Get()
        {
            return this.StatusCode(StatusCodes.Status200OK);
        }

        [HttpGet("{name}")]
        public IActionResult GetAddress(string name)
        {
            return this.Json(
                new TestData()
                {
                    Name = name,
                    Address = "Address"
                });
        }
    }
}