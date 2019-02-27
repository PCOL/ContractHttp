namespace ServerExample.Services
{
    using System;
    using System.Collections.Generic;
    using ServerExample.Models;

    public class CustomerService
        : ICustomerService
    {
        public IEnumerable<CustomerModel> GetAll()
        {
            return new[]
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
                };
        }

        public CustomerModel GetByName(string name)
        {
            return new CustomerModel()
            {
                Id = Guid.NewGuid().ToString(),
                Name = name
            };
        }

        public CustomerModel CreateCustomer(CustomerModel customer)
        {
            Console.WriteLine("-------------------------------------");
            Console.WriteLine("Id: {0}", customer.Id);
            Console.WriteLine("Name: {0}", customer.Name);
            Console.WriteLine("-------------------------------------");
            return customer;
        }
    }
}