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
        /// Adds the Newtonsoft JSON object serializer to dependency injection.
        /// </summary>
        /// <param name="services">A <see cref="IServiceCollection"/> instance.</param>
        /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection AddNewtonsoftJsonSerializer(this IServiceCollection services)
        {
            services.RemoveAll<IObjectSerializer>();

            return services
                .AddObjectSerializer<JsonObjectSerializer>()
                .AddObjectSerializerFactory();
        }

        /// <summary>
        /// Adds a JSON object serializer to dependency injection.
        /// </summary>
        /// <param name="services">A <see cref="IServiceCollection"/> instance.</param>
        /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
        public static IServiceCollection AddMicrosoftJsonSerializer(this IServiceCollection services)
        {
            services.RemoveAll<IObjectSerializer>();

            return services
                .AddObjectSerializer<TextJsonObjectSerializer>()
                .AddObjectSerializerFactory();
        }

        /// <summary>
        /// Adds an object serialiser to dependency injection.
        /// </summary>
        /// <typeparam name="T">The serializers type.</typeparam>
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

        /// <summary>
        /// Adds the Newtonsoft JSON object serializer to dependency injection.
        /// </summary>
        /// <param name="options">A <see cref="IServiceCollection"/> instance.</param>
        /// <returns>The <see cref="IServiceCollection"/> instance.</returns>
        public static HttpClientProxyOptions SetNewtonsoftJsonSerializer(this HttpClientProxyOptions options)
        {
            options.ObjectSerializer = new JsonObjectSerializer();

            return options;
        }

        /// <summary>
        /// Sets the options to use the Microsoft object serializer.
        /// </summary>
        /// <param name="options">A <see cref="HttpClientProxyOptions"/> instance.</param>
        /// <returns>The <see cref="HttpClientProxyOptions"/> instance.</returns>
        public static HttpClientProxyOptions SetMicrosoftJsonSerializer(this HttpClientProxyOptions options)
        {
            options.ObjectSerializer = new TextJsonObjectSerializer();
            return options;
        }
    }
}