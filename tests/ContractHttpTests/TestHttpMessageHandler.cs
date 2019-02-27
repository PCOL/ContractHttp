namespace ContractHttpTests
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// A test http message handler.
    /// </summary>
    public class TestHttpMessageHandler
        : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestHttpMessageHandler"/> class.
        /// </summary>
        /// <param name="handler">A message handler.</param>
        public TestHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        {
            this.handler = handler;
        }

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return this.handler(request);
        }
    }
}