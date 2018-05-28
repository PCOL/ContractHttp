namespace ContractHttp
{
    using System.Net.Http;
    using System.Reflection;
    using System.Threading;

    /// <summary>
    /// Defines the http request context.
    /// </summary>
    public interface IHttpRequestContext
    {
        /// <summary>
        /// Gets the method info.
        /// </summary>
        MethodInfo MethodInfo { get; }

        /// <summary>
        /// Gets the requests arguments.
        /// </summary>
        object[] Arguments { get; }

        /// <summary>
        /// Invokes a request action.
        /// </summary>
        /// <param name="request">A http request.</param>
        void InvokeRequestAction(HttpRequestMessage request);

        /// <summary>
        /// Invokes a response action.
        /// </summary>
        /// <param name="responseMessage">A http response.</param>
        void InvokeResponseAction(HttpResponseMessage responseMessage);

        /// <summary>
        /// Gets a cancellation token.
        /// </summary>
        /// <returns>A cancellation token.</returns>
        CancellationToken GetCancellationToken();
    }
}