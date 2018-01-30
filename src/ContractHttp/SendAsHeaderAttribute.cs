namespace ContractHttp
{
    using System;

    [AttributeUsage(AttributeTargets.Parameter)]
    public class SendAsHeaderAttribute
        : Attribute
    {
        public SendAsHeaderAttribute(string headerName)
        {
            this.Name = headerName;
        }

        public string Name { get; }
    }
}