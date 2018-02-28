using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace ContractHttp
{
    internal class HttpRequestBuilder
    {
        private HttpMethod httpMethod = HttpMethod.Get;

        private string uri;

        private HttpContent content;

        private Dictionary<string, string> queryStrings;

        private Dictionary<string, string> headers;

        private List<KeyValuePair<string, string>> formUrlProperties;

        /// <summary>
        /// Initialises a new instance of the <see cref="HttpRequestBuilder"/> class.
        /// </summary>
        public HttpRequestBuilder()
        {
        }

        /// <summary>
        /// Gets a value indicating whether or not the content has been set.
        /// </summary>
        /// <returns></returns>
        public bool IsContentSet
        {
            get
            {
                return this.content != null;
            }
        }

        /// <summary>
        /// Sets the http method.
        /// </summary>
        /// <param name="httpMethod">The http method to set.</param>
        /// <returns>This instance of the <see cref="HttpRequestBuilder"/> class.</returns>
        public HttpRequestBuilder SetMethod(HttpMethod httpMethod)
        {
            this.httpMethod = httpMethod;
            return this;
        }

        public HttpRequestBuilder SetUri(string uri)
        {
            this.uri = uri;
            return this;
        }

        public HttpRequestBuilder SetContent(HttpContent content)
        {
            this.content = content;
            return this;
        }

        public HttpRequestBuilder AddAuthorizationHeader(string scheme, string value)
        {
            return this.AddHeader("Authorization", $"{scheme} {value}");
        }

        public HttpRequestBuilder AddHeader(string key, string value)
        {
            this.headers = this.headers ?? new Dictionary<string, string>();
            this.headers.Add(key, value);
            return this;
        }

        public HttpRequestBuilder AddQueryString(string key, string value)
        {
            this.queryStrings = this.queryStrings ?? new Dictionary<string, string>();
            this.queryStrings.Add(key, value);
            return this;
        }

        public HttpRequestBuilder AddFormUrlProperty(string key, string value)
        {
            this.formUrlProperties = this.formUrlProperties ?? new List<KeyValuePair<string, string>>();
            this.formUrlProperties.Add(new KeyValuePair<string, string>(key, value));
            return this;
        }

        public HttpRequestBuilder AddFormUrlProperties(IEnumerable<KeyValuePair<string, string>> properties)
        {
            this.formUrlProperties = this.formUrlProperties ?? new List<KeyValuePair<string, string>>();
            this.formUrlProperties.AddRange(properties);
            return this;
        }

        /// <summary>
        /// Builds the <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <returns>A new instance of the <see cref="HttpRequestMessage"/> class.</returns>
        public HttpRequestMessage Build()
        {
            var requestUri = this.uri;
            if (this.queryStrings != null)
            {
                var query = string.Join("&", this.queryStrings.Select(q => $"{q.Key}={q.Value}"));
                requestUri += $"?{query}";
            }

            var request = new HttpRequestMessage(this.httpMethod, requestUri);

            if (this.headers != null)
            {
                foreach (var header in this.headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            if (this.content == null &&
                this.formUrlProperties != null)
            {
                this.content = new FormUrlEncodedContent(this.formUrlProperties);
            }

            if (this.content != null)
            {
                request.Content = this.content;
            }

            return request;
        }
    }
}