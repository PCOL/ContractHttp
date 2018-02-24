namespace ServerExample
{
    using System.Collections.Generic;
    using System.Reflection;
    using ContractHttp;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using ServerExample.Services;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddDynamicController(
                    typeof(ICustomerService),
                    new CustomerService());
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddDebug();
            loggerFactory.AddConsole();

            app.UseMvc();
        }
    }
}