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
        /// <param name="configure">A configure action.</param>
        /// <param name="url">Optional url to use.</param>
        /// <returns>A <see cref="TestServer"/> instance.</returns>
        public static TestServer CreateTestServer(Action<IServiceCollection> configureServices = null, Action<IApplicationBuilder> configure = null, string url = "http://localhost")
        {
            var testServer = new TestServer(
                new WebHostBuilder()
                    .UseUrls(url)
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