namespace ServerExample.Services
{
    using System.Collections.Generic;
    using ContractHttp;
    using Microsoft.AspNetCore.Mvc;
    using ServerExample.Models;

    [HttpController(RoutePrefix = "api/customers")]
    public interface ICustomerService
    {
        [HttpGet("")]
        IEnumerable<CustomerModel> GetAll();


        [HttpGet("{name}")]
        CustomerModel GetByName(string name);
    }
}