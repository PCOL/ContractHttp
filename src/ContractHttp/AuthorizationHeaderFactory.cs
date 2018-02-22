namespace ContractHttp
{
    using System;

    internal class AuthorizationHeaderFactory
        : IAuthorizationHeaderFactory
    {
        private string scheme;

        private Func<string> getAuthHeaderValue;

        public AuthorizationHeaderFactory(string scheme, Func<string> getAuthHeaderValue)
        {
            this.scheme = scheme;
            this.getAuthHeaderValue = getAuthHeaderValue;
        }

        public string GetAuthorizationHeaderScheme()
        {
            return this.scheme;
        }

        public string GetAuthorizationHeaderValue()
        {
            return this.getAuthHeaderValue?.Invoke();
        }
    }
}