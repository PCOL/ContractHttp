namespace ContractHttp
{
    using System;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = false)]
    public class AddAuthorizationHeaderAttribute
        : Attribute
    {
        public AddAuthorizationHeaderAttribute()
        {
        }

        public AddAuthorizationHeaderAttribute(Type authorizationFactoryType)
        {
            this.AuthorizationFactoryType = authorizationFactoryType;
        }

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