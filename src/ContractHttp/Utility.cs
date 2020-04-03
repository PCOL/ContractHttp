namespace ContractHttp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Various utility methods.
    /// </summary>
    internal static class Utility
    {
        /// <summary>
        /// Throws an exception if the passed argument is null.
        /// </summary>
        /// <param name="argument">The arguement.</param>
        /// <param name="argumentName">The argument name.</param>
        public static void ThrowIfArgumentNull(object argument, string argumentName)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Throws an exception if the passed argument is null or empty.
        /// </summary>
        /// <param name="argument">The arguement.</param>
        /// <param name="argumentName">The argument name.</param>
        public static void ThrowIfArgumentNullOrEmpty(string argument, string argumentName)
        {
            Utility.ThrowIfArgumentNull(argument, argumentName);

            if (argument == string.Empty)
            {
                throw new ArgumentException("Argument is empty", argumentName);
            }
        }

        /// <summary>
        /// Throws an exception if the passed argument is null, empty or contains whitespace.
        /// </summary>
        /// <param name="argument">The arguement.</param>
        /// <param name="argumentName">The argument name.</param>
        public static void ThrowIfArgumentNullEmptyOrWhitespace(string argument, string argumentName)
        {
            Utility.ThrowIfArgumentNullOrEmpty(argument, argumentName);

            if (argument.IndexOf(' ') != -1)
            {
                throw new ArgumentException("Argument contains whitespace", argumentName);
            }
        }

        /// <summary>
        /// Throws an <see cref="InvalidOperationException"/> if the type is not an interface.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The types name.</param>
        public static void ThrowIfNotInterface(Type type, string name)
        {
            if (type.IsInterface == false)
            {
                throw new InvalidOperationException(string.Format("{0} must be an interface", name));
            }
        }

        /// <summary>
        /// Checks if a list is null or empty.
        /// </summary>
        /// <typeparam name="T">The list type.</typeparam>
        /// <param name="list">The list to check.</param>
        /// <returns>True if null or empty; otherwise false.</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
        {
            return list == null ||
                list.Any() == false;
        }

        /// <summary>
        /// Combines a uri and a path.
        /// </summary>
        /// <param name="uri">The uri.</param>
        /// <param name="path">The path.</param>
        /// <returns>The combined uri.</returns>
        public static string CombineUri(string uri, string path)
        {
            uri = uri ?? string.Empty;
            if (uri.EndsWith("/") == false)
            {
                uri += "/";
            }

            if (path.IsNullOrEmpty() == false)
            {
                uri += path.TrimStart('/');
            }

            return uri;
        }

        /// <summary>
        /// Expands a string from a key/value list.
        /// </summary>
        /// <param name="str">The str to expand.</param>
        /// <param name="keys">An list of keys.</param>
        /// <param name="values">A list of values.</param>
        /// <returns>The expanded <see cref="string"/>.</returns>
        public static string ExpandString(this string str, IEnumerable<string> keys, IEnumerable<object> values)
        {
            return str.ExpandString(keys.ToKeyValuePair(values));
        }

        /// <summary>
        /// Expands a string from a key/value list.
        /// </summary>
        /// <param name="str">The str to expand.</param>
        /// <param name="keyValuePairs">A list of key/value pairs.</param>
        /// <returns>The expanded <see cref="string"/>.</returns>
        public static string ExpandString(this string str, IEnumerable<KeyValuePair<string, object>> keyValuePairs)
        {
            int end = 0;
            int start = str.IndexOf('{');
            if (start != -1)
            {
                string result = string.Empty;
                while (start != -1)
                {
                    result += str.Substring(end, start - end);

                    end = str.IndexOf('}', start);
                    if (end == -1)
                    {
                        throw new Exception();
                    }

                    object value = null;
                    string name = str.Substring(start + 1, end - start - 1);

                    foreach (var item in keyValuePairs)
                    {
                        if (item.Key == name)
                        {
                            value = item.Value;
                            break;
                        }
                    }

                    if (value != null)
                    {
                        result += value.ToString();
                    }

                    start = str.IndexOf('{', ++end);
                }

                result += str.Substring(end);
                return result;
            }

            return str;
        }

        private static IEnumerable<KeyValuePair<TKey, TValue>> ToKeyValuePair<TKey, TValue>(this IEnumerable<TKey> keys, IEnumerable<TValue> values)
        {
            var keyEnumerator = keys.GetEnumerator();
            var valueEnumerator = values.GetEnumerator();

            while (keyEnumerator.MoveNext() == true)
            {
                var value = default(TValue);
                if (valueEnumerator.MoveNext() == true)
                {
                    value = valueEnumerator.Current;
                }

                yield return new KeyValuePair<TKey, TValue>(keyEnumerator.Current, value);
            }
        }

        /// <summary>
        /// Checks if the return type is a task.
        /// </summary>
        /// <param name="returnType">The return type.</param>
        /// <param name="taskType">A variable to receive the task type.</param>
        /// <returns>True if the return type is a <see cref="Task"/> and therefore asynchronous; otherwise false.</returns>
        public static bool IsAsync(this Type returnType, out Type taskType)
        {
            taskType = null;
            if (typeof(Task).IsAssignableFrom(returnType) == true)
            {
                taskType = returnType.GetGenericArguments().FirstOrDefault() ?? typeof(void);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the value of an objects property.
        /// </summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value to set.</param>
        public static void SetObjectProperty<T>(this object obj, string propertyName, T value)
        {
            var property = obj?.GetType().GetProperty(propertyName, typeof(T));
            if (property != null && property.SetMethod != null && property.SetMethod.IsPublic)
            {
                property.SetValue(obj, value);
            }
        }

        /// <summary>
        /// Set the value of an objects property by the type of property.
        /// </summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetObjectProperty<T>(this object obj, T value)
        {
            obj.SetObjectProperty(() => value);
        }

        /// <summary>
        /// Set the value of an objects property by the type of property.
        /// </summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="valueFunction">The function that provides the value.</param>
        public static void SetObjectProperty<T>(this object obj, Func<T> valueFunction)
        {
            obj?.GetType()
                .GetProperties().SetProperty(obj, valueFunction());
        }

        /// <summary>
        /// Set the value of a property by the type of property.
        /// </summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <param name="properties">A list of properties.</param>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetProperty<T>(this IEnumerable<PropertyInfo> properties, object obj, T value)
        {
            properties.SetProperty(obj, () => value);
        }

        /// <summary>
        /// Set the value of a property by the type of property.
        /// </summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <param name="properties">A list of properties.</param>
        /// <param name="obj">The object.</param>
        /// <param name="valueFunction">The function that provides the value.</param>
        public static void SetProperty<T>(this IEnumerable<PropertyInfo> properties, object obj, Func<T> valueFunction)
        {
            var property = properties?.FirstOrDefault(p => p.PropertyType == typeof(T));
            if (property != null)
            {
                property.SetValue(obj, valueFunction());
            }
        }

        /// <summary>
        /// Set the value of a property by the type of property.
        /// </summary>
        /// <param name="properties">A list of properties.</param>
        /// <param name="obj">The object.</param>
        /// <param name="propertyType">The properties type.</param>
        /// <param name="valueFunction">The function that provides the value.</param>
        public static void SetProperty(this IEnumerable<PropertyInfo> properties, object obj, Type propertyType, Func<object> valueFunction)
        {
            var property = properties?.FirstOrDefault(p => p.PropertyType == propertyType);
            if (property != null)
            {
                property.SetValue(obj, valueFunction());
            }
        }

        /// <summary>
        /// Gets a service implemtation or default.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <typeparam name="TDefault">The default type.</typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>A service implementation or default.</returns>
        public static T GetServiceOrDefault<T, TDefault>(this IServiceProvider serviceProvider)
            where T : class
            where TDefault : T
        {
            return (T)serviceProvider.GetServiceOrDefault(typeof(T), typeof(TDefault));
        }

        /// <summary>
        /// Gets a service implemtation or default.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="serviceType">The service type.</param>
        /// <param name="defaultType">The default service type.</param>
        /// <returns>A service implementation or default.</returns>
        public static object GetServiceOrDefault(this IServiceProvider serviceProvider, Type serviceType, Type defaultType)
        {
            if (serviceType.IsAssignableFrom(defaultType) == false)
            {
                throw new ArgumentException("Argument is not of type", nameof(defaultType));
            }

            if (serviceProvider != null)
            {
                var service = serviceProvider.GetService(serviceType);
                if (service != null)
                {
                    return service;
                }

                service = serviceProvider.GetService(defaultType);
                if (service != null)
                {
                    return service;
                }

                return serviceProvider.CreateInstance(defaultType);
            }

            return Activator.CreateInstance(defaultType);
        }

        /// <summary>
        /// Creates an instance of a type resolving any arguments using dependency injection.
        /// </summary>
        /// <typeparam name="T">The type to create.</typeparam>
        /// <param name="serviceProvider">A service provider.</param>
        /// <returns>An instance of the type.</returns>
        internal static T CreateInstance<T>(this IServiceProvider serviceProvider)
        {
            return (T)serviceProvider.CreateInstance(typeof(T));
        }

        /// <summary>
        /// Creates an instance of a type resolving any arguments using dependency injection.
        /// </summary>
        /// <param name="serviceProvider">A service provider.</param>
        /// <param name="instanceType">The type to create.</param>
        /// <returns>An instance of the type.</returns>
        internal static object CreateInstance(this IServiceProvider serviceProvider, Type instanceType)
        {
            var ctor = instanceType.GetConstructors().FirstOrDefault();
            if (ctor != null)
            {
                var parms = ctor.GetParameters();
                if (parms.Any() == true)
                {
                    object[] args = new object[parms.Length];
                    for (int i = 0; i < args.Length; i++)
                    {
                        args[i] = serviceProvider?.GetRequiredService(parms[i].ParameterType);
                    }

                    return ctor.Invoke(args);
                }
            }

            return Activator.CreateInstance(instanceType);
        }

        /// <summary>
        /// Checks if a type a subclass of a generic type.
        /// </summary>
        /// <param name="type">The generic type.</param>
        /// <param name="check">The type to check.</param>
        /// <returns>True if it is a subclass; otherwise false.</returns>
        public static bool IsSubclassOfGeneric(this Type type, Type check)
        {
            while (type != null && type != typeof(object))
            {
                var genType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (check == genType)
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }
    }
}