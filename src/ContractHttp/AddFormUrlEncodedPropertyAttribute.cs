namespace ContractHttp
{
    using System;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class AddFormUrlEncodedPropertyAttribute
        : Attribute
    {
        public AddFormUrlEncodedPropertyAttribute(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }

        public string Key { get; }

        public string Value { get; }
    }
}