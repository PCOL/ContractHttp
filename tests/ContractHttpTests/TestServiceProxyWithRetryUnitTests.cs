namespace ContractHttpTests
{
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

            var httpClient = testServer.CreateClient();
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
        /// <returns></returns>
        [TestMethod]
        public async Task CallMethod_WithRetry_RetriesThreeTimes()
        {
            await this.testService.GetAsync(404);
            Assert.AreEqual(2, this.retryCounts.GetCount);
        }

        /// <summary>
        /// Tests that a method with retry and a response function retries the correct
        /// number of times.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task CallMethod_WithResponseFunctionAndRetry_RetriesThreeTimes()
        {
            bool called = false;
            await this.testService.GetAsync(
                404,
                (r) =>
                {
                    called = true;
                    return r.IsSuccessStatusCode;
                });

            Assert.IsTrue(called)
;            Assert.AreEqual(2, this.retryCounts.GetCount);
        }
    }
}