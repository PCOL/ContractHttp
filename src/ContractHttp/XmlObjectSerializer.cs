namespace ContractHttp
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// Xml implementation of the <see cref="IObjectSerializer"/> interface.
    /// </summary>
    public class XmlObjectSerializer
        : IObjectSerializer
    {
        /// <inheritdoc />
        public string ContentType => "application/xml";

        /// <inheritdoc />
        public object DeserializeObject(string data, Type type)
        {
            using (var reader = new StringReader(data))
            {
                return new XmlSerializer(type).Deserialize(reader);
            }
        }

        /// <inheritdoc />
        public string SerializeObject(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            using (var stream = new MemoryStream())
            {
                new XmlSerializer(obj.GetType()).Serialize(stream, obj);

                return UTF8Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        /// <inheritdoc />
        public object GetObjectFromPath(object obj, Type returnType, string path)
        {
            if (obj.GetType() == returnType &&
                path.IsNullOrEmpty() == true)
            {
                return obj;
            }

            return null;
        }
    }
}