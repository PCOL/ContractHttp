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
        /// A reference to the client.
        /// </summary>
        private readonly HttpClient httpClient;

        /// <summary>
        /// A reterence to the <see cref="HttpRequestContext"/>
        /// </summary>
        private readonly HttpRequestContext httpContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCall{T}"/> class.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> to use.</param>
        /// <param name="httpContext">A <see cref="HttpRequestContext"/></param>
        public AsyncCall(HttpClient httpClient, HttpRequestContext httpContext)
        {
            this.httpClient = httpClient;
            this.httpContext = httpContext;
        }

        /// <summary>
        /// Sends a request.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <param name="completionOption">The completion option.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        public Task<T> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption)
        {
            var dataType = typeof(T);
            var task = this.httpClient
                .SendAsync(request, completionOption, httpContext.CancellationToken);

            if (dataType == typeof(Stream))
            {
                var response = task.Result;
                response.EnsureSuccessStatusCode();

                var result = response.Content.ReadAsStreamAsync();
                return (Task<T>)(object)result;
            }

            return task.ContinueWith<T>(
                    (t) =>
                    {
                        if (t.IsFaulted == true)
                        {
                            throw t.Exception;
                        }

                        HttpResponseMessage response = ((Task<HttpResponseMessage>)t).Result;

                        this.httpContext.InvokeResponseAction(response);

                        return (T) this.httpContext.ProcessResult(response, typeof(T));
                    });
        }
    }
}