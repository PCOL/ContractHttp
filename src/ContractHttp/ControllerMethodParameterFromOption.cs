namespace ContractHttp
{
    /// <summary>
    /// Controller method parameter from option.
    /// </summary>
    public enum ControllerMethodParameterFromOption
    {
        /// <summary>
        /// The parameter comes from the request body.
        /// </summary>
        Body,

        /// <summary>
        /// The parameter comes from a header.
        /// </summary>
        Header,

        /// <summary>
        /// The parameter comes from a query parameter.
        /// </summary>
        Query,

        /// <summary>
        /// The parameter comes from a route parameter.
        /// </summary>
        Route,
    }
}