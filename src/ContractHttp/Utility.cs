namespace ContractHttp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        /// <param name="list">The list to check.</param>
        /// <returns>True if null or empty; otherwise false.</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
        {
            return list == null ||
                list.Any() == false;
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
            Utility.ThrowIfArgumentNullOrEmpty(str, nameof(str));

            return str.ExpandString(keys.ToKeyValuePair(values));
        }

        /// <summary>
        /// Expands a string from a key/value list.
        /// </summary>
        /// <param name="str">The str to expand.</param>
        /// <param name="KeyValuePairs">A list of key/value pairs.</param>
        /// <returns>The expanded <see cref="string"/>.</returns>
        public static string ExpandString(this string str, IEnumerable<KeyValuePair<string, object>> keyValuePairs)
        {
            Utility.ThrowIfArgumentNullOrEmpty(str, nameof(str));

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
        /// Gets a service implemtation or default.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>A service implementation or default</returns>
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
        /// <returns>A service implementation or default</returns>
        public static object GetServiceOrDefault(this IServiceProvider serviceProvider, Type serviceType, Type defaultType)
        {
            if (serviceType.IsAssignableFrom(defaultType) == false)
            {
                throw new ArgumentException("Argument is not of type", nameof(defaultType));
            }

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

            var ctor = defaultType.GetConstructors().FirstOrDefault();
            if (ctor != null)
            {
                var parms = ctor.GetParameters();
                if (parms.Any() == true)
                {
                    object[] args = new object[parms.Length];
                    for (int i = 0; i < args.Length; i++)
                    {
                        args[i] = serviceProvider.GetRequiredService(parms[i].ParameterType);
                    }

                    return ctor.Invoke(args);
                }
            }

            return Activator.CreateInstance(defaultType);
        }
    }
}