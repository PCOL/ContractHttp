namespace ContractHttpTests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using ContractHttp;
    using ContractHttpTests.Resources;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Tests a request sender.
    /// </summary>
    [TestClass]
    public class RequestSenderUnitTests
    {
        /// <summary>
        /// Call a get method without retry.
        /// </summary>
        [TestMethod]
        public void CreateClient_CallGetWithoutRetry_SendsRequestOnlyOnce()
        {
            var mock = new Mock<IHttpRequestSender>();
            mock.Setup(m => m.SendAsync(
                It.IsAny<IHttpRequestBuilder>(),
                It.IsAny<HttpCompletionOption>()))
                .ReturnsAsync(
                    () =>
                    {
                        Console.WriteLine("Here");
                        return new HttpResponseMessage()
                        {
                            StatusCode = HttpStatusCode.OK
                        };
                    });

            var sp = this.BuildServices(mock.Object);

            var clientProxy = new HttpClientProxy<ITestService>(
                new HttpClientProxyOptions()
                {
                    Services = sp
                });

            var client = clientProxy.GetProxyObject();
            var response = client.Get();

            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            mock.Verify(m => m.SendAsync(
                It.IsAny<IHttpRequestBuilder>(),
                It.IsAny<HttpCompletionOption>()),
                Times.Once);
        }

        private IServiceProvider BuildServices(IHttpRequestSender requestSender)
        {
            var services = new ServiceCollection();

            return services
                .AddTransient<IHttpRequestSender>(sp => requestSender)
                .AddTransient<IHttpRequestSenderFactory>(
                    sp =>
                    {
                        var factoryMock = new Mock<IHttpRequestSenderFactory>();
                        factoryMock.Setup(m => m.CreateRequestSender(
                            It.IsAny<HttpClient>(),
                            It.IsAny<IHttpRequestContext>()))
                            .Returns<HttpClient, IHttpRequestContext>(
                                (client, context) =>
                                {
                                    return sp.GetService<IHttpRequestSender>();
                                });

                        return factoryMock.Object;
                    })
                .BuildServiceProvider();
        }
    }
}