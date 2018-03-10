namespace ContractHttp
{
    using System;
    using System.Text;

    /// <summary>
    /// An attribute which specifies that the parameter is provided in the requests content.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class SendAsContentAttribute
        : Attribute
    {
        /// <summary>
        /// Gets or sets the content type.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the encoding.
        /// </summary>
        public Encoding Encoding { get; set; }
    }
}
