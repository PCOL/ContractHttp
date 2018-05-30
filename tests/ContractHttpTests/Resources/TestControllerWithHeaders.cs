namespace ContractHttpTests.Resources
{
    using System.Linq;
    using ContractHttpTests.Resources.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Primitives;

    /// <summary>
    /// A test controller for testing requests with headers.
    /// </summary>
    [Route("api/testheaders")]
    public class TestControllerWithHeaders
        : Controller
    {
        /// <summary>
        /// Get.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/>.</returns>
        [HttpGet("")]
        public IActionResult Get()
        {
            if (this.HttpContext.Request.Headers.TryGetValue("x-test-header", out StringValues values) == true)
            {
                return this.StatusCode(StatusCodes.Status200OK, values.First());
            }

            return this.StatusCode(StatusCodes.Status404NotFound);
        }

        /// <summary>
        /// Get by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>An <see cref="IActionResult"/>.</returns>
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