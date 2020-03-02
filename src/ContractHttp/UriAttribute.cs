namespace ContractHttp
{
    using System;

    /// <summary>
    /// An attribute to specify that the parameter is used as the url.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class UriAttribute
        : Attribute
    {
    }
}