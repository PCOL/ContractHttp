namespace ContractHttp
{
    using System;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    public class AddHeaderAttribute
        : Attribute
    {
        public AddHeaderAttribute(string header, string value)
        {
            this.Header = header;
            this.Value = value;
        }

        public string Header { get; }

        public string Value { get; }
    }
}