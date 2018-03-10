namespace ContractHttp
{
    using System;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class MethodAttribute
        : Attribute
    {
        protected MethodAttribute()
        {
        }

        public string Template { get; protected set; }

        public string ContentType { get; set; }
    }
}