namespace ContractHttp
{
    using System;
    using System.Net.Http;

    public abstract class HttpResponseIntercepterAttribute
        : Attribute
    {
        protected HttpResponseIntercepterAttribute(string contentType = null)
        {
            this.ContentType = contentType;
        }

        public string ContentType { get; }

        public abstract object ToObject(HttpResponseMessage content, Type dataType, IObjectSerializer serializer);
    }
}