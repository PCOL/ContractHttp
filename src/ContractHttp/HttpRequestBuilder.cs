namespace ContractHttp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;

    /// <summary>
    /// Builds a <see cref="HttpRequestMessage"/>.
    /// </summary>
    internal class HttpRequestBuilder
        : IHttpRequestBuilder
    {
        /// <summary>
        /// The proxy options.
        /// </summary>
        private readonly HttpClientProxyOptions options;

        /// <summary>
        /// The http method.
        /// </summary>
        private HttpMethod httpMethod = HttpMethod.Get;

        /// <summary>
        /// The url.
        /// </summary>
        private string uri;

        /// <summary>
        /// The http content.
        /// </summary>
        private HttpContent content;

        /// <summary>
        /// A dictionary of query strings.
        /// </summary>
        private Dictionary<string, object> queryStrings;

        /// <summary>
        /// A dictionary of request headers.
        /// </summary>
        private Dictionary<string, string> headers;

        /// <summary>
        /// The content disposition header value.
        /// </summary>
        private ContentDispositionHeaderValue contentDispositionHeader;

        /// <summary>
        /// A list of form Url properties.
        /// </summary>
        private List<KeyValuePair<string, string>> formUrlProperties;

        /// <summary>
        /// A value indicating whether or not the request is a mulitpart content request.
        /// </summary>
        private bool isMultipartContent;

        /// <summary>
        /// The http version.
        /// </summary>
        private Version httpVersion = new Version(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestBuilder"/> class.
        /// </summary>
        /// <param name="options">The proxy options.</param>
        public HttpRequestBuilder(HttpClientProxyOptions options)
        {
            this.options = options;
        }

        /// <summary>
        /// Gets a value indicating whether or not the content has been set.
        /// </summary>
        public bool IsContentSet
        {
            get
            {
                return this.content != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not the content is a specific type.
        /// </summary>
        /// <typeparam name="T">The content type.</typeparam>
        /// <returns>A value indicating whether or not the content is of the given type.</returns>
        public bool IsContent<T>()
        {
            return this.content is T;
        }

        /// <summary>
        /// Sets a value indicating whether or not this is a multipart content request.
        /// </summary>
        /// <param name="value">A value indicating whether or not this is a multipart content request.</param>
        /// <returns>This instance of the <see cref="HttpRequestBuilder"/> class.</returns>
        public HttpRequestBuilder SetMultipartContent(bool value)
        {
            this.isMultipartContent = value;
            return this;
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

        /// <summary>
        /// Sets the http version.
        /// </summary>
        /// <param name="httpVersion">The http version to set.</param>
        /// <returns>This instance of the <see cref="HttpRequestBuilder"/> class.</returns>
        public HttpRequestBuilder SetHttpVersion(Version httpVersion)
        {
            this.httpVersion = httpVersion;
            return this;
        }

        /// <summary>
        /// Sets the requests Uri.
        /// </summary>
        /// <param name="uri">The Uri.</param>
        /// <returns>The <see cref="HttpRequestBuilder"/> instance.</returns>
        public HttpRequestBuilder SetUri(string uri)
        {
            this.uri = uri;
            return this;
        }

        /// <summary>
        /// Sets the requests content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>The <see cref="HttpRequestBuilder"/> instance.</returns>
        public HttpRequestBuilder SetContent(HttpContent content)
        {
            this.content = content;
            return this;
        }

        /// <summary>
        /// Adds an authorization header.
        /// </summary>
        /// <param name="scheme">The authorization scheme.</param>
        /// <param name="value">The authorization value.</param>
        /// <returns>The <see cref="HttpRequestBuilder"/> instance.</returns>
        public HttpRequestBuilder AddAuthorizationHeader(string scheme, string value)
        {
            return this.AddHeader("Authorization", $"{scheme} {value}");
        }

        /// <summary>
        /// Adds a header.
        /// </summary>
        /// <param name="key">The header key.</param>
        /// <param name="value">The header value.</param>
        /// <returns>The <see cref="HttpRequestBuilder"/> instance.</returns>
        public HttpRequestBuilder AddHeader(string key, string value)
        {
            if (value.IndexOf(',') != -1)
            {
                value = $"\"{value}\"";
            }

            this.headers = this.headers ?? new Dictionary<string, string>();
            if (this.headers.TryGetValue(key, out string currentValue) == true)
            {
                this.headers[key] = currentValue += $", {value}";
            }
            else
            {
                this.headers.Add(key, value);
            }

            return this;
        }

        /// <summary>
        /// Sets a <see cref="ContentDispositionHeaderValue"/> header value.
        /// </summary>
        /// <param name="action">An action to set the content disposition header.</param>
        /// <returns>The <see cref="HttpRequestBuilder"/> instance.</returns>
        public HttpRequestBuilder SetContentDispositionHeader(Action<ContentDispositionHeaderValue> action)
        {
            this.contentDispositionHeader = this.contentDispositionHeader ?? new ContentDispositionHeaderValue("form-data");
            action(this.contentDispositionHeader);
            return this;
        }

        /// <summary>
        /// Adds a query string.
        /// </summary>
        /// <param name="key">The query key.</param>
        /// <param name="value">The query value.</param>
        /// <returns>The <see cref="HttpRequestBuilder"/> instance.</returns>
        public HttpRequestBuilder AddQueryString(string key, string value)
        {
            this.queryStrings = this.queryStrings ?? new Dictionary<string, object>();

            if (this.queryStrings.TryGetValue(key, out object existingValue) == true)
            {
                if (existingValue is string existingString)
                {
                    var list = new List<string>();
                    list.Add(existingString);
                    list.Add(value);

                    this.queryStrings[key] = list;
                }
                else if (existingValue is List<string> existingList)
                {
                    existingList.Add(value);
                }
            }
            else
            {
                this.queryStrings.Add(key, value);
            }

            return this;
        }

        /// <summary>
        /// Adds a form url property.
        /// </summary>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        /// <returns>The <see cref="HttpRequestBuilder"/> instance.</returns>
        public HttpRequestBuilder AddFormUrlProperty(string key, string value)
        {
            this.formUrlProperties = this.formUrlProperties ?? new List<KeyValuePair<string, string>>();
            this.formUrlProperties.Add(new KeyValuePair<string, string>(key, value));
            return this;
        }

        /// <summary>
        /// Adds a list of form url properties.
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <returns>The <see cref="HttpRequestBuilder"/> instance.</returns>
        public HttpRequestBuilder AddFormUrlProperties(IEnumerable<KeyValuePair<string, string>> properties)
        {
            if (properties != null)
            {
                this.formUrlProperties = this.formUrlProperties ?? new List<KeyValuePair<string, string>>();
                this.formUrlProperties.AddRange(properties);
            }

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
                var first = true;
                var query = string.Empty;
                foreach (var queryString in this.queryStrings)
                {
                    query += first == true ? "?" : "&";
                    if (queryString.Value is string queryStringValue)
                    {
                        query += $"{Uri.EscapeDataString(queryString.Key)}={Uri.EscapeDataString(queryStringValue)}";
                    }
                    else if (queryString.Value is List<string> queryStringList)
                    {
                        query += string.Join("&", queryStringList.Select(q => $"{Uri.EscapeDataString(queryString.Key)}={Uri.EscapeDataString(q)}"));
                    }

                    first = false;
                }

                requestUri += query;
            }

            var request = new HttpRequestMessage(this.httpMethod, requestUri)
            {
                Version = this.httpVersion
            };

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

            ////Console.WriteLine("Uri: {0}", requestUri);
            ////Console.WriteLine("Mulipart {0}, Disposition: {1}", this.isMultipartContent, this.contentDispositionHeader);
            if (this.isMultipartContent == true &&
                this.contentDispositionHeader != null)
            {
                Console.WriteLine("Building Mulipart");
                if (this.content is StreamContent streamContent)
                {
                    streamContent.Headers.ContentDisposition = this.contentDispositionHeader;
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    var multipartContent = new MultipartFormDataContent();
                    multipartContent.Add(streamContent);

                    this.content = multipartContent;
                }
            }

            if (this.options.DebugOutputEnabled == true)
            {
                Console.WriteLine("Uri: {0}", requestUri);
            }

            if (this.content != null)
            {
                request.Content = this.content;

                if (this.options.DebugOutputEnabled == true)
                {
                    Console.WriteLine("Content: {0}", this.content);
                }
            }

            return request;
        }
    }
}