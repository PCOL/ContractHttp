namespace ContractHttp
{
    using System;

    public class SendAsFormUrlAttribute
        : Attribute
    {
        public SendAsFormUrlAttribute()
        {
        }

        public string Name { get; set;}
    }
}