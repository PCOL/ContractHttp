namespace ContractHttp
{
    using System;

    /// <summary>
    /// Represents an authorization header factory.
    /// </summary>
    internal class AuthorizationHeaderFactory
        : IAuthorizationHeaderFactory
    {
        private string scheme;

        private Func<string> getAuthHeaderValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationHeaderFactory"/> class.
        /// </summary>
        /// <param name="scheme">The authorization scheme.</param>
        /// <param name="getAuthHeaderValue">A function to get the authorization header value.</param>
        public AuthorizationHeaderFactory(
            string scheme, Func<string> getAuthHeaderValue)
        {
            this.scheme = scheme;
            this.getAuthHeaderValue = getAuthHeaderValue;
        }

        /// <summary>
        /// Gets the authorization header scheme.
        /// </summary>
        /// <returns>The scheme.</returns>
        public string GetAuthorizationHeaderScheme()
        {
            return this.scheme;
        }

        /// <summary>
        /// Gets the authorization header value.
        /// </summary>
        /// <returns>The value.</returns>
        public string GetAuthorizationHeaderValue()
        {
            return this.getAuthHeaderValue?.Invoke();
        }
    }
}