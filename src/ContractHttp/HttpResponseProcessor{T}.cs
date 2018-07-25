namespace ContractHttp
{
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// Base class used to implement Http response processors.
    /// </summary>
    /// <typeparam name="T">The response type.</typeparam>
    public abstract class HttpResponseProcessor<T>
    {
        /// <summary>
        /// Processes a response.
        /// </summary>
        /// <param name="response">A <see cref="HttpResponseMessage"/>.</param>
        /// <returns>A result.</returns>
        public abstract Task<T> ProcessResponseAsync(HttpResponseMessage response);
    }
}