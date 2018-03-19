namespace ContractHttp
{
    using System;
    using System.Net.Http;

    public abstract class HttpResponseIntercepterAttribute
        : Attribute
    {
        protected HttpResponseIntercepterAttribute()
        {
        }

        public abstract object ToObject(HttpContent content, Type dataType);

        public abstract object ToObject(string content, Type dataType);
    }
}