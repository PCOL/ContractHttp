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

        public abstract object ToObject(HttpResponseMessage content, Type dataType);
    }
}