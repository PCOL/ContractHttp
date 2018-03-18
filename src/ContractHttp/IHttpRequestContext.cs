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
        /// The requests arguments.
        /// </summary>
        object[] Arguments { get; }

        void InvokeRequestAction(HttpRequestMessage request);

        void InvokeResponseAction(HttpResponseMessage responseMessage);

        CancellationToken GetCancellationToken();
    }
}