namespace ClientExample
{
    using System;
    using System.Collections.Generic;
    using ClientExample.Models;
    using ContractHttp;
    using Microsoft.AspNetCore.Mvc;

    [HttpClientContract(Route = "api/customers")]
    public interface ICustomerClient
    {
        [HttpGet]
        IEnumerable<CustomerModel> GetCustomers();
    }
}