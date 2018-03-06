namespace Benchmarks
{
    using Benchmarks.Contracts;
    using Benchmarks.Controllers;
    using BenchmarkDotNet.Attributes;
    using ContractHttp;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using System.Threading.Tasks;

    public class ClientBenchmarks
    {
        private TestServer testServer;

        private IClientBenchmarks benchmarkClient;

        [GlobalSetup()]
        public void GlobalSetup()
        {
            this.testServer = Utility.CreateTestServer(
                services =>
                {
                    services.AddTransient<BenchmarkController>();
                    services.AddMvc();
                });

            var httpClient = testServer.CreateClient();
            this.benchmarkClient = new HttpClientProxy<IClientBenchmarks>(
                "http://localhost",
                new HttpClientProxyOptions()
                {
                    HttpClient = httpClient
                }).GetProxyObject();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            if (this.testServer != null)
            {
                this.testServer.Dispose();
            }
        }

        [Benchmark]
        public void SimpleGet()
        {
            var response = this.benchmarkClient.Get();
        }

        [Benchmark]
        public async Task SimpleGetAsync()
        {
            var response = await this.benchmarkClient.GetAsync();
        }

        [Benchmark]
        public void SimpleGetByName()
        {
            var response = this.benchmarkClient.GetByName("name");
        }

        [Benchmark]
        public async Task SimpleGetByNameAsync()
        {
            var response = await this.benchmarkClient.GetByNameAsync("name");
        }
    }
}