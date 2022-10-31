namespace ContractHttp
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// A json object serializer.
    /// </summary>
    public class TextJsonObjectSerializer
        : IObjectSerializer
    {
        private static readonly JsonSerializerOptions SerializerOptions;

        static TextJsonObjectSerializer()
        {
            SerializerOptions = new JsonSerializerOptions()
            {
                 PropertyNameCaseInsensitive = true,
                 DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                 PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        /// <inheritdoc />
        public string ContentType { get; } = "application/json";

        /// <inheritdoc />
        public object DeserializeObject(string value, Type type)
        {
            return JsonSerializer.Deserialize(value, type, SerializerOptions);
        }

        /// <inheritdoc />
        public string SerializeObject(object value)
        {
            return JsonSerializer.Serialize(value, SerializerOptions);
        }

        /// <inheritdoc />
        public object GetObjectFromPath(object obj, Type returnType, string path)
        {
            if (obj is JsonElement jsonElement)
            {
                object returnValue = null;
                var value = this.GetElement(jsonElement, path);
                if (value != null)
                {
                    returnValue = this.GetElementValue(value.Value, returnType);
                }

                return returnValue;
            }
            else if (obj.GetType() == returnType &&
                path.IsNullOrEmpty() == true)
            {
                return obj;
            }

            return null;
        }

        /// <summary>
        /// Gets a element at a given path.
        /// </summary>
        /// <param name="jsonElement">The base JSON element.</param>
        /// <param name="path">The path to read from.</param>
        /// <returns>The <see cref="JsonElement"/> if found; otherwise null.</returns>
        private JsonElement? GetElement(JsonElement jsonElement, string path)
        {
            var segment = new Span<char>(path.ToCharArray());
            var left = new Span<char>(path.ToCharArray());
            while (left.Length != 0)
            {
                var pos = left.IndexOf('.');
                if (pos != -1)
                {
                    segment = left.Slice(0, pos);
                    left = left.Slice(pos + 1);
                }
                else
                {
                    segment = left;
                    left = Span<char>.Empty;
                }

                var arrayStart = segment.IndexOf('[');
                if (arrayStart != -1)
                {
                    var arrayEnd = segment.IndexOf(']');
                    var arrayPart = segment.Slice(arrayStart + 1, arrayEnd - arrayStart);
                    segment = segment.Slice(0, arrayStart);
                }

                var found = false;
                foreach (var item in jsonElement.EnumerateObject())
                {
                    if (item.NameEquals(segment) == true)
                    {
                        jsonElement = item.Value;
                        found = true;
                    }
                }

                if (found == false)
                {
                    return null;
                }
            }

            return jsonElement;
        }

        /// <summary>
        /// The value of a <see cref="JsonElement"/> as a given type.
        /// </summary>
        /// <param name="jsonElement">The <see cref="JsonElement"/>.</param>
        /// <param name="returnType">The required type.</param>
        /// <returns>An object of the required type; otherwise null.</returns>
        private object GetElementValue(JsonElement jsonElement, Type returnType)
        {
            if (jsonElement.ValueKind == JsonValueKind.String &&
                returnType == typeof(string))
            {
                return jsonElement.GetString();
            }
            else if (jsonElement.ValueKind == JsonValueKind.Number)
            {
                if (returnType == typeof(short))
                {
                    return jsonElement.GetInt16();
                }
                else if (returnType == typeof(int))
                {
                    return jsonElement.GetInt32();
                }
                else if (returnType == typeof(long))
                {
                    return jsonElement.GetInt64();
                }
                else if (returnType == typeof(float))
                {
                    return jsonElement.GetSingle();
                }
                else if (returnType == typeof(double))
                {
                    return jsonElement.GetDouble();
                }
            }
            else if (jsonElement.ValueKind == JsonValueKind.True &&
                returnType == typeof(bool))
            {
                return true;
            }
            else if (jsonElement.ValueKind == JsonValueKind.False &&
                returnType == typeof(bool))
            {
                return false;
            }
            else if (jsonElement.ValueKind == JsonValueKind.Object &&
                returnType.IsClass == true)
            {
                var properties = returnType.GetProperties();
                var returnObj = Activator.CreateInstance(returnType);
                foreach (var kvp in jsonElement.EnumerateObject())
                {
                    var property = properties.FirstOrDefault(p => p.Name.Equals(kvp.Name, StringComparison.OrdinalIgnoreCase));
                    if (property != null)
                    {
                        var value = this.GetElementValue(kvp.Value, property.PropertyType);
                        property.SetValue(returnObj, value);
                    }
                }

                return returnObj;
            }
            else if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                Debug.WriteLine("Json Array");

                Type arrayType;
                if (returnType.IsArray == true)
                {
                    arrayType = returnType.GetElementType();
                    Debug.WriteLine("Array Type: {0}", arrayType);
                }
                else if (returnType.IsGenericType == true)
                {
                    arrayType = returnType.GetGenericArguments()[0];
                    Debug.WriteLine("Generic Type: {0}", arrayType);
                }
                else
                {
                    throw new InvalidOperationException("Invalid return type");
                }

                var len = jsonElement.GetArrayLength();
                var array = Array.CreateInstance(arrayType, len);
                var index = 0;
                foreach (var item in jsonElement.EnumerateArray())
                {
                    array.SetValue(this.GetElementValue(item, arrayType), index++);
                }

                if (returnType.IsArray == true)
                {
                    return array;
                }

                if (returnType.IsGenericType == true)
                {
                    if (returnType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    {
                        var method = typeof(Enumerable).GetMethod("AsEnumerable", BindingFlags.Public | BindingFlags.Static);
                        Console.WriteLine("method: {0}", method != null);
                        var genericMethod = method.MakeGenericMethod(arrayType);
                        Console.WriteLine("Generic method: {0}", genericMethod != null);

                        return genericMethod.Invoke(null, new object[] { array });
                    }
                    else if (returnType.GetGenericTypeDefinition() == typeof(List<>) ||
                        returnType.GetGenericTypeDefinition() == typeof(IList<>))
                    {
                        var method = typeof(Enumerable).GetMethod("ToList", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(arrayType);
                        return method.Invoke(null, new object[] { array });
                    }
                }
            }

            return null;
        }
    }
}