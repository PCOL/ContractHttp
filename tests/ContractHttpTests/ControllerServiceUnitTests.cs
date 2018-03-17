namespace ContractHttpTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;
    using ContractHttp;
    using ContractHttpTests.Resources;
    using ContractHttpTests.Resources.Models;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    [TestClass]
    public class ControllerServiceUnitTests
    {
        private TestServer testServer;

        private HttpClient testClient;

        [TestInitialize]
        public void TestInitialize()
        {
            this.testServer = TestUtils.CreateTestServer(
                services =>
                {
                    services
                        .AddMvc()
                        .AddDynamicController<ITestControllerService>(
                            new TestControllerService());
                },
                app =>
                {
                    var loggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();
                    loggerFactory.AddDebug();
                    loggerFactory.AddConsole();

                    app.UseMvc();
                });

            this.testClient = this.testServer.CreateClient();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (this.testServer != null)
            {
                this.testServer.Dispose();
            }
        }

        [TestMethod]
        public async Task CreateController_GetAll()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/api/data"))
            {
                var response = await this.testClient.SendAsync(request);
                Assert.IsNotNull(response);
                Assert.IsTrue(response.IsSuccessStatusCode);

                var content = await response.Content.ReadAsStringAsync();
                var list = JsonConvert.DeserializeObject<IEnumerable<TestData>>(content);

                Assert.IsNotNull(list);
                Assert.AreEqual("Test1", list.First().Name);
                Assert.AreEqual("Somewhere1", list.First().Address);
                Assert.AreEqual("Test2", list.Skip(1).First().Name);
                Assert.AreEqual("Somewhere2", list.Skip(1).First().Address);
            }
        }

        [TestMethod]
        public async Task CreateController_GetByName()
        {
            const string CustomerName = "Mr X";

            using (var request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost/api/data/{CustomerName}"))
            {
                var response = await this.testClient.SendAsync(request);
                Assert.IsNotNull(response);
                Assert.IsTrue(response.IsSuccessStatusCode);

                var content = await response.Content.ReadAsStringAsync();
                var testData = JsonConvert.DeserializeObject<TestData>(content);

                Assert.IsNotNull(testData);
                Assert.AreEqual(CustomerName, testData.Name);
                Assert.AreEqual("Somewhere", testData.Address);
            }
        }
    }
}