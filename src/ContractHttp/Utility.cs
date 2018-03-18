namespace ContractHttp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
    }
}