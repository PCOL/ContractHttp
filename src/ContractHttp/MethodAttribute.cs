namespace ContractHttp
{
    using System;

    /// <summary>
    /// A base attribute class use to define a method attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class MethodAttribute
        : Attribute
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="MethodAttribute"/> class.
        /// </summary>
        protected MethodAttribute()
        {
        }

        /// <summary>
        /// Gets the path template.
        /// </summary>
        public string Template { get; protected set; }

        /// <summary>
        /// Gets or sets the content type.
        /// </summary>
        public string ContentType { get; set; }
    }
}