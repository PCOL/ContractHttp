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
    }
}