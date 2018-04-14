using System;
using System.Collections.Generic;
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

        private ITestService testService;

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
            var testServiceProxy = new HttpClientProxy<ITestService>(
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateProxy_WithNullBaseUri_Throws()
        {
            new HttpClientProxy<ITestService>(null, null);
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
        public void CreateProxy_Create_Good()
        {
            var response = this.testService.Create(
                new CreateModel()
                {
                    Name = "good",
                    Value = "Test"
                });
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [TestMethod]
        public void CreateProxy_Create_Bad()
        {
            var response = this.testService.Create(
                new CreateModel()
                {
                    Name = "bad",
                    Value = "Test"
                });
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [TestMethod]
        public void CreateProxy_Create_FormUrlEncodedDictionary_Good()
        {
            var response = this.testService.CreateFormUrlEncoded(
                new Dictionary<string, string>()
                {
                    { "Name", "good" },
                    { "Value", "test" }
                });

            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [TestMethod]
        public void CreateProxy_Create_FormUrlEncodedDictionary_Bad()
        {
            var response = this.testService.CreateFormUrlEncoded(
                new Dictionary<string, string>()
                {
                    { "Name", "bad" },
                    { "Value", "test" }
                });

            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [TestMethod]
        public void CallCreate_WithFormUrlEncoded_Good()
        {
            var response = this.testService.CreateFormUrlEncoded("good", "test");

            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [TestMethod]
        public void CallCreate_WithFormUrlEncoded_Bad()
        {
            var response = this.testService.CreateFormUrlEncoded("bad", "test");

            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [TestMethod]
        public void CallDeleteByName_WithUriSegmentName_Good()
        {
            var response = this.testService.Delete("good");
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public void CallDeleteByName_WithUriSegmentName_Bad()
        {
            var response = this.testService.Delete("bad");
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [TestMethod]
        public void CallDeleteByName_WithQueryStringName_Good()
        {
            var response = this.testService.DeleteUsingQueryString("good");
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [TestMethod]
        public void CallDeleteByName_WithQueryStringName_Bad()
        {
            var response = this.testService.DeleteUsingQueryString("bad");
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [TestMethod]
        public void CallGetById_WithServiceResultReturnType()
        {
            var result = this.testService.GetById("id");
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Response);
            Assert.AreEqual(HttpStatusCode.OK, result.Response.StatusCode);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual("Name", result.Result.Name);
            Assert.AreEqual("Address", result.Result.Address);
        }

        [TestMethod]
        public void CallGetById_WithFromJsonAttribute()
        {
            var response = this.testService.GetById("id", out string name);
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Name", name);
        }

        [TestMethod]
        public void CallGetById_WithFromModelAttribute()
        {
            var response = this.testService.GetAddressById("id", out string address);
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Address", address);
        }

        [TestMethod]
        public void CallGetAll_WithResponseInReturnType()
        {
            var results = this.testService.GetAll();
            Assert.IsNotNull(results);
            foreach (var item in results)
            {
                Assert.AreEqual("Name", item.Name);
                Assert.AreEqual("Address", item.Address);
                Assert.IsNotNull(item.Response);
                Assert.AreEqual(HttpStatusCode.OK, item.Response.StatusCode);
            }
        }
    }
}
