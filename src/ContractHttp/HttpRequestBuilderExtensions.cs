namespace ContractHttp
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Http request builder extension methods.
    /// </summary>
    internal static class HttpRequestBuilderExtensions
    {
        /// <summary>
        /// Adds the method headers to a <see cref="HttpRequestBuilder"/>.
        /// </summary>
        /// <param name="requestBuilder">A <see cref="HttpRequestBuilder"/> instance.</param>
        /// <param name="method">The <see cref="MethodInfo"/>.</param>
        /// <param name="names">A list of keys.</param>
        /// <param name="values">A list of values.</param>
        /// <returns>The <see cref="HttpRequestBuilder"/> instance.</returns>
        public static HttpRequestBuilder AddMethodHeaders(
            this HttpRequestBuilder requestBuilder,
            MethodInfo method,
            IEnumerable<string> names,
            IEnumerable<object> values)
        {
            var headerAttrs = method
                .GetCustomAttributes<AddHeaderAttribute>()
                .Union(
                    method.DeclaringType.GetCustomAttributes<AddHeaderAttribute>());

            if (headerAttrs.Any() == true)
            {
                foreach (var attr in headerAttrs)
                {
                    requestBuilder.AddHeader(
                        attr.Header,
                        attr.Value.ExpandString(names, values));
                }
            }

            return requestBuilder;
        }

        /// <summary>
        /// Adds an authorization header.
        /// </summary>
        /// <param name="requestBuilder">A request builder.</param>
        /// <param name="methodInfo">A method info.</param>
        /// <param name="names">The methods parameter names.</param>
        /// <param name="arguments">The methods arguments.</param>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> instance.</param>
        /// <returns>The request builder.</returns>
        public static HttpRequestBuilder AddAuthorizationHeader(
            this HttpRequestBuilder requestBuilder,
            MethodInfo methodInfo,
            IEnumerable<string> names,
            IEnumerable<object> arguments,
            IServiceProvider serviceProvider)
        {
            var authAttr = methodInfo.GetCustomAttribute<AddAuthorizationHeaderAttribute>() ??
                methodInfo.DeclaringType.GetCustomAttribute<AddAuthorizationHeaderAttribute>();

            if (authAttr != null)
            {
                if (authAttr.HeaderValue != null)
                {
                    requestBuilder.AddHeader(
                        "Authorization",
                        authAttr.HeaderValue.ExpandString(names, arguments));
                }
                else
                {
                    var authFactoryType = authAttr.AuthorizationFactoryType ?? typeof(IAuthorizationHeaderFactory);
                    var authFactory = serviceProvider?.GetService(authFactoryType) as IAuthorizationHeaderFactory;
                    if (authFactory != null)
                    {
                        requestBuilder.AddAuthorizationHeader(
                            authFactory.GetAuthorizationHeaderScheme(),
                            authFactory.GetAuthorizationHeaderValue());
                    }
                }
            }

            return requestBuilder;
        }

        /// <summary>
        /// Adds an authorization header.
        /// </summary>
        /// <param name="requestBuilder">A request builder.</param>
        /// <param name="methodInfo">A method info.</param>
        /// <param name="names">The methods parameter names.</param>
        /// <param name="arguments">The methods arguments.</param>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> instance.</param>
        /// <returns>The request builder.</returns>
        public static async Task<HttpRequestBuilder> AddAuthorizationHeaderAsync(
            this HttpRequestBuilder requestBuilder,
            MethodInfo methodInfo,
            IEnumerable<string> names,
            IEnumerable<object> arguments,
            IServiceProvider serviceProvider)
        {
            var authAttr = methodInfo.GetCustomAttribute<AddAuthorizationHeaderAttribute>() ??
                methodInfo.DeclaringType.GetCustomAttribute<AddAuthorizationHeaderAttribute>();

            if (authAttr != null)
            {
                if (authAttr.HeaderValue != null)
                {
                    requestBuilder.AddHeader(
                        "Authorization",
                        authAttr.HeaderValue.ExpandString(names, arguments));
                }
                else
                {
                    var authFactoryType = authAttr.AuthorizationFactoryType ?? typeof(IAuthorizationHeaderFactory);
                    var authFactory = serviceProvider?.GetService(authFactoryType) as IAuthorizationHeaderFactory;
                    if (authFactory != null)
                    {
                        requestBuilder.AddAuthorizationHeader(
                            authFactory.GetAuthorizationHeaderScheme(),
                            await authFactory.GetAuthorizationHeaderValueAsync());
                    }
                }
            }

            return requestBuilder;
        }

        /// <summary>
        /// Checks a parameter for any <see cref="SendAsQueryAttribute"/> attributes.
        /// </summary>
        /// <param name="requestBuilder">A request builder instance.</param>
        /// <param name="attrs">The parameters attributes.</param>
        /// <param name="parm">The parameter.</param>
        /// <param name="argument">The parameters actual value.</param>
        /// <returns>The request builder instance.</returns>
        internal static HttpRequestBuilder CheckParameterForSendAsQuery(
            this HttpRequestBuilder requestBuilder,
            IEnumerable<Attribute> attrs,
            ParameterInfo parm,
            object argument)
        {
            foreach (var query in attrs.OfType<SendAsQueryAttribute>())
            {
                var name = query.Name.IsNullOrEmpty() == false ? query.Name : parm.Name;

                if (parm.ParameterType.IsGenericType == true)
                {
                    if (parm.ParameterType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                    {
                        IDictionary<string, string> queryMap = null;
                        if (typeof(IDictionary<string, string>).IsAssignableFrom(parm.ParameterType) == true)
                        {
                            queryMap = (IDictionary<string, string>)argument;
                        }
                        else if (typeof(IDictionary<object, object>).IsAssignableFrom(parm.ParameterType) == true)
                        {
                            queryMap = ((IDictionary<object, object>)argument)
                                .ToDictionary(key => key.Key.ToString(), value => value.Value.ToString());
                        }
                        else if (typeof(IDictionary<string, object>).IsAssignableFrom(parm.ParameterType) == true)
                        {
                            queryMap = ((IDictionary<string, object>)argument)
                                .ToDictionary(key => key.Key, value => value.Value.ToString());
                        }
                        else if (typeof(IDictionary<object, string>).IsAssignableFrom(parm.ParameterType) == true)
                        {
                            queryMap = ((IDictionary<object, string>)argument)
                                .ToDictionary(key => key.Key.ToString(), value => value.Value);
                        }

                        if (query != null)
                        {
                            foreach (var item in queryMap)
                            {
                                var value = parm.ConvertParameterValue(item.Value, query.Format, query.Base64, query.Encoding);
                                requestBuilder.AddQueryString(item.Key, value);
                            }
                        }
                    }
                    else if (typeof(IEnumerable<string>).IsAssignableFrom(parm.ParameterType) == true)
                    {
                        foreach (var item in (IEnumerable<string>)argument)
                        {
                            var value = parm.ConvertParameterValue(item, query.Format, query.Base64, query.Encoding);
                            requestBuilder.AddQueryString(name, value);
                        }
                    }
                }
                else
                {
                    var value = parm.ConvertParameterValue(argument, query.Format, query.Base64, query.Encoding);
                    requestBuilder.AddQueryString(name, value);
                }
            }

            return requestBuilder;
        }

        /// <summary>
        /// Converts a parameter value.
        /// </summary>
        /// <param name="parm">The parameter info.</param>
        /// <param name="argument">The argument value.</param>
        /// <param name="format">The format to use.</param>
        /// <param name="toBase64">A value indicating whether or not the value should be base64 encoded.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <returns>The value.</returns>
        private static string ConvertParameterValue(this ParameterInfo parm, object argument, string format, bool toBase64, Encoding encoding)
        {
            var value = argument.ToString();
            if (format.IsNullOrEmpty() == false)
            {
                if (parm.ParameterType == typeof(short))
                {
                    value = ((short)argument).ToString(format);
                }
                else if (parm.ParameterType == typeof(int))
                {
                    value = ((int)argument).ToString(format);
                }
                else if (parm.ParameterType == typeof(long))
                {
                    value = ((long)argument).ToString(format);
                }
            }

            if (toBase64 == true)
            {
                value = Convert.ToBase64String(encoding.GetBytes(value));
            }

            return value;
        }

        /// <summary>
        /// Checks a parameter for any <see cref="SendAsHeaderAttribute"/> attributes.
        /// </summary>
        /// <param name="requestBuilder">A request builder instance.</param>
        /// <param name="attrs">The parameters attributes.</param>
        /// <param name="argument">The parameters actual value.</param>
        /// <returns>The request builder instance.</returns>
        internal static HttpRequestBuilder CheckParameterForSendAsHeader(
            this HttpRequestBuilder requestBuilder,
            IEnumerable<Attribute> attrs,
            object argument)
        {
            void AddHeader(string name, string format, object arg)
            {
                if (string.IsNullOrEmpty(format) == false)
                {
                    requestBuilder.AddHeader(
                        name,
                        string.Format(format, arg.ToString()));
                }
                else
                {
                    requestBuilder.AddHeader(
                        name,
                        arg.ToString());
                }
            }

            foreach (var attr in attrs.OfType<SendAsHeaderAttribute>())
            {
                if (typeof(IEnumerable<string>).IsAssignableFrom(argument.GetType()) == true)
                {
                    foreach (var item in (IEnumerable<string>)argument)
                    {
                        AddHeader(attr.Name, attr.Format, item);
                    }
                }
                else
                {
                    AddHeader(attr.Name, attr.Format, argument);
                }
            }

            return requestBuilder;
        }
    }
}