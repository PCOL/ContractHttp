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

        /// <summary>
        /// Gets an object from another object using a path.
        /// </summary>
        /// <param name="obj">The object to extract from.</param>
        /// <param name="returnType">The expected type.</param>
        /// <param name="path">The path to object.</param>
        /// <returns>The object at the path; otherwise null.</returns>
        object GetObjectFromPath(object obj, Type returnType, string path);
    }
}