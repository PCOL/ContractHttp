namespace ContractHttpTests
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using ContractHttp;
    using ContractHttpTests.Resources;
    using ContractHttpTests.Resources.Models;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    /// <summary>
    /// Tests service call interception.
    /// </summary>
    [TestClass]
    public class InterceptionTests
    {
        private TestServer testServer;

        private ITestServiceWithInterception testService;

        /// <summary>
        /// Test initialisation.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.testServer = TestUtils.CreateTestServer(
                services =>
                {
                    services.AddTransient<TestController>();
                    services.AddMvc();
                });

            var httpClient = this.testServer.CreateClient();
            var testServiceProxy = new HttpClientProxy<ITestServiceWithInterception>(
                "http://localhost",
                new HttpClientProxyOptions()
                {
                    HttpClient = httpClient
                });

            this.testService = testServiceProxy.GetProxyObject();
        }

        /// <summary>
        /// Test cleanup.
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            if (this.testServer != null)
            {
                this.testServer.Dispose();
            }
        }

        /// <summary>
        /// Tests a method with a request action.
        /// </summary>
        /// <returns>A task.</returns>
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

        /// <summary>
        /// Tests a method with a response action.
        /// </summary>
        /// <returns>A task.</returns>
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

        /// <summary>
        /// Tests a method with a response function.
        /// </summary>
        /// <returns>A task.</returns>
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