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
    public class TestServiceWithHeadersProxyUnitTests
    {
        private TestServer testServer;

        private HttpClientProxy<ITestServiceWithHeaders> clientProxy;

        private ITestServiceWithHeaders testService;

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
                testServer.CreateClient());

            this.testService = clientProxy.GetProxyObject();
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
        public void GetWithHeader_SendsHeader_ReceivesHeaderValue()
        {
            const string HeaderValue = "header value";
            var response = this.testService.Get(HeaderValue);
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(HeaderValue, response.Content.ReadAsStringAsync().Result.Trim('\"'));
        }
    }
}