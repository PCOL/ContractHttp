namespace ContractHttp
{
    using System;

    /// <summary>
    /// An attribute to declare an interface as an Http client contract.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Interface)]
    public class HttpClientContractAttribute
        : Attribute
    {
        /// <summary>
        /// Gets or sets the Uri to send the request to.
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// Gets or sets the content type.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the amount of time to wait before the request should timeout.
        /// </summary>
        public TimeSpan? Timeout { get; set; }
    }
}
