namespace ContractHttp
{
    using System;

    /// <summary>
    /// Specifies an exception handler.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ExceptionHandlerAttribute
        : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlerAttribute"/> class.
        /// </summary>
        /// <param name="exceptionType">The excedption type.</param>
        /// <param name="statusCode">The status code to return.</param>
        public ExceptionHandlerAttribute(Type exceptionType, int statusCode)
        {
            this.ExceptionType = exceptionType;
            this.StatusCode = statusCode;
        }

        /// <summary>
        /// Gets the excpetion type.
        /// </summary>
        public Type ExceptionType { get; }

        /// <summary>
        /// Gets the status code.
        /// </summary>
        public int StatusCode { get; }

        /// <summary>
        /// Gets or sets a model type.
        /// </summary>
        public Type ModelType { get; set; }

        /// <summary>
        /// Gets or sets the header to return the error message in.
        /// </summary>
        public string ResponseHeader { get; set; }
    }
}