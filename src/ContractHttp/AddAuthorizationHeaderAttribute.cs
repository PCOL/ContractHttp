namespace ContractHttp
{
    using System;

    /// <summary>
    /// An attribute used to specifiy that an authorization header should be added to the request.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = false)]
    public class AddAuthorizationHeaderAttribute
        : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddAuthorizationHeaderAttribute"/> class.
        /// </summary>
        public AddAuthorizationHeaderAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddAuthorizationHeaderAttribute"/> class.
        /// </summary>
        /// <param name="authorizationFactoryType">The authorization header factory type.</param>
        public AddAuthorizationHeaderAttribute(Type authorizationFactoryType)
        {
            this.AuthorizationFactoryType = authorizationFactoryType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddAuthorizationHeaderAttribute"/> class.
        /// </summary>
        /// <param name="headerValue">The header value.</param>
        public AddAuthorizationHeaderAttribute(string headerValue)
        {
            this.HeaderValue = headerValue;
        }

        /// <summary>
        /// Gets the header value.
        /// </summary>
        public string HeaderValue { get; }

        /// <summary>
        /// Gets the authorization factory type.
        /// </summary>
        public Type AuthorizationFactoryType { get; }
    }
}