namespace ContractHttpTests
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;

    public static class TestUtils
    {
        public static TestServer CreateTestServer(Action<IServiceCollection> configureServices = null, Action<IApplicationBuilder> configure = null)
        {
            var testServer = new TestServer(
                new WebHostBuilder()
                    .UseUrls("http://localhost")
                    .ConfigureServices(
                        services =>
                        {
                            configureServices?.Invoke(services);
                        })
                    .Configure(
                        app =>
                        {
                            configure?.Invoke(app);
                            app.UseMvc();
                        }));

            return testServer;
        }
    }
}