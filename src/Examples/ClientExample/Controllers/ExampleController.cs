namespace ClientExample.Controllers
{
    using System;
    using ClientExample.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    [Controller]
    [Route("api/customers")]
    public class ExampleController
        : Controller
    {
        [HttpGet]
        public IActionResult GetCustomers()
        {
            return this.StatusCode(
                StatusCodes.Status200OK,
                new[]
                {
                    new CustomerModel()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Customer1"
                    },
                    new CustomerModel()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Customer2"
                    }
                });
        }
    }
}