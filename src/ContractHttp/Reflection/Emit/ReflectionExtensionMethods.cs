namespace ContractHttp.Reflection.Emit
{
    using System;
    using System.Reflection;
    using FluentIL;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Reflection extension methods.
    /// </summary>
    public static class ReflectionExtensionMethods
    {
        /// <summary>
        /// Gets an attribute from the method or its declaring type.
        /// </summary>
        /// <param name="methodInfo">The method.</param>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <returns>The attribute if found; otherwise null.</returns>
        public static T GetMethodOrTypeAttribute<T>(this MethodInfo methodInfo)
            where T : Attribute
        {
            var attr = methodInfo.GetCustomAttribute<T>();
            if (attr != null)
            {
                return attr;
            }

            return methodInfo.DeclaringType.GetCustomAttribute<T>();
        }

        /// <summary>
        /// Processes type builder attributes.
        /// </summary>
        /// <param name="typeBuilder">A type builder.</param>
        /// <param name="type">The type to process the attributes for.</param>
        /// <returns>The type builder instance.</returns>
        public static ITypeBuilder ProcessAttributes(
            this ITypeBuilder typeBuilder,
            Type type)
        {
            foreach (var attr in type.GetCustomAttributes())
            {
/*
                if (attr is SwaggerRequestHeaderParameterAttribute)
                {
                    typeBuilder.SetCustomAttribute(
                        AttributeUtility.BuildAttribute<string, SwaggerRequestHeaderParameterAttribute>(
                            ((SwaggerRequestHeaderParameterAttribute)attr).Header,
                            () => AttributeUtility.GetAttributePropertyValues<SwaggerRequestHeaderParameterAttribute>((SwaggerRequestHeaderParameterAttribute)attr, null)));
                }
                else
*/
                if (attr is ObsoleteAttribute)
                {
                    typeBuilder.SetCustomAttribute(
                        AttributeUtility.BuildAttribute<ObsoleteAttribute>(null));
                }
            }

            return typeBuilder;
        }

        /// <summary>
        /// Processes method attributes.
        /// </summary>
        /// <param name="methodBuilder">A method builder.</param>
        /// <param name="methodInfo">The method.</param>
        /// <returns>The method builder.</returns>
        public static IMethodBuilder ProcessAttributes(
            this IMethodBuilder methodBuilder,
            MethodInfo methodInfo)
        {
            foreach (var attr in methodInfo.GetCustomAttributes())
            {
                if (attr is ProducesAttribute)
                {
                    methodBuilder.SetCustomAttribute(
                        AttributeUtility.BuildAttribute<Type, ProducesAttribute>(
                            ((ProducesAttribute)attr).Type,
                            () => AttributeUtility.GetAttributePropertyValues<ProducesAttribute>((ProducesAttribute)attr, new[] { "Type", "ContentTypes" })));
                }
                else if (attr is ProducesResponseTypeAttribute)
                {
                    methodBuilder.SetCustomAttribute(
                        AttributeUtility.BuildAttribute<int, ProducesResponseTypeAttribute>(
                            ((ProducesResponseTypeAttribute)attr).StatusCode,
                            () => AttributeUtility.GetAttributePropertyValues<ProducesResponseTypeAttribute>((ProducesResponseTypeAttribute)attr, new[] { "type" })));
                }

/*
                else if (attr is SwaggerParameterAttribute)
                {
                    methodBuilder.SetCustomAttribute(
                        AttributeUtility.BuildAttribute<string, SwaggerParameterAttribute>(
                            ((SwaggerParameterAttribute)attr).ParameterName,
                            () => AttributeUtility.GetAttributePropertyValues<SwaggerParameterAttribute>((SwaggerParameterAttribute)attr, null)));
                }
                else if (attr is SwaggerRequestHeaderParameterAttribute)
                {
                    methodBuilder.SetCustomAttribute(
                        AttributeUtility.BuildAttribute<string, SwaggerRequestHeaderParameterAttribute>(
                            ((SwaggerRequestHeaderParameterAttribute)attr).Header,
                            () => AttributeUtility.GetAttributePropertyValues<SwaggerRequestHeaderParameterAttribute>((SwaggerRequestHeaderParameterAttribute)attr, null)));
                }
*/
                else if (attr is ObsoleteAttribute)
                {
                    methodBuilder.SetCustomAttribute(
                        AttributeUtility.BuildAttribute<ObsoleteAttribute>(null));
                }
            }

            return methodBuilder;
        }
    }
}