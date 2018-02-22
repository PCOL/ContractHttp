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
    }
}