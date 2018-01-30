namespace ContractHttp
{
    using System;

    /// <summary>
    /// An attribute to declare an interface as an Http client contract.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class HttpClientContractAttribute
        : Attribute
    {
        /// <summary>
        /// Gets the Uri to send the request to.
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// Gets the content type.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the amount of time to wait before the request should timeout.
        /// </summary>
        public TimeSpan? Timeout { get; set; }
    }
}
