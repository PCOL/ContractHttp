namespace ContractHttp
{
    using System;
    using System.Net.Http;
    using Newtonsoft.Json.Linq;

    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter)]
    public class FromJsonAttribute
        : HttpResponseIntercepterAttribute
    {
        public FromJsonAttribute(string jsonPath)
        {
            this.JsonPath = jsonPath;
        }

        public string JsonPath { get; }

        /// <summary>
        /// Converts a json string to an object.
        /// </summary>
        /// <param name="httpContent">The response content.</param>
        /// <param name="dataType">The object type.</param>
        /// <returns></returns>
        public override object ToObject(HttpContent httpContent, Type dataType)
        {
            var content = httpContent.ReadAsStringAsync().Result;
            return this.ToObject(content, dataType);
        }

        /// <summary>
        /// Converts a json string to an object.
        /// </summary>
        /// <param name="httpContent">The response content.</param>
        /// <param name="dataType">The object type.</param>
        /// <returns></returns>
        public override object ToObject(string content, Type dataType)
        {
            if (content.IsNullOrEmpty() == false)
            {
                var jobj = JObject.Parse(content);
                if (jobj != null)
                {
                    var token = jobj.SelectToken(this.JsonPath);
                    return token?.ToObject(dataType);
                }
            }

            return null;
        }
    }
}