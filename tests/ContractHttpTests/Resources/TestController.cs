namespace ContractHttpTests.Resources
{
    using System;
    using System.Collections.Generic;
    using ContractHttpTests.Resources.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;

    [Route("api/test")]
    public class TestController
        : Controller
    {
        [HttpGet("")]
        public IActionResult Get()
        {
            return this.StatusCode(
                StatusCodes.Status200OK,
                new[]
                {
                    new TestData()
                    {
                        Name = "Name",
                        Address = "Address"
                    },
                    new TestData()
                    {
                        Name = "Name",
                        Address = "Address"
                    }
                });
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

            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpDelete("")]
        public IActionResult DeleteAddressFromQuery([FromQuery]string name)
        {
            if (name == "good")
            {
                return this.StatusCode(StatusCodes.Status204NoContent);
            }

            return this.StatusCode(StatusCodes.Status500InternalServerError);
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
            else if (model.Name == "conflict")
            {
                return this.StatusCode(StatusCodes.Status409Conflict);
            }

            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpPost("form")]
        public IActionResult CreateFromForm([FromForm]CreateModel model)
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
            else if (model.Name == "conflict")
            {
                return this.StatusCode(StatusCodes.Status409Conflict);
            }

            return this.StatusCode(StatusCodes.Status500InternalServerError);
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

        [HttpGet("id/{id}")]
        public IActionResult GetById(string id)
        {
            return this.StatusCode(
                StatusCodes.Status200OK,
                new TestData()
                {
                    Name = "Name",
                    Address = "Address"
                });
        }
    }
}