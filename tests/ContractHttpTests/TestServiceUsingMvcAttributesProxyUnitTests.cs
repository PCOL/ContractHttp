using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ContractHttp;
using ContractHttpTests.Resources;
using ContractHttpTests.Resources.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ContractHttpTests
{
    [TestClass]
    public class TestServiceUsingMvcAttributesProxyUnitTests
    {
        private TestServer testServer;

        private ITestServiceUsingMvcAttributes testService;

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

            var testServiceUsingMvcProxy = new HttpClientProxy<ITestServiceUsingMvcAttributes>(
                "http://localhost",
                httpClient);

            this.testService = testServiceUsingMvcProxy.GetProxyObject();
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
        public void CreateProxy_GetUsingMvcAttribute()
        {
            var response = this.testService.Get();
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateProxy_GetUsingMvcAttributeAsync()
        {
            var response = await this.testService.GetAsync();
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void CreateProxy_GetUsingMvcAttributeTestDataByName()
        {
            var testData = this.testService.Get("Name");
            Assert.IsNotNull(testData);
            Assert.AreEqual("Name", testData.Name);
            Assert.AreEqual("Address", testData.Address);
        }

        [TestMethod]
        public void CreateProxy_DeleteByNameUsingMvcAttribute_Good()
        {
            var response = this.testService.Delete("good");
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public void CreateProxy_DeleteByNameUsingMvcAttribute_Bad()
        {
            var response = this.testService.Delete("bad");
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateProxy_DeleteByNameUsingMvcAttributeAsync_Good()
        {
            var response = await this.testService.DeleteAsync("good");
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public void CreateProxy_UpdateModelUsingMvcAttribute_Good()
        {
            var response = this.testService.UpdateModel(
                "test",
                new TestData()
                {
                    Name = "test",
                    Address = "Somewhere"
                });
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public void CreateProxy_CreateUsingMvcAttribute_Good()
        {
            var response = this.testService.Create(
                new CreateModel()
                {
                    Name = "good",
                    Value = "value"
                });

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public void CreateProxy_CreateUsingMvcAttribute_Bad()
        {
            this.testService.Create(
                new CreateModel()
                {
                    Name = "bad",
                    Value = "value"
                });
        }

        [TestMethod]
        public async Task CreateProxy_CreateUsingMvcAttributeAsync_Good()
        {
            var response = await this.testService.CreateAsync(
                new CreateModel()
                {
                    Name = "good",
                    Value = "value"
                });

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Id);
        }

        [TestMethod]
        public void CreateProxy_CreateWithHttpResponseUsingMvcAttribute_Good()
        {
            var result = this.testService.CreateWithHttpResponse(
                new CreateModel()
                {
                    Name = "good",
                    Value = "value"
                },
                out HttpResponseMessage response);

            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Id);
        }

        [TestMethod]
        public void CreateProxy_CreateWithHttpResponseUsingMvcAttribute_Bad()
        {
            var result = this.testService.CreateWithHttpResponse(
                new CreateModel()
                {
                    Name = "bad",
                    Value = "value"
                },
                out HttpResponseMessage response);

            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.IsNull(result);
        }
    }
}
