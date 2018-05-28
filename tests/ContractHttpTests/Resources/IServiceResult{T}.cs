namespace ContractHttpTests.Resources
{
    using System.Net.Http;

    /// <summary>
    /// Defines an interface for a service result.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    public interface IServiceResult<T>
    {
        /// <summary>
        /// Gets the result.
        /// </summary>
        T Result { get; }

        /// <summary>
        /// Gets the Http response.
        /// </summary>
        HttpResponseMessage Response { get; }
    }
}