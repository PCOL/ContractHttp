namespace ContractHttpTests.Resources
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;
    using ContractHttpTests.Resources.Models;
    using System;

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

        [HttpDelete("{name}")]
        public IActionResult DeleteAddress(string name)
        {
            if (name == "good")
            {
                return this.StatusCode(StatusCodes.Status204NoContent);
            }

            return this.StatusCode(StatusCodes.Status404NotFound);
        }

        [HttpPost]
        public IActionResult Create([FromBody]CreateModel model)
        {
            if (model.Name == "good")
            {
                return this.StatusCode(
                    StatusCodes.Status201Created,
                    new CreateResponseModel()
                    {
                        Id = Guid.NewGuid().ToString()
                    });
            }

            return this.StatusCode(StatusCodes.Status409Conflict);
        }

        [HttpPut("{name}")]
        public IActionResult Update(string name, [FromBody]TestData value)
        {
            if (name == value.Name)
            {
                return this.StatusCode(StatusCodes.Status204NoContent);
            }

            return this.StatusCode(StatusCodes.Status404NotFound);
        }
    }
}