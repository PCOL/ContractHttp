namespace ContractHttp
{
    /// <summary>
    /// Defines the authorization header factory.
    /// </summary>
    public interface IAuthorizationHeaderFactory
    {
        /// <summary>
        /// Gets the authorization header scheme.
        /// </summary>
        /// <returns>The authorization header scheme.</returns>
        string GetAuthorizationHeaderScheme();

        /// <summary>
        /// Gets the authorization header value.
        /// </summary>
        /// <returns>The authorization header value.</returns>
        string GetAuthorizationHeaderValue();
    }
}