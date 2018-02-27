namespace ContractHttp
{
    using System;
    using Newtonsoft.Json.Linq;

    [AttributeUsage(AttributeTargets.ReturnValue)]
    public class FromJsonAttribute
        : Attribute
    {
        public FromJsonAttribute(string jsonPath)
        {
            this.JsonPath = jsonPath;
        }

        public string JsonPath { get; }

        /// <summary>
        /// Converts a json string to an object.
        /// </summary>
        /// <param name="content">The json content.</param>
        /// <param name="dataType">The object type.</param>
        /// <returns></returns>
        internal object JsonToObject(string content, Type dataType)
        {
            var jobj = JObject.Parse(content);
            if (jobj != null)
            {
                var token = jobj.SelectToken(this.JsonPath);
                return token.ToObject(dataType);
            }

            return null;
        }
    }
}