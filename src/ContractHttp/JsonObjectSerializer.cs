namespace ContractHttp
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// A json object serializer.
    /// </summary>
    public class JsonObjectSerializer
        : IObjectSerializer
    {
        /// <summary>
        /// Gets the serializers content type.
        /// </summary>
        public string ContentType { get; } = "application/json";

        /// <summary>
        /// Deserializes an object.
        /// </summary>
        /// <param name="value">The value to deserialize.</param>
        /// <param name="type">The type of object to deserialize.</param>
        /// <returns>An instance of the object.</returns>
        public object DeserializeObject(string value, Type type)
        {
            return JsonConvert.DeserializeObject(value, type);
        }

        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <returns>The serilised object.</returns>
        public string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value);
        }
    }
}