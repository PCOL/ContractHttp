namespace ClientExample
{
    using System;
    using System.Net.Http;
    using ContractHttp;
    using Microsoft.AspNetCore.Hosting;

    class Program
    {
        static void Main(string[] args)
        {
            string url = "http://localhost:6000";

            using (var webHost = StartServer(url))
            {
                var httpClient = new HttpClient();
                var proxy = new HttpClientProxy<ICustomerClient>(url, httpClient);
                var client = proxy.GetProxyObject();

                var customers = client.GetCustomers();
                if (customers != null)
                {
                    foreach (var customer in customers)
                    {
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
