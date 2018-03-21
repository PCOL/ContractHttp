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
        /// <param name="response">The response.</param>
        /// <param name="dataType">The object type.</param>
        /// <returns></returns>
        public override object ToObject(HttpResponseMessage response, Type dataType)
        {
            var content = response.Content.ReadAsStringAsync().Result;
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