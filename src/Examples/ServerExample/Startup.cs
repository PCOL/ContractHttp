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
            List<Assembly> list = new List<Assembly>();
            services.AddDynamicController(typeof(ICustomerService), new CustomerService(), list);

            var mvcBuilder = services.AddMvc();
            foreach (var item in list)
            {
                mvcBuilder.AddApplicationPart(item);
            }
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddDebug();
            loggerFactory.AddConsole();

            app.UseMvc();
        }
    }
}