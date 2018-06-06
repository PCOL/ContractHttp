namespace ContractHttp
{
    using System;

    /// <summary>
    /// An attribute used to indicate a parameter should be sent form url encoded.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Parameter)]
    public class SendAsFormUrlAttribute
        : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendAsFormUrlAttribute"/> class.
        /// </summary>
        public SendAsFormUrlAttribute()
        {
        }

        /// <summary>
        /// Gets or sets the paramter name.
        /// </summary>
        public string Name { get; set; }
    }
}