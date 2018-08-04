namespace ContractHttp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

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
                var value = argument.ToString();
                if (query.Format.IsNullOrEmpty() == false)
                {
                    if (parm.ParameterType == typeof(short))
                    {
                        value = ((short)argument).ToString(query.Format);
                    }
                    else if (parm.ParameterType == typeof(int))
                    {
                        value = ((int)argument).ToString(query.Format);
                    }
                    else if (parm.ParameterType == typeof(long))
                    {
                        value = ((long)argument).ToString(query.Format);
                    }
                }

                if (query.Base64 == true)
                {
                    value = Convert.ToBase64String(query.Encoding.GetBytes(value));
                }

                requestBuilder.AddQueryString(name, value);
            }

            return requestBuilder;
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
            object argument
        )
        {
            foreach (var attr in attrs.OfType<SendAsHeaderAttribute>())
            {
                if (string.IsNullOrEmpty(attr.Format) == false)
                {
                    requestBuilder.AddHeader(
                        attr.Name,
                        string.Format(attr.Format, argument.ToString()));
                }
                else
                {
                    requestBuilder.AddHeader(
                        attr.Name,
                        argument.ToString());
                }
            }

            return requestBuilder;
        }
    }
}