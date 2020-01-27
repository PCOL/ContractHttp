namespace ContractHttpTests.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ContractHttpTests.Resources.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// A test controller for testing requests with query parameters.
    /// </summary>
    [Route("api/testquery")]
    public class TestControllerWithQueryParameters
        : Controller
    {
        /// <summary>
        /// Get with multiple query parameters of the same name.
        /// </summary>
        /// <param name="items">A list of items.</param>
        /// <returns>An <see cref="IActionResult"/>.</returns>
        [HttpGet]
        public IActionResult Get(IEnumerable<string> items)
        {
            if (items.SequenceEqual(new[] { "A", "B", "C", "D" }) == true)
            {
                return this.StatusCode(StatusCodes.Status204NoContent);
            }

            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }

        /// <summary>
        /// Get with multiple query parameters of the same name.
        /// </summary>
        /// <param name="name1">Parameter 1.</param>
        /// <param name="name2">Parameter 2.</param>
        /// <returns>An <see cref="IActionResult"/>.</returns>
        [HttpGet("map")]
        public IActionResult Get(string name1, string name2)
        {
            if (name1 == "name1value" &&
                name2 == "name2value")
            {
                return this.StatusCode(StatusCodes.Status204NoContent);
            }

            return this.StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}