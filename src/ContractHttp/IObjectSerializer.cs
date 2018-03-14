namespace ContractHttp
{
    using System;

    /// <summary>
    /// Defines an object serializer.
    /// </summary>
    public interface IObjectSerializer
    {
        /// <summary>
        /// Gets the content type.
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A string containing the serialized object data.</returns>
        string SerializeObject(object obj);

        /// <summary>
        /// Deserializes an object.
        /// </summary>
        /// <param name="data">A data to deserialize.</param>
        /// <param name="type">The type of object to create.</param>
        /// <returns>An instance of the object.</returns>
        object DeserializeObject(string data, Type type);
    }
}