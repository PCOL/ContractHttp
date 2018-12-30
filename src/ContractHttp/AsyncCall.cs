namespace ContractHttp
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents a typed asynchronous call.
    /// </summary>
    /// <typeparam name="T">The calls return type.</typeparam>
    internal class AsyncCall<T>
    {
        /// <summary>
        /// A reference to the <see cref="IHttpRequestSender"/>
        /// </summary>
        private readonly IHttpRequestSender requestSender;

        /// <summary>
        /// A reference to the request context.
        /// </summary>
        private readonly HttpRequestContext httpContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCall{T}"/> class.
        /// </summary>
        /// <param name="requestSender">A <see cref="IHttpRequestSender"/></param>
        /// <param name="httpContext">A <see cref="HttpRequestContext"/></param>
        public AsyncCall(
            IHttpRequestSender requestSender,
            HttpRequestContext httpContext)
        {
            this.requestSender = requestSender;
            this.httpContext = httpContext;
        }

        /// <summary>
        /// Sends a request.
        /// </summary>
        /// <param name="requestBuilder">The request builder.</param>
        /// <param name="completionOption">The completion option.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        public async Task<T> SendAsync(HttpRequestBuilder requestBuilder, HttpCompletionOption completionOption)
        {
            var dataType = typeof(T);
            var response = await this.requestSender
                .SendAsync(
                    requestBuilder,
                    completionOption)
                .ConfigureAwait(false);

            if (dataType == typeof(Stream))
            {
                response.EnsureSuccessStatusCode();

                var result = await response.Content
                    .ReadAsStreamAsync()
                    .ConfigureAwait(false);

                return (T)(object)result;
            }

            return (T)this.httpContext.ProcessResult(response, typeof(T));
        }
    }
}