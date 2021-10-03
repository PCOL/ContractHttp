namespace ContractHttpTests
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using ContractHttp;
    using ContractHttpTests.Resources;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Multipart Form Unit Tests.
    /// </summary>
    [TestClass]
    public class MultipartFormUnitTests
    {
        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void Test()
        {
            var handler = new TestHttpMessageHandler(
                async (req) =>
                {
                    await Task.Yield();
                    if (req.Content is MultipartFormDataContent multipartFormData)
                    {
                        if (req.Content.Headers.ContentType.ToString().StartsWith("multipart/"))
                        {
                            return new HttpResponseMessage(HttpStatusCode.OK);
                        }
                    }

                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                });

            var httpClient = new HttpClient(handler);
            var testContract = new HttpClientProxy<ITestMultipart>(
                "http://localhost",
                new HttpClientProxyOptions()
                {
                    HttpClient = httpClient
                });

            var proxy = testContract.GetProxyObject();

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("Hello World")))
            {
                var response = proxy.UploadFile("myfile.png", stream);

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }
        }

        /// <summary>
        /// Test Async.
        /// </summary>
        /// <returns>A task.</returns>
        [TestMethod]
        public async Task TestAsync()
        {
            var handler = new TestHttpMessageHandler(
                async (req) =>
                {
                    await Task.Yield();
                    if (req.Content is MultipartFormDataContent multipartFormData)
                    {
                        if (req.Content.Headers.ContentType.ToString().StartsWith("multipart/"))
                        {
                            return new HttpResponseMessage(HttpStatusCode.OK);
                        }
                    }

                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                });

            var httpClient = new HttpClient(handler);
            var testContract = new HttpClientProxy<ITestMultipart>(
                "http://localhost",
                new HttpClientProxyOptions()
                {
                    HttpClient = httpClient
                });

            var proxy = testContract.GetProxyObject();

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("Hello World")))
            {
                var response = await proxy.UploadFileAsync("myfile.png", stream);

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }
        }
    }
}