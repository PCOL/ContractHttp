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
    public class TestServiceProxyUnitTests
    {
        private TestServer testServer;

        private HttpClientProxy<ITestService> clientProxy;

        private ITestService testService;

        [TestInitialize]
        public void TestInitialize()
        {
            this.testServer = TestUtils.CreateTestServer(
                services =>
                {
                    services.AddSingleton<TestController>();
                });

            this.clientProxy = new HttpClientProxy<ITestService>("http://localhost", testServer.CreateClient());
            this.testService = clientProxy.GetProxyObject();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            this.testServer.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateProxy_WithNullBaseUri_Throws()
        {
            new HttpClientProxy<ITestService>(null, (HttpClient)null);
        }

        [TestMethod]
        public void CreateProxy_SetupInterface_CallGet()
        {
            var response = this.testService.Get();
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void CreateProxy_GetTestDataByName()
        {
            var testData = this.testService.Get("Name");
            Assert.IsNotNull(testData);
            Assert.AreEqual("Name", testData.Name);
            Assert.AreEqual("Address", testData.Address);
        }

        [TestMethod]
        public void CreateProxy_DeleteByName_Good()
        {
            var response = this.testService.Delete("good");
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public void CreateProxy_DeleteByName_Bad()
        {
            var response = this.testService.Delete("bad");
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void CreateProxy_GetUsingMvcAttribute()
        {
            var response = this.testService.GetUsingMvcAttribute();
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateProxy_GetUsingMvcAttributeAsync()
        {
            var response = await this.testService.GetUsingMvcAttributeAsync();
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public void CreateProxy_GetUsingMvcAttributeTestDataByName()
        {
            var testData = this.testService.GetUsingMvcAttribute("Name");
            Assert.IsNotNull(testData);
            Assert.AreEqual("Name", testData.Name);
            Assert.AreEqual("Address", testData.Address);
        }

        [TestMethod]
        public void CreateProxy_DeleteByNameUsingMvcAttribute_Good()
        {
            var response = this.testService.DeleteUsingMvcAttribute("good");
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public void CreateProxy_DeleteByNameUsingMvcAttribute_Bad()
        {
            var response = this.testService.DeleteUsingMvcAttribute("bad");
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateProxy_DeleteByNameUsingMvcAttributeAsync_Good()
        {
            var response = await this.testService.DeleteUsingMvcAttributeAsync("good");
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public void CreateProxy_UpdateModelUsingMvcAttribute_Good()
        {
            var response = this.testService.UpdateModelUsingMvcAttribute(
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
            var response = this.testService.CreateUsingMvcAttibute(
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
            this.testService.CreateUsingMvcAttibute(
                new CreateModel()
                {
                    Name = "bad",
                    Value = "value"
                });
        }

        [TestMethod]
        public async Task CreateProxy_CreateUsingMvcAttributeAsync_Good()
        {
            var response = await this.testService.CreateUsingMvcAttibuteAsync(
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
            var result = this.testService.CreateWithHttpResponseUsingMvcAttibute(
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
            var result = this.testService.CreateWithHttpResponseUsingMvcAttibute(
                new CreateModel()
                {
                    Name = "bad",
                    Value = "value"
                },
                out HttpResponseMessage response);

            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.Conflict , response.StatusCode);
            Assert.IsNull(result);
        }
    }
}
