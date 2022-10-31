namespace ContractHttp
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A json object serializer which uses the Newtonsoft JSON serializer.
    /// </summary>
    public class JsonObjectSerializer
        : IObjectSerializer
    {
        /// <inheritdoc />
        public string ContentType { get; } = "application/json";

        /// <inheritdoc />
        public object DeserializeObject(string value, Type type)
        {
            return JsonConvert.DeserializeObject(value, type);
        }

        /// <inheritdoc />
        public string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        /// <inheritdoc />
        public object GetObjectFromPath(object obj, Type returnType, string path)
        {
            if (obj is JObject jsonObj)
            {
                if (path.IsNullOrEmpty() == false)
                {
                    var token = jsonObj.SelectToken(path);
                    return token?.ToObject(returnType);
                }

                return jsonObj.ToObject(returnType);
            }
            else if (obj.GetType() == returnType &&
                path.IsNullOrEmpty() == true)
            {
                return obj;
            }

            return null;
        }
    }
}