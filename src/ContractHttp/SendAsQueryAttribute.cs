namespace ContractHttp
{
    using System;
    using System.Text;

    [AttributeUsage(AttributeTargets.Parameter)]
    public class SendAsQueryAttribute
        : Attribute
    {
        public SendAsQueryAttribute(string name)
        {
            this.Name = name;
            this.Encoding = Encoding.UTF8;
        }

        public string Name { get; }

        public string Format { get; set; }

        public Encoding Encoding { get; set; }
        
        public bool Base64 { get; set; }
    }
}