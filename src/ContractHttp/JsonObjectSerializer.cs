namespace ContractHttp
{
    using System;
    using Newtonsoft.Json;

    public class JsonObjectSerializer
        : IObjectSerializer
    {
        public string ContentType { get; } = "application/json";

        public object DeserializeObject(string value, Type type)
        {
            return JsonConvert.DeserializeObject(value, type);
        }

        public string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value);
        }
    }
}