namespace ContractHttp
{
    using System;

    /// <summary>
    /// An attribute which specifies that the parameter is provided in the requests content.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class SendAsContentAttribute
        : Attribute
    {
    }
}
