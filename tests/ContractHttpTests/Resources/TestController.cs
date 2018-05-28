namespace ContractHttpTests.Resources
{
    using System;
    using System.Collections.Generic;
    using ContractHttpTests.Resources.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A test controller.
    /// </summary>
    [Route("api/test")]
    public class TestController
        : Controller
    {
        /// <summary>
        /// Get.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/>.</returns>
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

        /// <summary>
        /// Gets the address by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>An <see cref="IActionResult"/>.</returns>
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

        /// <summary>
        /// Deletes by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>An <see cref="IActionResult"/>.</returns>
        [HttpDelete("{name}")]
        public IActionResult DeleteAddress(string name)
        {
            if (name == "good")
            {
                return this.StatusCode(StatusCodes.Status204NoContent);
            }

            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }

        /// <summary>
        /// Delete address by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>An <see cref="IActionResult"/>.</returns>
        [HttpDelete("")]
        public IActionResult DeleteAddressFromQuery([FromQuery]string name)
        {
            if (name == "good")
            {
                return this.StatusCode(StatusCodes.Status204NoContent);
            }

            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }

        /// <summary>
        /// Create.
        /// </summary>
        /// <param name="model">The create model.</param>
        /// <returns>An <see cref="IActionResult"/>.</returns>
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

        /// <summary>
        /// Create.
        /// </summary>
        /// <param name="model">The create model.</param>
        /// <returns>An <see cref="IActionResult"/>.</returns>
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

        /// <summary>
        /// Update by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns>An <see cref="IActionResult"/>.</returns>
        [HttpPut("{name}")]
        public IActionResult Update(string name, [FromBody]TestData value)
        {
            if (name == value.Name)
            {
                return this.StatusCode(StatusCodes.Status204NoContent);
            }

            return this.StatusCode(StatusCodes.Status404NotFound);
        }

        /// <summary>
        /// Get by id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns>An <see cref="IActionResult"/>.</returns>
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