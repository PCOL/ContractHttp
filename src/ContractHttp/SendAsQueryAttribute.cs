namespace ContractHttp
{
    using System;
    using System.Text;

    /// <summary>
    /// An attribute used to state that a parameter should be sent as a query parameter in the request.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Parameter)]
    public class SendAsQueryAttribute
        : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendAsQueryAttribute"/> class.
        /// </summary>
        public SendAsQueryAttribute()
        {
            this.Encoding = Encoding.UTF8;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendAsQueryAttribute"/> class.
        /// </summary>
        /// <param name="name">The query parameters name.</param>
        public SendAsQueryAttribute(string name)
            : this()
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets the query key name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the format.
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the query value encoding.
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the value should be base64 encoded.
        /// </summary>
        public bool Base64 { get; set; }

        /// <summary>
        /// Gets or sets the type that handles custom serialization for the query string.
        /// </summary>
        public Type SerializerType { get; set; }
    }
}