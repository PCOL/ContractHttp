using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ContractHttp
{
    public static class SerializationExtensionMethods
    {
        public static IServiceCollection AddJsonObjectSerializer(this IServiceCollection services)
        {
            return services
                .AddObjectSerializer<JsonObjectSerializer>()
                .AddObjectSerializerFactory();
        }

        public static IServiceCollection AddObjectSerializer<T>(
            this IServiceCollection services)
            where T : class, IObjectSerializer
        {
            return services
                .AddScoped<IObjectSerializer, T>()
                .AddObjectSerializerFactory();
        }

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