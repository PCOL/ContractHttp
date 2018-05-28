namespace ContractHttpTests.Resources
{
    using System.Net.Http;

    /// <summary>
    /// An implementation of the <see cref="IServiceResult{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    public class ServiceResult<T>
        : IServiceResult<T>
    {
        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// Gets or sets the Http response.
        /// </summary>
        public HttpResponseMessage Response { get; set; }
    }
}