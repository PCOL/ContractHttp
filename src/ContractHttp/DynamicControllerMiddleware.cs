namespace ContractHttp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;

    /// <summary>
    /// Middleware to invoke the dynamic controller.
    /// </summary>
    public class DynamicControllerMiddleware
    {
        private RequestDelegate next;

        private List<TypeInfo> controllerTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicControllerMiddleware"/> class.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="controllerTypes"></param>
        public DynamicControllerMiddleware(RequestDelegate next, List<TypeInfo> controllerTypes)
        {
            this.next = next;
            this.controllerTypes = controllerTypes;
        }

        public async Task Invoke(HttpContext context)
        {
/*
            Console.WriteLine(context.Request.Path);

            foreach (var c in this.controllerTypes)
            {
                var attr = c.GetCustomAttribute<RouteAttribute>();
                if (attr != null)
                {
                    Console.WriteLine("Route: {0}", attr.Template);
                    string path = "/" + attr.Template;
                    if (context.Request.Path.StartsWithSegments(new PathString(path)) == true)
                    {
                        Console.WriteLine("Match");
                        foreach( var m in c.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                        {
                            var getAttr = m.GetCustomAttribute<HttpGetAttribute>();
                            if (getAttr != null)
                            {
                                Console.WriteLine(getAttr.Template);
                                string test = path + "/" + getAttr.Template;
                                var comparer = new WildcardStringComparer(test.Replace("{id}", "*"));
                                if (comparer.CompareTo(context.Request.Path.Value) == 0)
                                {
                                    object obj = context.RequestServices.GetService(c.AsType());
                                    var response = m.Invoke(obj, new object[] { Guid.NewGuid() });

                                    var stream =  new MemoryStream();
                                    var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true);

                                    writer.Write(JsonConvert.SerializeObject(((ObjectResult)response).Value));

                                    context.Response.StatusCode = ((ObjectResult)response).StatusCode.GetValueOrDefault();
                                    context.Response.Body = stream;

                                    return;
                                }
                            }
                        }
                    }
                }
            }
*/

            await this.next.Invoke(context);
        }
    }
}