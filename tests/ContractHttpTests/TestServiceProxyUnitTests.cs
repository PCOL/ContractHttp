namespace ContractHttpTests
{
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

    /// <summary>
    /// Tests a service proxy.
    /// </summary>
    [TestClass]
    public class TestServiceProxyUnitTests
    {
        private TestServer testServer;

        private ITestService testService;

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
            var testServiceProxy = new HttpClientProxy<ITestService>(
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
        /// Creating a proxy with a null base Url throws an exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateProxy_WithNullBaseUri_Throws()
        {
            new HttpClientProxy<ITestService>(null, null);
        }

        /// <summary>
        /// Creates a proxy and calls a simple get method.
        /// </summary>
        [TestMethod]
        public void CreateProxy_SetupInterface_CallGet()
        {
            var response = this.testService.Get();
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Creates a proxy and calls a method with a single parameter.
        /// </summary>
        [TestMethod]
        public void CreateProxy_GetTestDataByName()
        {
            var testData = this.testService.Get("Name");
            Assert.IsNotNull(testData);
            Assert.AreEqual("Name", testData.Name);
            Assert.AreEqual("Address", testData.Address);
        }

        /// <summary>
        /// Creates a proxy and calls a create method with a good response.
        /// </summary>
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

        /// <summary>
        /// Creates a proxy and calls a create method with a bad response.
        /// </summary>
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

        /// <summary>
        /// Creates a proxy and calls a create method with form url encoding.
        /// </summary>
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

        /// <summary>
        /// Creates a proxy and calls a create method with form url encoding that
        /// returns a bad response.
        /// </summary>
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

        /// <summary>
        /// Creates a proxy and calls a create method with form url encoding.
        /// </summary>
        [TestMethod]
        public void CallCreate_WithFormUrlEncoded_Good()
        {
            var response = this.testService.CreateFormUrlEncoded("good", "test");

            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        /// <summary>
        /// Creates a proxy and calls a create method with form url encoding that
        /// returns a bad response.
        /// </summary>
        [TestMethod]
        public void CallCreate_WithFormUrlEncoded_Bad()
        {
            var response = this.testService.CreateFormUrlEncoded("bad", "test");

            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        /// <summary>
        /// Creates a proxy and calls a delete by name method that returns a good response.
        /// </summary>
        [TestMethod]
        public void CallDeleteByName_WithUriSegmentName_Good()
        {
            var response = this.testService.Delete("good");
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        /// <summary>
        /// Creates a proxy and calls a delete by name method that returns a bad response.
        /// </summary>
        [TestMethod]
        public void CallDeleteByName_WithUriSegmentName_Bad()
        {
            var response = this.testService.Delete("bad");
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        /// <summary>
        /// Creates a proxy and calls a delete by name method using a query string that
        /// returns a good response.
        /// </summary>
        [TestMethod]
        public void CallDeleteByName_WithQueryStringName_Good()
        {
            var response = this.testService.DeleteUsingQueryString("good");
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        /// <summary>
        /// Creates a proxy and calls a delete by name method using a query string that
        /// returns a bad response.
        /// </summary>
        [TestMethod]
        public void CallDeleteByName_WithQueryStringName_Bad()
        {
            var response = this.testService.DeleteUsingQueryString("bad");
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        /// <summary>
        /// Creates a proxy and calls a get by id method that returns a <see cref="IServiceResult{T}"/> type.
        /// </summary>
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

        /// <summary>
        /// Creates a proxy and calls a get by id method that returns a value uisng a
        /// <see cref="FromJsonAttribute"/>.
        /// </summary>
        [TestMethod]
        public void CallGetById_WithFromJsonAttribute()
        {
            var response = this.testService.GetById("id", out string name);
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Name", name);
        }

        /// <summary>
        /// Creates a proxy and calls a get by id method that returns a value uisng a
        /// <see cref="FromModelAttribute"/>.
        /// </summary>
        [TestMethod]
        public void CallGetById_WithFromModelAttribute()
        {
            var response = this.testService.GetAddressById("id", out string address);
            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Address", address);
        }

        /// <summary>
        /// Call <see cref="ITestService.GetByIdUsingResponseProcessor(string)"/> with response processor
        /// </summary>
        [TestMethod]
        public void CallGetById_WithResponseProcessor()
        {
            var result = this.testService.GetByIdUsingResponseProcessor("id");
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual("Address", result.Result.Address);
        }

        /// <summary>
        /// Call <see cref="ITestService.GetByIdUsingResponseProcessorAsync(string)"/> with response processor async.
        /// </summary>
        /// <returns>A task.</returns>
        [TestMethod]
        public async Task CallGetById_WithResponseProcessorAsync()
        {
            var result = await this.testService.GetByIdUsingResponseProcessorAsync("id");
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Result);
            Assert.AreEqual("Address", result.Result.Address);
        }

        /// <summary>
        /// Creates a proxy and calls a get all method that returns a <see cref="HttpResponseMessage"/>
        /// in the return type.
        /// </summary>
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

        /// <summary>
        /// Calls a none existent uri.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public void CallGetNotExists()
        {
            var result = this.testService.GetNotExists();
        }
    }
}
