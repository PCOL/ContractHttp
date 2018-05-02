namespace ContractHttp.Reflection
{
    using System;
    using System.Reflection;

    public class Binder
    {
        private object obj;

        public Binder(object obj)
        {
            this.obj = obj;
        }

        /// <summary>
        /// Converts an object instance into another object instance,
        /// copying the matching property values over to the new instance.
        /// </summary>
        /// <typeparam name="T">The type of object to copy from.</typeparam>
        /// <typeparam name="TResult">The type of object to create and copy to.</typeparam>
        /// <param name="obj">The </param>
        /// <returns>An instance of typeparamref name="TResult".</returns>
        public static TResult Bind<T, TResult>(T obj)
            where T : class
            where TResult : class, new()
        {
            TResult result = new TResult();
            Bind<T, TResult>(obj, result);
            return result;
        }

        /// <summary>
        /// Converts an object instance into another object instance,
        /// copying the matching property values over to the new instance.
        /// </summary>
        /// <param name="typeResult">The type of object instance to create.</param>
        /// <param name="obj">The instance to copy the property values from.</param>
        /// <returns>A new instance of the required type.</returns>
        public static object Bind(Type typeResult, object obj)
        {
            if (typeResult == null)
            {
                throw new ArgumentNullException(nameof(typeResult));
            }

            if (obj != null)
            {
                object result = Activator.CreateInstance(typeResult);
                if (result != null)
                {
                    Bind(obj, result);
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Copies the matching property values from one object to another.
        /// </summary>
        /// <param name="source">The object to copy values from.</param>
        /// <param name="destination">The object instance to copy the values to.</param>
        public static void Bind(object source, object destination)
        {
            var props = destination.GetType().GetProperties();
            if (props != null)
            {
                foreach (var destinationProperty in props)
                {
                    var sourceProperty = source.GetType().GetProperty(destinationProperty.Name);
                    if (sourceProperty != null &&
                        sourceProperty.PropertyType == destinationProperty.PropertyType)
                    {
                        destinationProperty.SetValue(destination, sourceProperty.GetValue(source));
                    }
                }
            }
        }

        /// <summary>
        /// Copies the matching property values from one object to another.
        /// </summary>
        /// <param name="source">The object to copy values from.</param>
        /// <param name="destination">The object instance to copy the values to.</param>
        public static void Bind<TSource, TDestination>(TSource source, TDestination destination)
        {
            var props = typeof(TSource).GetProperties();
            foreach (var prop in props)
            {
                var propertyInfo = typeof(TDestination).GetProperty(prop.Name);
                if (propertyInfo != null &&
                    propertyInfo.PropertyType == prop.PropertyType)
                {
                    propertyInfo.SetValue(destination, prop.GetValue(source));
                }
            }
        }

        /// <summary>
        /// Gets the value of a specific paramref name="propertyName".
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static T GetValue<T>(object obj, string propertyName)
        {
            var prop = obj.GetType().GetProperty(propertyName);
            if (prop != null)
            {
                return (T)prop.GetValue(obj);
            }

            return default(T);
        }

        public static bool TryGetValue<T>(object obj, string propertyName, out T value)
        {
            value = default(T);
            var prop = obj.GetType().GetProperty(propertyName);
            if (prop != null)
            {
                value = (T)prop.GetValue(obj);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets an object of type from the binder.
        /// </summary>
        /// <returns></returns>
        public TResult GetObject<TResult>()
        {
            return (TResult)Bind(typeof(TResult), this.obj);
        }

        public TResult GetValue<TResult>(string propertyName)
        {
            return GetValue<TResult>(this.obj, propertyName);
        }

        public bool TryGetValue<TResult>(string propertyName, out TResult value)
        {
            return TryGetValue<TResult>(this.obj, propertyName, out value);
        }
    }
}