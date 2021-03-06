namespace ContractHttp
{
    using System;

    /// <summary>
    /// An attribute for http model validation.
    /// </summary>
    public class HttpModelValidationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpModelValidationAttribute"/> class.
        /// </summary>
        public HttpModelValidationAttribute()
        {
        }

        /// <summary>
        /// Gets or sets the status code to return if the model is not valid.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the model to return if the validation fails.
        /// </summary>
        public Type ModelType { get; set; }
    }
}