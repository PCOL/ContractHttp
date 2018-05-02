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
        /// Adds the method headers to a <see cref="RequestBuilder"/>.
        /// </summary>
        /// <param name="requestBuilder">THe <see cref=""/>RequestBuilder.</param>
        /// <param name="method">The <see cref="MethodInfo"/>.</param>
        /// <param name="names">A list of keys.</param>
        /// <param name="values">A list of values.</param>
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
        /// <returns></returns>
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
    }
}