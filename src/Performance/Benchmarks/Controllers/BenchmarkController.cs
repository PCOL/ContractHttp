namespace Benchmarks.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;
    using Benchmarks.Contracts.Models;

    [Route("api")]
    public class BenchmarkController
        : Controller
    {
        [HttpGet("")]
        public IActionResult Get()
        {
            return this.StatusCode(
                StatusCodes.Status200OK,
                new[]
                {
                    new SimpleModel()
                    {
                        Name = "name1",
                        Address = "address1"
                    },
                    new SimpleModel()
                    {
                        Name = "name2",
                        Address = "address2"
                    }
                });
        }

        [HttpGet("{name}")]
        public IActionResult GetByName(string name)
        {
            return this.StatusCode(
                StatusCodes.Status200OK,
                new SimpleModel()
                {
                    Name = name,
                    Address = "address"
                });
        }
    }
}