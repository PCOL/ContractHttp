namespace ContractHttpTests
{
    using System.Net;
    using System.Threading.Tasks;
    using ContractHttp;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests a service with retry.
    /// </summary>
    [TestClass]
    public class TestServiceProxyWithRetryUnitTests
    {
        private TestServer testServer;

        private ITestServiceWithRetry testService;

        private TestRetryCounts retryCounts;

        /// <summary>
        /// Test initialisation.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.retryCounts = new TestRetryCounts();
            this.testServer = TestUtils.CreateTestServer(
                services =>
                {
                    services.AddSingleton<TestRetryCounts>(this.retryCounts);
                    services.AddTransient<TestControllerForRetry>();
                    services.AddMvc();
                });

            var httpClient = this.testServer.CreateClient();
            var testServiceProxy = new HttpClientProxy<ITestServiceWithRetry>(
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
        /// Tests that a method with retry retries the correct number of times.
        /// </summary>
        /// <returns>A task.</returns>
        [TestMethod]
        public async Task CallMethod_WithRetry_RetriesThreeTimes()
        {
            await this.testService.GetAsync((int)HttpStatusCode.BadGateway);
            Assert.AreEqual(2, this.retryCounts.GetCount);
        }

        /// <summary>
        /// Tests that a get method with retry and a response function retries the correct
        /// number of times.
        /// </summary>
        /// <returns>A task.</returns>
        [TestMethod]
        public async Task CallGetMethod_WithResponseFunctionAndRetry_RetriesThreeTimes()
        {
            bool called = false;
            await this.testService.GetAsync(
                (int)HttpStatusCode.BadGateway,
                (r) =>
                {
                    called = true;
                    return r.IsSuccessStatusCode;
                });

            Assert.IsTrue(called);
            Assert.AreEqual(2, this.retryCounts.GetCount);
        }

        /// <summary>
        /// Tests that a post method with retry and a response function retries the correct
        /// number of times.
        /// </summary>
        /// <returns>A task.</returns>
        [TestMethod]
        public async Task CallPostMethod_WithResponseFunctionAndRetry_RetriesThreeTimes()
        {
            bool called = false;
            var result = await this.testService.PostAsync(
                (r) =>
                {
                    called = true;
                    return r.IsSuccessStatusCode;
                });

            Assert.IsTrue(result);
            Assert.IsTrue(called);
            Assert.AreEqual(2, this.retryCounts.PostCount);
        }
    }
}