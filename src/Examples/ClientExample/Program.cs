namespace ClientExample
{
    using System;
    using System.Net.Http;
    using ContractHttp;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    class Program
    {
        static void Main(string[] args)
        {
            string url = "http://localhost:6000";

            using (var webHost = StartServer(url))
            {
                var client = webHost.Services.GetService<ICustomerClient>();

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
                .ConfigureServices(
                    services =>
                    {
                        services.AddJsonObjectSerializer();
                        services.AddHttpClient(new HttpClient());
                        services.AddHttpClientProxy<ICustomerClient>(url);
                    })
                .Build();

            webHost.Start();

            return webHost;
        }
    }
}
