namespace ContractHttp
{
    using System;

    /// <summary>
    /// An attribute used to indicate that a parmeter shoud be used as a content disposition property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class SendAsContentDispositionAttribute
        : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendAsContentDispositionAttribute"/> class.
        /// </summary>
        public SendAsContentDispositionAttribute()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the parameter speicifes the name property.
        /// </summary>
        public bool IsName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the parameter speicifes the filename property.
        /// </summary>
        public bool IsFileName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the parameter speicifes the filenamestar property.
        /// </summary>
        public bool IsFileNameStar { get; set; }

        /// <summary>
        /// Gets or sets the name of a parameter key.
        /// </summary>
        public string ParameterKey { get; set; }
    }
}