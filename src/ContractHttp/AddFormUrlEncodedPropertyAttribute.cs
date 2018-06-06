namespace ContractHttp
{
    using System;

    /// <summary>
    /// An attributed used to specify that a form url encoded property should be added to the
    /// request.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class AddFormUrlEncodedPropertyAttribute
        : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddFormUrlEncodedPropertyAttribute"/> class.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        public AddFormUrlEncodedPropertyAttribute(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }

        /// <summary>
        /// Gets the property key.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets the property value.
        /// </summary>
        public string Value { get; }
    }
}