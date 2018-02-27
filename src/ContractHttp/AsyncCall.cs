namespace ContractHttp
{
    using System;
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
        /// A reterence to the <see cref="MethodInfo"/>
        /// </summary>
        private readonly MethodInfo methodInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncCall{T}"/> class.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/> to use.</param>
        /// <param name="methodInfo">A <see cref="MethodInfo"/></param>
        public AsyncCall(HttpClient httpClient, MethodInfo methodInfo)
        {
            this.httpClient = httpClient;
            this.methodInfo = methodInfo;
        }

        /// <summary>
        /// Sends a request.
        /// </summary>
        /// <param name="request">The request to send.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        public Task<T> SendAsync(HttpRequestMessage request)
        {
            Type dataType = typeof(T);
            Task<HttpResponseMessage> task = this.httpClient.SendAsync(request);
            if (dataType == typeof(HttpResponseMessage))
            {
                return (Task<T>)(object)task;
            }

            return task.ContinueWith<T>(
                (t) =>
                {
                    if (t.IsFaulted == true)
                    {
                        throw t.Exception;
                    }

                    HttpResponseMessage response = ((Task<HttpResponseMessage>)t).Result;

                    if (dataType != typeof(HttpResponseMessage))
                    {
                        response.EnsureSuccessStatusCode();
                    }

                    T result = default(T);
                    if (dataType != typeof(void))
                    {
                        string content = response.Content.ReadAsStringAsync().Result;

                        var fromJsonAttr = this.methodInfo.ReturnParameter.GetCustomAttribute<FromJsonAttribute>();
                        if (fromJsonAttr != null)
                        {
                            result = (T) (object)fromJsonAttr.JsonToObject(content, dataType);
                        }
                        else
                        {
                            if (dataType == typeof(string))
                            {
                                return (T)(object)content;
                            }

                            if (content.IsNullOrEmpty() == false)
                            {
                                result = JsonConvert.DeserializeObject<T>(content);
                            }
                        }
                    }

                    return result;
                });
        }
    }
}