namespace ClientExample
{
    using System;
    using System.Collections.Generic;
    using ClientExample.Models;
    using ContractHttp;

    [HttpClientContract(Route = "api/customers")]
    public interface ICustomerClient
    {
        [Get]
        IEnumerable<CustomerModel> GetCustomers();
    }
}