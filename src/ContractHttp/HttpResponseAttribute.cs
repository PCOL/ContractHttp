namespace ContractHttp
{
    using System;

    /// <summary>
    /// A http response attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class HttpResponseAttribute
        : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseAttribute"/> class.
        /// </summary>
        /// <param name="modelType">The model type to return.</param>
        /// <param name="successStatusCode">The success status code.</param>
        /// <param name="failureErrorCode">The failre status code.</param>
        public HttpResponseAttribute(Type modelType, int successStatusCode, int failureErrorCode)
        {
            this.ModelType = modelType;
            this.SuccessStatusCode = successStatusCode;
            this.FailureStatusCode = failureErrorCode;
        }

        /// <summary>
        /// Gets the model type to return.
        /// </summary>
        public Type ModelType { get; }

        /// <summary>
        /// Gets the status code to return if the operation is successful.
        /// </summary>
        public int SuccessStatusCode { get; }

        /// <summary>
        /// Gets the status code to return if the operation is unsuccessful.
        /// </summary>
        public int FailureStatusCode { get; }
    }
}