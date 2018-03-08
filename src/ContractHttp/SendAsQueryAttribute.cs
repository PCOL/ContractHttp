namespace ContractHttp
{
    using System;
    using System.Text;

    [AttributeUsage(AttributeTargets.Parameter)]
    public class SendAsQueryAttribute
        : Attribute
    {
        public SendAsQueryAttribute(string name)
        {
            this.Name = name;
            this.Encoding = Encoding.UTF8;
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
    }
}