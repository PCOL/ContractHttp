namespace ContractHttp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// Serialization extension methods.
    /// </summary>
    public static class SerializationExtensionMethods
    {
        /// <summary>
        /// Adds a JSON object serializer to dependency injection.
        /// </summary>
        /// <param name="services">A <see cref="IServiceCollection"/> instance.</param>
        /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection AddJsonObjectSerializer(this IServiceCollection services)
        {
            return services
                .AddObjectSerializer<JsonObjectSerializer>()
                .AddObjectSerializerFactory();
        }

        /// <summary>
        /// Adds an object serialiser to dependency injection.
        /// </summary>
        /// <param name="services">A <see cref="IServiceCollection"/> instance.</param>
        /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection AddObjectSerializer<T>(
            this IServiceCollection services)
            where T : class, IObjectSerializer
        {
            return services
                .AddScoped<IObjectSerializer, T>()
                .AddObjectSerializerFactory();
        }

        /// <summary>
        /// Adds an object serializer factory to dependency injection.
        /// </summary>
        /// <param name="services">A <see cref="IServiceCollection"/> instance.</param>
        /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection AddObjectSerializerFactory(this IServiceCollection services)
        {
            services.TryAddScoped<Func<string, IObjectSerializer>>(
                sp =>
                {
                    var list = sp.GetServices<IObjectSerializer>();
                    return (contentType) =>
                    {
                        return list.FirstOrDefault(s => s.ContentType == contentType);
                    };
                });

            return services;
        }
    }
}