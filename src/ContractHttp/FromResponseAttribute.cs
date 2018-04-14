namespace ContractHttp
{
    using System;
    using System.Net.Http;

    /// <summary>
    /// An abstract attribute class used for creating attributes that convert a <see cref="HttpResponseMessage"/>
    /// into a type.
    /// </summary>
    public abstract class FromResponseAttribute
        : Attribute
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="FromResponseAttribute"/> class.
        /// </summary>
        protected FromResponseAttribute()
        {
        }

        /// <summary>
        /// Gets or sets the content type.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Converts the given <see cref="HttpResponseMessage"/> into the required type.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
        /// <param name="dataType">The required type.</param>
        /// <param name="serializer">A <see cref="IObjectSerializer"/> for the responses contnet type.</param>
        /// <returns></returns>
        public abstract object ToObject(HttpResponseMessage response, Type dataType, IObjectSerializer serializer);
    }
}