namespace ContractHttp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Http extension methods
    /// </summary>
    public static class HttpControllerExtensionMethods
    {
        /// <summary>
        /// A controller factory instance.
        /// </summary>
        private static ControllerFactory factory = new ControllerFactory();

        /// <summary>
        /// A list of controller types.
        /// </summary>
        private static List<Type> controllerTypes = new List<Type>();

        /// <summary>
        /// Add a dynamically generated controller to a service collection.
        /// </summary>
        /// <typeparam name="TController">The controller type.</typeparam>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="services">A service collection.</param>
        /// <param name="assemblies">A list to receive the assembly the controller is in.</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddDynamicController<TController, TService>(this IServiceCollection services, List<Assembly> assemblies)
            where TController : class
        {
            string controllerName = ControllerFactory.GetTypeName(typeof(TController));
            Type controllerType = controllerTypes.FirstOrDefault(ct => ct.FullName == controllerName);
            if (controllerType == null)
            {
                controllerType = factory.CreateControllerType<TController>(typeof(TService));
                assemblies.Add(controllerType.Assembly);
                controllerTypes.Add(controllerType);
            }

            services.AddTransient(controllerType,
                (sp) => {
                    TService service = sp.GetService<TService>();
                    var controller = factory.CreateController<TController>(service);
                    return controller;
                });

            return services;
        }

        /// <summary>
        /// Add a dynamically generated controller to a service collection.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="services">A service collection.</param>
        /// <param name="assemblies">A list to receive the assembly the controller is in.</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddDynamicController<TService>(this IServiceCollection services, List<Assembly> assemblies)
            where TService : class
        {
            string controllerName = ControllerFactory.GetTypeName(typeof(TService));
            Type controllerType = controllerTypes.FirstOrDefault(ct => ct.FullName == controllerName);
            if (controllerType == null)
            {
                controllerType = factory.CreateControllerType<TService>(typeof(TService));
                assemblies.Add(controllerType.Assembly);
                controllerTypes.Add(controllerType);
            }

            services.AddTransient(controllerType,
                (sp) => {
                    TService service = sp.GetService<TService>();
                    var controller = factory.CreateController(controllerType, service);
                    return controller;
                });

            return services;
        }

        /// <summary>
        /// Add a dynamically generated controller to a service collection.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="services">A service collection.</param>
        /// <param name="serviceImpl">The service implementation.</param>
        /// <param name="assemblies">A list to receive the assembly the controller is in.</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddDynamicController<TService>(
            this IServiceCollection services,
            TService serviceImpl,
            List<Assembly> assemblies)
        {
            return services.AddDynamicController(
                typeof(TService),
                serviceImpl,
                assemblies);
        }

        /// <summary>
        /// Add a dynamically generated controller to a service collection.
        /// </summary>
        /// <typeparam name="TController">The type that defines the controller</typeparam>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="services">A service collection.</param>
        /// <param name="serviceImpl">The service implementation.</param>
        /// <param name="assemblies">A list to receive the assembly the controller is in.</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddDynamicController<TController, TService>(
            this IServiceCollection services,
            TService serviceImpl,
            List<Assembly> assemblies)
        {
            return services.AddDynamicController(
                typeof(TController),
                typeof(TService),
                serviceImpl,
                assemblies);
        }

        /// <summary>
        /// Add a dynamically generated controller to a service collection.
        /// </summary>
        /// <param name="services">A service collection.</param>
        /// <param name="serviceType">The service type.</param>
        /// <param name="serviceImpl">The service implementation.</param>
        /// <param name="assemblies">A list to receive the assembly the controller is in.</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddDynamicController(
            this IServiceCollection services,
            Type serviceType,
            object serviceImpl,
            List<Assembly> assemblies)
        {
            return services.AddDynamicController(
                serviceType,
                serviceType,
                serviceImpl,
                assemblies);
        }

        /// <summary>
        /// Add a dynamically generated controller to a service collection.
        /// </summary>
        /// <param name="services">A service collection.</param>
        /// <param name="controllerContractType">A type that defines the controller</param>
        /// <param name="serviceType">The service type.</param>
        /// <param name="serviceImpl">The service implementation.</param>
        /// <param name="assemblies">A list to receive the assembly the controller is in.</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddDynamicController(
            this IServiceCollection services,
            Type controllerContractType,
            Type serviceType,
            object serviceImpl,
            List<Assembly> assemblies)
        {
            string controllerName = ControllerFactory.GetTypeName(controllerContractType);
            Type controllerType = controllerTypes.FirstOrDefault(ct => ct.FullName == controllerName);
            if (controllerType == null)
            {
                controllerType = factory.CreateControllerType(controllerContractType, serviceType);
                controllerTypes.Add(controllerType);
            }

            if (assemblies.FirstOrDefault(a => a == controllerType.Assembly) == null)
            {
                assemblies.Add(controllerType.Assembly);
            }

            services.AddSingleton(serviceType, serviceImpl);

            services.AddTransient(controllerType,
                (sp) => {
                    var service = sp.GetService(serviceType);
                    var controller = factory.CreateController(controllerType, service);
                    return controller;
                });

            return services;
        }

        /// <summary>
        /// Adds a list of assembly parts to a <see cref="IMvcBuilder"/>.
        /// </summary>
        /// <param name="mvcBuilder">An <see cref="IMvcBuilder"/> instance</param>
        /// <param name="assemblyParts"></param>
        /// <returns>The <see cref="IMvcBuilder"/> instance.</returns>
        public static IMvcBuilder AddAssemblyParts(
            this IMvcBuilder mvcBuilder,
            List<Assembly> assemblyParts)
        {
            foreach (var item in assemblyParts)
            {
                mvcBuilder.AddApplicationPart(item);
            }

            return mvcBuilder;
        }

        /// <summary>
        /// Adds a dynamic controller.
        /// </summary>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="mvcBuilder">An <see cref="IMvcBuilder"/> instance</param>
        /// <param name="serviceImpl">An instance of the service.</param>
        /// <returns>The <see cref="IMvcBuilder"/> instance.</returns>
        public static IMvcBuilder AddDynamicController<TService>(
            this IMvcBuilder mvcBuilder,
            TService serviceImpl)
            where TService : class
        {
            mvcBuilder
                .AddDynamicController(
                    typeof(TService),
                    typeof(TService));

            mvcBuilder.Services.AddSingleton<TService>(serviceImpl);

            return mvcBuilder;
        }

        /// <summary>
        /// Adds a dynamic controller.
        /// </summary>
        /// <typeparam name="TController">The type that defines the controller</typeparam>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <param name="mvcBuilder">An <see cref="IMvcBuilder"/> instance</param>
        /// <param name="serviceImpl">An instance of the service.</param>
        /// <returns>The <see cref="IMvcBuilder"/> instance.</returns>
        public static IMvcBuilder AddDynamicController<TController, TService>(
            this IMvcBuilder mvcBuilder,
            TService serviceImpl)
            where TService : class
        {
            mvcBuilder
                .AddDynamicController(
                    typeof(TController),
                    typeof(TService));

            mvcBuilder.Services.AddSingleton<TService>(serviceImpl);

            return mvcBuilder;
        }

        /// <summary>
        /// Adds a dynamic controller.
        /// </summary>
        /// <typeparam name="TController">The type that defines the controller</typeparam>
        /// <typeparam name="TService">The service type.</typeparam>
        /// <typeparam name="TServiceImpl">The service implementaion type</typeparam>
        /// <param name="mvcBuilder">An <see cref="IMvcBuilder"/> instance</param>
        /// <returns>The <see cref="IMvcBuilder"/> instance.</returns>
        public static IMvcBuilder AddDynamicController<TController, TService, TServiceImpl>(
            this IMvcBuilder mvcBuilder)
            where TService : class
            where TServiceImpl : class, TService
        {
            mvcBuilder
                .AddDynamicController(
                    typeof(TController),
                    typeof(TService));

            mvcBuilder.Services.AddTransient<TService, TServiceImpl>();

            return mvcBuilder;
        }

        /// <summary>
        /// Adds a dynamic controller.
        /// </summary>
        /// <param name="mvcBuilder">An <see cref="IMvcBuilder"/> instance</param>
        /// <param name="serviceType">The service type.</param>
        /// <param name="serviceImpl">The service implementation.</param>
        /// <returns>The <see cref="IMvcBuilder"/> instance.</returns>
        public static IMvcBuilder AddDynamicController(
            this IMvcBuilder mvcBuilder,
            Type serviceType,
            object serviceImpl)
        {
            return mvcBuilder
                .AddDynamicController(
                    serviceType,
                    serviceType,
                    serviceImpl);
        }

        /// <summary>
        /// Adds a dynamic controller.
        /// </summary>
        /// <param name="mvcBuilder">An <see cref="IMvcBuilder"/> instance</param>
        /// <param name="controllerContractType">The controller type.</param>
        /// <param name="serviceType">The service type.</param>
        /// <param name="serviceImpl">The service implementation.</param>
        /// <returns>The <see cref="IMvcBuilder"/> instance.</returns>
        public static IMvcBuilder AddDynamicController(
            this IMvcBuilder mvcBuilder,
            Type controllerContractType,
            Type serviceType,
            object serviceImpl)
        {
            mvcBuilder
                .AddDynamicController(
                    controllerContractType,
                    serviceType);

            mvcBuilder.Services.AddSingleton(serviceType, serviceImpl);

            return mvcBuilder;
        }

        /// <summary>
        /// Adds a dynamic controller.
        /// </summary>
        /// <param name="mvcBuilder">An <see cref="IMvcBuilder"/> instance</param>
        /// <param name="controllerContractType">The controller type.</param>
        /// <param name="serviceType">The service type.</param>
        /// <returns>The <see cref="IMvcBuilder"/> instance.</returns>
        private static IMvcBuilder AddDynamicController(
            this IMvcBuilder mvcBuilder,
            Type controllerContractType,
            Type serviceType)
        {
            string controllerName = ControllerFactory.GetTypeName(controllerContractType);
            Type controllerType = controllerTypes.FirstOrDefault(ct => ct.FullName == controllerName);
            if (controllerType == null)
            {
                controllerType = factory.CreateControllerType(controllerContractType, serviceType);
                controllerTypes.Add(controllerType);
            }

            mvcBuilder.AddApplicationPart(controllerType.Assembly);

            mvcBuilder.Services.AddTransient(controllerType,
                (sp) => {
                    var service = sp.GetService(serviceType);
                    var controller = factory.CreateController(controllerType, service);
                    return controller;
                });

            return mvcBuilder;
        }
    }
}