namespace ContractHttp
{
    using System;

    public class SendAsFormUrlAttribute
        : Attribute
    {
        public SendAsFormUrlAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
    }
}