namespace ContractHttpTests.Resources
{
    using System.Linq;
    using ContractHttpTests.Resources.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Primitives;

    [Route("api/testheaders")]
    public class TestControllerWithHeaders
        : Controller
    {
        [HttpGet("")]
        public IActionResult Get()
        {
            if (this.HttpContext.Request.Headers.TryGetValue("x-test-header", out StringValues values) == true)
            {
                return this.StatusCode(StatusCodes.Status200OK, values.First());
            }

            return this.StatusCode(StatusCodes.Status404NotFound);
        }

        [HttpGet("{name}")]
        public IActionResult Get(string name)
        {

            this.Response.Headers.Add("x-test-header", "header value");

            return this.StatusCode(
                StatusCodes.Status200OK,
                new TestData()
                {
                    Name = name
                });
        }
    }
}