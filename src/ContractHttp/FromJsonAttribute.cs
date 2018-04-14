namespace ContractHttp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Allows an out parameter or return value to be set from a JSON value.
    /// </summary>
    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter)]
    public class FromJsonAttribute
        : FromResponseAttribute
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="FromJsonAttribute"/> class.
        /// </summary>
        /// <param name="jsonPath">The path to the json value.</param>
        public FromJsonAttribute(string jsonPath)
        {
            this.ContentType = "application/json";
            this.JsonPath = jsonPath;
        }

        /// <summary>
        /// Gets or sets the contentType
        /// </summary>
        public new string ContentType
        {
            get => base.ContentType;
            set {}
        }

        /// <summary>
        /// Gets the json path.
        /// </summary>
        public string JsonPath { get; }

        /// <summary>
        /// Gets or sets thge return type.
        /// </summary>
        public Type ReturnType { get; set; }

        /// <summary>
        /// Converts a json string to an object.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="dataType">The object type.</param>
        /// <param name="serializer">The object serializer.</param>
        /// <returns>The object instance.</returns>
        public override object ToObject(
            HttpResponseMessage response,
            Type dataType,
            IObjectSerializer serializer)
        {
            var content = response.Content.ReadAsStringAsync().Result;
            if (content.IsNullOrEmpty() == true)
            {
                return null;
            }

            var jobj = JObject.Parse(content);
            if (jobj == null)
            {
                return null;
            }

            Type objectType = this.ReturnType ?? dataType;
            var properties = objectType.GetProperties();

            object returnObj = null;
            if (objectType.IsGenericType == false)
            {
                returnObj = this.GetObject(jobj, objectType, this.JsonPath);
            }
            else
            {
                if (dataType.IsAssignableFrom(objectType) == false)
                {
                    throw new InvalidCastException($"Cannot cast {objectType.Name} to {dataType.Name}");
                }

                var genArgs = objectType.GetGenericArguments();
                if (genArgs?.Length > 1)
                {
                    throw new NotSupportedException("Only one generic argument is supported");
                }

                var resultType = genArgs.First();

                returnObj = Activator.CreateInstance(this.ReturnType);
                properties.SetProperty(
                    returnObj,
                    resultType,
                    () => this.GetObject(jobj, resultType, this.JsonPath));
            }

            properties.SetProperty<HttpResponseMessage>(
                returnObj,
                () => response);

            return returnObj;
        }

        /// <summary>
        /// Sets the first property of a given type that is publicly setable.
        /// </summary>
        /// <param name="properties">A list of properties.</param>
        /// <param name="obj">The object that the property belongs to.</param>
        /// <param name="propertyType">The type of property to look for.</param>
        /// <param name="func">A function to provide the property value.</param>
        private void SetProperty(IEnumerable<PropertyInfo> properties, object obj, Type propertyType, Func<object> func)
        {
            if (obj != null)
            {
                var resultProperty = properties?.FirstOrDefault(
                    p => p.PropertyType == propertyType &&
                    p.SetMethod != null &&
                    p.SetMethod.IsPublic == true);

                if (resultProperty != null)
                {
                    resultProperty.SetValue(obj, func());
                }
            }
        }

        /// <summary>
        /// Gets an object from a <see cref="JObject"/> instance.
        /// </summary>
        /// <param name="obj">The <see cref="JObject"/> instance.</param>
        /// <param name="objectType">The type of object to get.</param>
        /// <returns></returns>
        private object GetObject(JObject obj, Type objectType, string jsonPath)
        {
            if (jsonPath.IsNullOrEmpty() == false)
            {
                var token = obj.SelectToken(jsonPath);
                return token?.ToObject(objectType);
            }

            return obj.ToObject(objectType);
        }
    }
}