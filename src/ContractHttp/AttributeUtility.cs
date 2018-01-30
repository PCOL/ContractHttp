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
        public static void TransferAttribute<TResult>(this MethodInfo methodInfo, MethodBuilder methodBuilder)
            where TResult : Attribute
        {
            var attr = methodInfo.GetCustomAttribute<TResult>(false);
            if (attr != null)
            {
                methodBuilder.SetCustomAttribute(
                    BuildAttribute<TResult>(
                        () =>
                        {
                            return GetAttributePropertyValues<TResult>(attr, new string[0]);
                        }));
            }
        }

        public static void TransferAttributes<T, TResult>(this MethodInfo methodInfo, MethodBuilder methodBuilder)
            where TResult : Attribute
        {
            var attrs = methodInfo.GetCustomAttributes<TResult>(false);
            if (attrs.IsNullOrEmpty() == true)
            {
                return;
            }

            methodInfo.TransferAttributesInternal<TResult>(methodBuilder, attrs, new Type[] { typeof(T) });
        }

        public static void TransferAttribute<T1, T2, TResult>(this MethodInfo methodInfo, MethodBuilder methodBuilder)
            where TResult : Attribute
        {
            var attrs = methodInfo.GetCustomAttributes<TResult>(false);
            if (attrs.IsNullOrEmpty() == true)
            {
                return;
            }

            methodInfo.TransferAttributesInternal<TResult>(methodBuilder, attrs, new Type[] { typeof(T1), typeof(T2) });
        }

        public static void TransferAttribute<T1, T2, T3, TResult>(this MethodInfo methodInfo, MethodBuilder methodBuilder)
            where TResult : Attribute
        {
            var attrs = methodInfo.GetCustomAttributes<TResult>(false);
            if (attrs.IsNullOrEmpty() == true)
            {
                return;
            }

            methodInfo.TransferAttributesInternal<TResult>(methodBuilder, attrs, new Type[] { typeof(T1), typeof(T2), typeof(T3) });
        }

        private static void TransferAttributesInternal<T>(this MethodInfo methodInfo, MethodBuilder methodBuilder, IEnumerable<T> attrs, Type[] ctorArgTypes)
        {
// Console.WriteLine("Type: {0}", typeof(T).Name);
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

// Console.WriteLine("Value: {0} - {1}", value, value == typeof(Type));
                        }
                        
                        ctorParmValues[i] = value;
                        ignoreProperties.Add(prop.Name);
                    }
                }
                
                methodBuilder.SetCustomAttribute(
                    BuildAttributeInternal(
                        ctor,
                        ctorParmValues,
                        () =>
                        {
                            return GetAttributePropertyValues<T>(attr, ignoreProperties);
                        }));
            }
        }

        internal static Tuple<PropertyInfo, object>[] GetAttributePropertyValues<TResult>(TResult attr, IEnumerable<string> ignoreProperties)
        {
            var properties = typeof(TResult).GetProperties(BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance);
            if (properties.IsNullOrEmpty() == false)
            {
                var propertyValues = new List<Tuple<PropertyInfo, object>>(properties.Length);
                for (int i = 0; i < properties.Length; i++)
                {
                    if (properties[i].DeclaringType == typeof(TResult) &&
                        properties[i].SetMethod != null &&
                        (ignoreProperties == null || ignoreProperties.Contains(properties[i].Name, StringComparer.OrdinalIgnoreCase) == false))
                    {
                        object value = properties[i].GetValue(attr);
                        if (value is Type)
                        {
                            value = value as Type;
// Console.WriteLine("Property: {0} - {1}", value, value == typeof(Type));
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

        public static CustomAttributeBuilder BuildAttribute<TResult>(Func<Tuple<PropertyInfo, object>[]> action = null)
            where TResult : Attribute
        {
            var ctor = typeof(TResult).GetConstructor(Type.EmptyTypes);
            return BuildAttributeInternal(ctor, new object[0], action);
        }

        public static CustomAttributeBuilder BuildAttribute<T, TResult>(T constructorArg, Func<Tuple<PropertyInfo, object>[]> action = null)
            where TResult : Attribute
        {
            var ctor = typeof(TResult).GetConstructor(new Type[] { typeof(T) });
            return BuildAttributeInternal(ctor, new object[] { constructorArg }, action);
        }

        public static CustomAttributeBuilder BuildAttribute<T1, T2, TResult>(T1 constructorArg1, T2 constructorArg2, Func<Tuple<PropertyInfo, object>[]> action = null)
            where TResult : Attribute
        {
            var ctor = typeof(TResult).GetConstructor(new Type[] { typeof(T1), typeof(T2) });
            return BuildAttributeInternal(ctor, new object[] { constructorArg1, constructorArg2 }, action);
        }

        public static CustomAttributeBuilder BuildAttributeInternal(ConstructorInfo ctor, object[] ctorValues, Func<Tuple<PropertyInfo, object>[]> action)
        {
            if (action != null)
            {
                var results = action();
                if (results != null)
                {
                    PropertyInfo[] propertyInfos = new PropertyInfo[results.Length];
                    object[] propertyValues = new object[results.Length];
                    for (int i = 0; i < results.Length; i++)
                    {
                        propertyInfos[i] = results[i].Item1;
                        propertyValues[i] = results[i].Item2;
                    }
/*
Console.WriteLine("Ctor: {0}", ctor);
Console.WriteLine("CtorValues: {0}", string.Join(", ", ctorValues.Select(v => v.GetType().Name)));
Console.WriteLine("PropertyInfos: {0}", string.Join<string>(", ", propertyInfos.Select(p => p.GetType().Name)));
Console.WriteLine("PropertyValues: {0}", string.Join(", ", propertyValues));
*/
                    return new CustomAttributeBuilder(ctor, ctorValues, propertyInfos, propertyValues);
                }
            }
                
            return new CustomAttributeBuilder(ctor, ctorValues);            
        }
    }
}