namespace ContractHttp
{
    using System;

    /// <summary>
    /// An attribute used to define an Http call contract.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpCallContractAttribute
        : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpCallContractAttribute"/> class.
        /// </summary>
        /// <param name="method">The Http method to use when the request is made.</param>
        /// <param name="uri">The Uri to send the request to./></param>
        /// <param name="contentType">The content type of the request.</param>
        public HttpCallContractAttribute(HttpCallMethod method, string uri, string contentType = "application/json")
        {
            this.Method = method;
            this.Uri = uri;
            this.ContentType = contentType;
        }

        /// <summary>
        /// Gets the Http method.
        /// </summary>
        public HttpCallMethod Method { get; private set; }

        /// <summary>
        /// Gets the Uri to send the request to.
        /// </summary>
        public string Uri { get; private set; }

        /// <summary>
        /// Gets the content type.
        /// </summary>
        public string ContentType { get; private set; }

        /// <summary>
        /// Gets or sets the amount of time to wait before the request should timeout.
        /// </summary>
        public TimeSpan? Timeout { get; set; }
    }
}
