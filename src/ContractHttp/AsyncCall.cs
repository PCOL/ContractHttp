namespace ContractHttp
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a typed asynchronous call.
    /// </summary>
    /// <typeparam name="U">The calls data type.</typeparam>
    internal class AsyncCall<T>
    {
        /// <summary>
        /// A reference to a client.
        /// </summary>
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCall{T}"/> class.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> to use.</param>
        public AsyncCall(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        // /// <summary>
        // /// Represents a get call.
        // /// </summary>
        // /// <param name="uri">The uri to call.</param>
        // /// <returns>A <see cref="Task"/>.</returns>
        // public Task<T> GetAsync(string uri, string contentType)
        // {
        //     var request = HttpClientProxy<T>.CreateRequest(HttpMethod.Get, uri);
        //     return this.SendAsync(request);
        // }

        /// <summary>
        /// Sends a request.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        public Task<T> SendAsync(HttpRequestMessage request)
        {
            Type dataType = typeof(T);
            Task<HttpResponseMessage> task = this.httpClient.SendAsync(request);
            return task.ContinueWith<T>(
                (t) =>
                {
                    if (t.IsFaulted == true)
                    {
                        throw t.Exception;
                    }

                    HttpResponseMessage response = ((Task<HttpResponseMessage>)t).Result;
                    T result = default(T);

                    if (dataType != typeof(void))
                    {
                        string json = response.Content.ReadAsStringAsync().Result;
                        if (json.IsNullOrEmpty() == false)
                        {
                            result = JsonConvert.DeserializeObject<T>(json);
                        }
                    }

                    if (dataType != typeof(HttpResponseMessage))
                    {
                        response.EnsureSuccessStatusCode();
                    }

                    return result;
                });
        }
    }
}