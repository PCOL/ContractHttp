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
    /// <typeparam name="U">The calls data type.</typeparam>
    internal class AsyncCall<T>
    {
        /// <summary>
        /// A reterence to the <see cref="IHttpRequestSender"/>
        /// </summary>
        private readonly IHttpRequestSender requestSender;

        private readonly HttpRequestContext httpContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCall{T}"/> class.
        /// </summary>
        /// <param name="requestSender">A <see cref="IHttpRequestSender"/></param>
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
            var response = await this.requestSender.SendAsync(
                requestBuilder,
                completionOption);

            if (dataType == typeof(Stream))
            {
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStreamAsync();
                return (T)(object)result;
            }

            return (T) this.httpContext.ProcessResult(response, typeof(T));
        }
    }
}