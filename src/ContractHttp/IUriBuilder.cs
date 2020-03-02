namespace ContractHttp
{
    using System;

    /// <summary>
    /// Defines the Uri builder interface.
    /// </summary>
    public interface IUriBuilder
    {
        /// <summary>
        /// Builds a Uri.
        /// </summary>
        /// <returns>The built Uri.</returns>
        Uri BuildUri();
    }
}