namespace ContractHttpTests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using ContractHttp;
    using ContractHttpTests.Resources;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Dynamic Uri Unit Tests.
    /// </summary>
    [TestClass]
    public class DynamicUriUnitTests
    {
        /// <summary>
        /// Call a service with a <see ref="UriAttribute"/> parameter.
        /// </summary>
        /// <returns>A task.</returns>
        [TestMethod]
        public async Task CallService_WithUriAttributeParameter()
        {
            const string Url = "http://dynamic.com";
            var id = Guid.NewGuid().ToString();

            Uri requestUri = null;
            var mock = new Mock<IHttpRequestSender>();
            mock.Setup(m => m.SendAsync(
                It.IsAny<IHttpRequestBuilder>(),
                It.IsAny<HttpCompletionOption>()))
                .Returns<IHttpRequestBuilder, HttpCompletionOption>(
                    (builder, options) =>
                    {
                        var request = builder.Build();
                        requestUri = request.RequestUri;

                        return Task.FromResult(
                            new HttpResponseMessage()
                            {
                                StatusCode = HttpStatusCode.OK
                            });
                    });

            var sp = this.BuildServices(mock.Object);

            var clientProxy = new HttpClientProxy<IDynamicUriClient>(
                new HttpClientProxyOptions()
                {
                    Services = sp
                });

            var client = clientProxy.GetProxyObject();
            var response = await client.GetWidgetAsync(Url, id);

            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(requestUri);
            Assert.AreEqual($"{Url}/api/test/widgets/{id}", requestUri.ToString());

            mock.Verify(m => m.SendAsync(
                It.IsAny<IHttpRequestBuilder>(),
                It.IsAny<HttpCompletionOption>()),
                Times.Once);
        }

        /// <summary>
        /// Call a service with a <see ref="IUriBuilder"/> parameter.
        /// </summary>
        /// <returns>A task.</returns>
        [TestMethod]
        public async Task CallService_WithUriBuilderParameter()
        {
            const string Url = "http://dynamic.com";
            var id = Guid.NewGuid().ToString();

            Uri requestUri = null;
            var mock = new Mock<IHttpRequestSender>();
            mock.Setup(m => m.SendAsync(
                It.IsAny<IHttpRequestBuilder>(),
                It.IsAny<HttpCompletionOption>()))
                .Returns<IHttpRequestBuilder, HttpCompletionOption>(
                    (builder, options) =>
                    {
                        var request = builder.Build();
                        requestUri = request.RequestUri;

                        return Task.FromResult(
                            new HttpResponseMessage()
                            {
                                StatusCode = HttpStatusCode.OK
                            });
                    });

            var sp = this.BuildServices(mock.Object);

            var clientProxy = new HttpClientProxy<IDynamicUriClient>(
                new HttpClientProxyOptions()
                {
                    Services = sp
                });

            var mockUriBuilder = new Mock<IUriBuilder>();
            mockUriBuilder.Setup(m => m.BuildUri()).Returns(new Uri(Url));

            var client = clientProxy.GetProxyObject();
            var response = await client.GetWidgetAsync(mockUriBuilder.Object, id);

            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(requestUri);
            Assert.AreEqual($"{Url}/api/test/widgets/{id}", requestUri.ToString());

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