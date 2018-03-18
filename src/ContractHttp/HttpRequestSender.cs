namespace ContractHttp
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    public class HttpRequestSender
        : IHttpRequestSender
    {
        private readonly HttpClient httpClient;

        private readonly IHttpRequestContext requestContext;

        public HttpRequestSender(HttpClient httpClient, IHttpRequestContext requestContext)
        {
            this.httpClient = httpClient;
            this.requestContext = requestContext;
        }

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
                    });
            }

            return await this.SendAsync(
                requestBuilder.Build(),
                completionOption);
        }

        /// <summary>
        /// Sends the request to the service calling pre and post actions if they are configured.
        /// </summary>
        /// <param name="httpClient">The http client to use.</param>
        /// <param name="request">The http request to send.</param>
        /// <param name="completionOption">The completion option.</param>
        /// <returns>A <see cref="HttpResponseMessage"/>.</returns>
        private async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            HttpCompletionOption completionOption)
        {

            this.requestContext.InvokeRequestAction(request);

            var response = await httpClient.SendAsync(
                request,
                completionOption,
                this.requestContext.GetCancellationToken());

            this.requestContext.InvokeResponseAction(response);

            return response;
        }
    }
}