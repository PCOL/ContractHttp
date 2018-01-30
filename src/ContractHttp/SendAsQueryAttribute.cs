namespace ContractHttp
{
    using System;

    [AttributeUsage(AttributeTargets.Parameter)]
    public class SendAsQueryAttribute
        : Attribute
    {
        public SendAsQueryAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
    }
}