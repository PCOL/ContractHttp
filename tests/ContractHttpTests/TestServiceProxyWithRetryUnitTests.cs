namespace ContractHttpTests
{
    using System.Threading.Tasks;
    using ContractHttp;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestServiceProxyWithRetryUnitTests
    {
        private TestServer testServer;

        private ITestServiceWithRetry testService;

        private TestRetryCounts retryCounts;

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

        [TestCleanup]
        public void TestCleanup()
        {
            if (this.testServer != null)
            {
                this.testServer.Dispose();
            }
        }

        [TestMethod]
        public async Task Test()
        {
            await this.testService.GetAsync(404);
            Assert.AreEqual(2, this.retryCounts.GetCount);
        }
    }
}