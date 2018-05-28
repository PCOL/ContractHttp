namespace ContractHttpTests
{
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

    /// <summary>
    /// Tests a service with headers.
    /// </summary>
    [TestClass]
    public class TestServiceWithHeaderProxyUnitTests
    {
        private TestServer testServer;

        private HttpClientProxy<ITestServiceWithHeaders> clientProxy;

        private ITestServiceWithHeaders testService;

        /// <summary>
        /// Test initialisation.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.testServer = TestUtils.CreateTestServer(
                services =>
                {
                    services.AddTransient<TestControllerWithHeaders>();
                    services.AddMvc();
                });

            this.clientProxy = new HttpClientProxy<ITestServiceWithHeaders>(
                "http://localhost",
                new HttpClientProxyOptions()
                {
                    HttpClient = testServer.CreateClient()
                });

            this.testService = clientProxy.GetProxyObject();
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
        public void GetWithHeader_SendsHeader_ReceivesHeaderValue()
        {
            const string HeaderValue = "header value";
            var response = this.testService.Get(HeaderValue);
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(HeaderValue, response.Content.ReadAsStringAsync().Result.Trim('\"'));
        }

        /// <summary>
        /// Tests if a header is returned.
        /// </summary>
        [TestMethod]
        public void GetByName_ReturnsHeader()
        {
            const string HeaderValue = "header value";
            var response = this.testService.GetByName("TEST", out string header);
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(HeaderValue, header);
        }
    }
}