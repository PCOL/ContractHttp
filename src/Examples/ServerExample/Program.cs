using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using ServerExample.Models;

namespace ServerExample
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "http://localhost:6000";

            using (var webHost = StartServer(url))
            {
                var httpClient = new HttpClient();
                GetCustomers(httpClient, url);
                GetCustomer(httpClient, url, "test");
            }
        }

        private static void GetCustomers(HttpClient httpClient, string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"{url}/api/customers"))
            {
                var response = httpClient.SendAsync(request).Result;
                Console.WriteLine("StatusCode: {0}", response.StatusCode);
                if (response.IsSuccessStatusCode == true)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    if (string.IsNullOrEmpty(content) == false)
                    {
                        Console.WriteLine("Get Customers");
                        Console.WriteLine();
                        var customers = JsonConvert.DeserializeObject<IEnumerable<CustomerModel>>(content);
                        foreach (var customer in customers)
                        {
                            Console.WriteLine("Id: {0}, Name: {1}", customer.Id, customer.Name);
                        }
                    }
                }
            }
        }

        private static void GetCustomer(HttpClient httpClient, string url, string name)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"{url}/api/customers/{name}"))
            {
                var response = httpClient.SendAsync(request).Result;
                Console.WriteLine("StatusCode: {0}", response.StatusCode);
                if (response.IsSuccessStatusCode == true)
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    if (string.IsNullOrEmpty(content) == false)
                    {
                        Console.WriteLine("Get Customer: {0}", name);
                        Console.WriteLine();
                        var customer = JsonConvert.DeserializeObject<CustomerModel>(content);
                        Console.WriteLine("Id: {0}, Name: {1}", customer.Id, customer.Name);
                    }
                }
            }
        }

        private static IWebHost StartServer(string url)
        {
            var webHost = new WebHostBuilder()
                .UseKestrel()
                .UseStartup<Startup>()
                .UseUrls(url)
                .Build();

            webHost.Start();

            return webHost;
        }
    }
}
