namespace ContractHttp
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an authorization header factory.
    /// </summary>
    internal class AuthorizationHeaderFactory
        : IAuthorizationHeaderFactory
    {
        private string scheme;

        private Func<string> getAuthHeaderValue;

        private Func<Task<string>> getAuthHeaderValueAsync;

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
        /// Initializes a new instance of the <see cref="AuthorizationHeaderFactory"/> class.
        /// </summary>
        /// <param name="scheme">The authorization scheme.</param>
        /// <param name="getAuthHeaderValue">A function to get the authorization header value.</param>
        public AuthorizationHeaderFactory(
            string scheme, Func<Task<string>> getAuthHeaderValue)
        {
            this.scheme = scheme;
            this.getAuthHeaderValueAsync = getAuthHeaderValue;
        }

        /// <inheritdoc />
        public string GetAuthorizationHeaderScheme()
        {
            return this.scheme;
        }

        /// <inheritdoc />
        public string GetAuthorizationHeaderValue()
        {
            if (this.getAuthHeaderValue != null)
            {
                return this.getAuthHeaderValue();
            }

            return this.getAuthHeaderValueAsync?.Invoke()?.Result;
        }

        /// <inheritdoc />
        public async Task<string> GetAuthorizationHeaderValueAsync()
        {
            if (this.getAuthHeaderValueAsync != null)
            {
                return await this.getAuthHeaderValueAsync();
            }

            return this.getAuthHeaderValue?.Invoke();
        }
    }
}