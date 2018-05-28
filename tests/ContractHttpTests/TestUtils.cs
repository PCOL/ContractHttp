namespace ContractHttpTests
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Test utilities.
    /// </summary>
    public static class TestUtils
    {
        /// <summary>
        /// Creates a test server.
        /// </summary>
        /// <param name="configureServices">A configure services action.</param>
        /// <param name="configure">A configure aciton.</param>
        /// <returns>A <see cref="TestServer"/> instance.</returns>
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