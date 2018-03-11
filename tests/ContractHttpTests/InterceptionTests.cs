using System.Net.Http;
using System.Threading.Tasks;
using ContractHttp;
using ContractHttpTests.Resources;
using ContractHttpTests.Resources.Models;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace ContractHttpTests
{
    [TestClass]
    public class InterceptionTests
    {
        private TestServer testServer;

        private ITestServiceWithInterception testService;

        [TestInitialize]
        public void TestInitialize()
        {
            this.testServer = TestUtils.CreateTestServer(
                services =>
                {
                    services.AddTransient<TestController>();
                    services.AddMvc();
                });

            var httpClient = testServer.CreateClient();
            var testServiceProxy = new HttpClientProxy<ITestServiceWithInterception>(
                "http://localhost",
                new HttpClientProxyOptions()
                {
                    HttpClient = httpClient
                });

            this.testService = testServiceProxy.GetProxyObject();
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
        public async Task Get_WithRequestInterceptionAction_ActionCalled()
        {
            bool called = false;

            var result = await this.testService.GetAsync(
                "test",
                (HttpRequestMessage r) =>
                {
                    called = true;
                });

            Assert.IsNotNull(result);
            Assert.IsTrue(called);
        }

        [TestMethod]
        public async Task Get_WithResponseInterceptionAction_ActionCalled()
        {
            bool called = false;

            var result = await this.testService.GetAsync(
                "test",
                (HttpResponseMessage r) =>
                {
                    called = true;
                });

            Assert.IsNotNull(result);
            Assert.IsTrue(called);
        }

        [TestMethod]
        public async Task Get_WithRequestInterceptionFunc_Called()
        {
            bool called = false;

            var result = await this.testService.GetAsync(
                "test",
                (HttpResponseMessage r) =>
                {
                    called = true;

                    var s = r.Content.ReadAsStringAsync().Result;

                    return JsonConvert.DeserializeObject<TestData>(s);
                });

            Assert.IsNotNull(result);
            Assert.IsTrue(called);
        }
    }
}