namespace ContractHttp
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implementation of the <see cref="IHttpRequestSender"/> interface.
    /// </summary>
    public class HttpRequestSender
        : IHttpRequestSender
    {
        private readonly HttpClient httpClient;

        private readonly IHttpRequestContext requestContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestSender"/> class.
        /// </summary>
        /// <param name="httpClient">A http client.</param>
        /// <param name="methodContext">A http request context.</param>
        public HttpRequestSender(HttpClient httpClient, IHttpRequestContext methodContext)
        {
            this.httpClient = httpClient;
            this.requestContext = methodContext;
        }

        /// <summary>
        /// Sends a request.
        /// </summary>
        /// <param name="requestBuilder">A request builder.</param>
        /// <param name="completionOption">A http completion option.</param>
        /// <returns>The http response.</returns>
        public async Task<HttpResponseMessage> SendAsync(
            IHttpRequestBuilder requestBuilder,
            HttpCompletionOption completionOption)
        {
            var retryAttribute = this.requestContext.MethodInfo.GetCustomAttribute<RetryAttribute>() ??
                this.requestContext.MethodInfo.DeclaringType.GetCustomAttribute<RetryAttribute>();

            if (retryAttribute != null)
            {
                RetryHandler retry = new RetryHandler()
                    .RetryCount(retryAttribute.RetryCount)
                    .WaitTime(TimeSpan.FromMilliseconds(retryAttribute.WaitTime))
                    .MaxWaitTime(TimeSpan.FromMilliseconds(retryAttribute.MaxWaitTime))
                    .DoubleWaitTimeOnRetry(retryAttribute.DoubleWaitTimeOnRetry);

                return await retry.RetryAsync<HttpResponseMessage>(
                    () =>
                    {
                        return this.SendAsync(
                            requestBuilder.Build(),
                            completionOption);
                    },
                    (r) =>
                    {
                        if (retryAttribute.HttpStatusCodesToRetry != null)
                        {
                            return retryAttribute.HttpStatusCodesToRetry.Contains(r.StatusCode);
                        }

                        return false;
                    })
                    .ConfigureAwait(false);
            }

            return await this
                .SendAsync(
                    requestBuilder.Build(),
                    completionOption)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Sends the request to the service calling pre and post actions if they are configured.
        /// </summary>
        /// <param name="request">The http request to send.</param>
        /// <param name="completionOption">The completion option.</param>
        /// <returns>A <see cref="HttpResponseMessage"/>.</returns>
        private async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            HttpCompletionOption completionOption)
        {
            this.requestContext.InvokeRequestAction(request);

            var response = await this.httpClient
                .SendAsync(
                    request,
                    completionOption,
                    this.requestContext.GetCancellationToken())
                .ConfigureAwait(false);

            this.requestContext.InvokeResponseAction(response);

            return response;
        }
    }
}