namespace ContractHttpTests
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;

    public static class TestUtils
    {
        public static TestServer CreateTestServer(Action<IServiceCollection> action)
        {
            var testServer = new TestServer(
                new WebHostBuilder()
                    .ConfigureServices(
                        services =>
                        {
                            action?.Invoke(services);
                            services.AddMvc();
                        })
                    .Configure(
                        app =>
                        {
                            app.UseMvc();
                        }));

            return testServer;
        }
    }
}