namespace ContractHttpTests
{
    using System.Collections.Generic;
    using System.Net;
    using ContractHttp;
    using ContractHttpTests.Resources;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests a service with headers.
    /// </summary>
    [TestClass]
    public class TestServiceWithQueryParametersProxyUnitTests
    {
        private TestServer testServer;

        private HttpClientProxy<ITestServiceWithQueryParameters> clientProxy;

        private ITestServiceWithQueryParameters testService;

        /// <summary>
        /// Test initialisation.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.testServer = TestUtils.CreateTestServer(
                services =>
                {
                    services.AddTransient<TestControllerWithQueryParameters>();
                    services.AddMvc(
                        options =>
                        {
                            options.EnableEndpointRouting = false;
                        });
                });

            this.clientProxy = new HttpClientProxy<ITestServiceWithQueryParameters>(
                "http://localhost",
                new HttpClientProxyOptions()
                {
                    HttpClient = this.testServer.CreateClient()
                });

            this.testService = this.clientProxy.GetProxyObject();
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
        /// Sends a header.
        /// </summary>
        [TestMethod]
        public void GetWithEnumerableHeader_SendsHeader_ReceivesHeaderValue()
        {
            var items = new[] { "A", "B", "C", "D" };
            var response = this.testService.Get(items);
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        /// <summary>
        /// Sends a header.
        /// </summary>
        [TestMethod]
        public void GetWithDictionaryHeader_SendsHeader_ReceivesHeaderValue()
        {
            var items = new Dictionary<string, string>()
            {
                { "name1", "name1value" },
                { "name2", "name2value" }
            };

            var response = this.testService.Get(items);
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}