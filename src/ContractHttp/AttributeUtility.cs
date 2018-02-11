namespace ContractHttp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// Attribute utility methods
    /// </summary>
    public static class AttributeUtility
    {
        /// <summary>
        /// Transfers attributes from a defined method to a new one.
        /// </summary>
        /// <typeparam name="TAttr">The attribute type.</typeparam>
        /// <param name="methodInfo">The defined method.</param>
        /// <param name="methodBuilder">The new method builder.</param>
        public static void TransferAttribute<TAttr>(
            this MethodInfo methodInfo,
            MethodBuilder methodBuilder)
            where TAttr : Attribute
        {
            var attr = methodInfo.GetCustomAttribute<TAttr>(false);
            if (attr != null)
            {
                methodBuilder.SetCustomAttribute(
                    BuildAttribute<TAttr>(
                        () =>
                        {
                            return GetAttributePropertyValues<TAttr>(attr, new string[0]);
                        }));
            }
        }

        /// <summary>
        /// Transfers attributes from a defined method to a new one.
        /// </summary>
        /// <typeparam name="T">The parameter type.</typeparam>
        /// <typeparam name="TAttr">The attribute type.</typeparam>
        /// <param name="methodInfo">The defined method.</param>
        /// <param name="methodBuilder">The new method builder.</param>
        public static void TransferAttributes<T, TAttr>(
            this MethodInfo methodInfo,
            MethodBuilder methodBuilder)
            where TAttr : Attribute
        {
            var attrs = methodInfo.GetCustomAttributes<TAttr>(false);
            if (attrs.IsNullOrEmpty() == true)
            {
                return;
            }

            methodInfo.TransferAttributesInternal<TAttr>(methodBuilder, attrs, new Type[] { typeof(T) });
        }

        /// <summary>
        /// Transfers attributes from a defined method to a new one.
        /// </summary>
        /// <typeparam name="T1">The first parameter type.</typeparam>
        /// <typeparam name="T2">The second parameter type.</typeparam>
        /// <typeparam name="TAttr">The attribute type.</typeparam>
        /// <param name="methodInfo">The defined method.</param>
        /// <param name="methodBuilder">The new method builder.</param>
        public static void TransferAttribute<T1, T2, TAttr>(
            this MethodInfo methodInfo,
            MethodBuilder methodBuilder)
            where TAttr : Attribute
        {
            var attrs = methodInfo.GetCustomAttributes<TAttr>(false);
            if (attrs.IsNullOrEmpty() == true)
            {
                return;
            }

            methodInfo.TransferAttributesInternal<TAttr>(methodBuilder, attrs, new Type[] { typeof(T1), typeof(T2) });
        }

        /// <summary>
        /// Transfers attributes from a defined method to a new one.
        /// </summary>
        /// <typeparam name="T1">The first parameter type.</typeparam>
        /// <typeparam name="T2">The second parameter type.</typeparam>
        /// <typeparam name="T3">The third parameter type.</typeparam>
        /// <typeparam name="TAttr">The attribute type.</typeparam>
        /// <param name="methodInfo">The defined method.</param>
        /// <param name="methodBuilder">The new method builder.</param>
        public static void TransferAttribute<T1, T2, T3, TAttr>(
            this MethodInfo methodInfo,
            MethodBuilder methodBuilder)
            where TAttr : Attribute
        {
            var attrs = methodInfo.GetCustomAttributes<TAttr>(false);
            if (attrs.IsNullOrEmpty() == true)
            {
                return;
            }

            methodInfo.TransferAttributesInternal<TAttr>(methodBuilder, attrs, new Type[] { typeof(T1), typeof(T2), typeof(T3) });
        }

        /// <summary>
        /// Transfers attributes from a defined method to a new one.
        /// </summary>
        /// <param name="methodInfo">The defined method.</param>
        /// <param name="methodBuilder">The new method builder.</param>
        /// <param name="attrs">A list of attributes.</param>
        /// <param name="ctorArgTypes">An array of constructor arguments types.</param>
        private static void TransferAttributesInternal<T>(
            this MethodInfo methodInfo,
            MethodBuilder methodBuilder,
            IEnumerable<T> attrs,
            Type[] ctorArgTypes)
        {
            var ctor = typeof(T).GetConstructor(ctorArgTypes);
            if (ctor == null)
            {
                return;
            }

            object[] ctorParmValues = new object[ctorArgTypes.Length];
            var parms = ctor.GetParameters();
            if (parms == null ||
                parms.Length != ctorArgTypes.Length)
            {
                return;
            }

            foreach (var attr in attrs)
            {
                List<string> ignoreProperties = new List<string>();
                for (int i = 0; i < parms.Length; i++)
                {
                    PropertyInfo prop = attr.GetType().GetProperty(parms[i].Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.GetProperty);
                    if (prop != null)
                    {
                        object value = prop.GetValue(attr);
                        if (value is Type)
                        {
                            value = Type.GetTypeFromHandle(((Type)value).TypeHandle);
                        }

                        ctorParmValues[i] = value;
                        ignoreProperties.Add(prop.Name);
                    }
                }

                methodBuilder.SetCustomAttribute(
                    BuildAttribute(
                        ctor,
                        ctorParmValues,
                        () =>
                        {
                            return GetAttributePropertyValues<T>(attr, ignoreProperties);
                        }));
            }
        }

        /// <summary>
        /// Gets an array for attribute property values.
        /// </summary>
        /// <typeparam name="TAttr">The attribute type.</typeparam>
        /// <param name="attr"></param>
        /// <param name="ignoreProperties"></param>
        /// <returns>An array of properties and values.</returns>
        internal static Tuple<PropertyInfo, object>[] GetAttributePropertyValues<TAttr>(
            TAttr attr,
            IEnumerable<string> ignoreProperties)
        {
            var properties = attr.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance);
            if (properties.IsNullOrEmpty() == false)
            {
                var propertyValues = new List<Tuple<PropertyInfo, object>>(properties.Length);
                for (int i = 0; i < properties.Length; i++)
                {
                    if (properties[i].DeclaringType == attr.GetType() &&
                        properties[i].SetMethod != null &&
                        (ignoreProperties == null || ignoreProperties.Contains(properties[i].Name, StringComparer.OrdinalIgnoreCase) == false))
                    {
                        object value = properties[i].GetValue(attr);
                        if (value is Type)
                        {
                            value = value as Type;
                        }

                        propertyValues.Add(
                            new Tuple<PropertyInfo, object>(
                                properties[i],
                                value));
                    }
                }

                if (propertyValues.Any())
                {
                    return propertyValues.ToArray();
                }
            }

            return null;
        }

        /// <summary>
        /// Builds an attribute with no constructor arguments.
        /// </summary>
        /// <param name="func">A function to return properties and values.</param>
        /// <returns>A <see cref="CustomAttributeBuilder"/> instance.</returns>
        public static CustomAttributeBuilder BuildAttribute<TResult>(
            Func<Tuple<PropertyInfo, object>[]> func = null)
            where TResult : Attribute
        {
            var ctor = typeof(TResult).GetConstructor(Type.EmptyTypes);
            return BuildAttribute(ctor, new object[0], func);
        }

        /// <summary>
        /// Builds an attribute with one constructor argument.
        /// </summary>
        /// <param name="constructorArg1">The constructor argument value.</param>
        /// <param name="func">A function to return properties and values.</param>
        /// <returns>A <see cref="CustomAttributeBuilder"/> instance.</returns>
        public static CustomAttributeBuilder BuildAttribute<T, TResult>(
            T constructorArg,
            Func<Tuple<PropertyInfo, object>[]> func = null)
            where TResult : Attribute
        {
            var ctor = typeof(TResult).GetConstructor(new Type[] { typeof(T) });
            return BuildAttribute(ctor, new object[] { constructorArg }, func);
        }

        /// <summary>
        /// Builds an attribute with two constructor arguments.
        /// </summary>
        /// <param name="constructorArg1">The first constructor argument value.</param>
        /// <param name="constructorArg2">The second constructor argument value.</param>
        /// <param name="func">A function to return properties and values.</param>
        /// <returns>A <see cref="CustomAttributeBuilder"/> instance.</returns>
        public static CustomAttributeBuilder BuildAttribute<T1, T2, TResult>(
            T1 constructorArg1,
            T2 constructorArg2,
            Func<Tuple<PropertyInfo, object>[]> func = null)
            where TResult : Attribute
        {
            var ctor = typeof(TResult).GetConstructor(new Type[] { typeof(T1), typeof(T2) });
            return BuildAttribute(ctor, new object[] { constructorArg1, constructorArg2 }, func);
        }

        /// <summary>
        /// Builds an attribute.
        /// </summary>
        /// <param name="ctor">The constructor.</param>
        /// <param name="ctorValues">The constructor parameter values.</param>
        /// <param name="func">A function to return properties and values.</param>
        /// <returns>A <see cref="CustomAttributeBuilder"/> instance.</returns>
        public static CustomAttributeBuilder BuildAttribute(
            ConstructorInfo ctor,
            object[] ctorValues,
            Func<Tuple<PropertyInfo, object>[]> func)
        {
            var results = func?.Invoke();
            if (results != null)
            {
                PropertyInfo[] propertyInfos = new PropertyInfo[results.Length];
                object[] propertyValues = new object[results.Length];
                for (int i = 0; i < results.Length; i++)
                {
                    propertyInfos[i] = results[i].Item1;
                    propertyValues[i] = results[i].Item2;
                }

                return new CustomAttributeBuilder(ctor, ctorValues, propertyInfos, propertyValues);
            }

            return new CustomAttributeBuilder(ctor, ctorValues);
        }
    }
}