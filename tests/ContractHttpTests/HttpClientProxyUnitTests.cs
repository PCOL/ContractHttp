using System;
using System.Net;
using System.Net.Http;
using ContractHttp;
using ContractHttpTests.Resources;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ContractHttpTests
{
    [TestClass]
    public class HttpClientProxyUnitTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateProxy_WithNullBaseUri_Throws()
        {
            new HttpClientProxy<ITestService>(null, (HttpClient)null);
        }

        [TestMethod]
        public void CreateProxy_SetupInterface_CallGet()
        {
            using (var testServer = this.CreateTestServer())
            {
                var clientProxy = new HttpClientProxy<ITestService>("http://localhost", testServer.CreateClient());
                var proxy = clientProxy.GetProxyObject();

                var response = proxy.Get();
                Assert.IsNotNull(response);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [TestMethod]
        public void CreateProxy_GetTestDataByName()
        {
            using (var testServer = this.CreateTestServer())
            {
                var clientProxy = new HttpClientProxy<ITestService>("http://localhost", testServer.CreateClient());
                var proxy = clientProxy.GetProxyObject();

                var testData = proxy.Get("Name");
                Assert.IsNotNull(testData);
                Assert.AreEqual("Name", testData.Name);
                Assert.AreEqual("Address", testData.Address);
            }
        }

        private TestServer CreateTestServer()
        {
            var testServer = new TestServer(
                new WebHostBuilder()
                    .ConfigureServices(
                        services =>
                        {
                            services.AddSingleton<TestController>();
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
