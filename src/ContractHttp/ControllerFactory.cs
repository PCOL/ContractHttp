namespace ContractHttp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using ContractHttp.Reflection.Emit;
    using FluentIL;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Primitives;

    /// <summary>
    /// Controller factory.
    /// </summary>
    public class ControllerFactory
        : TypeFactory,
        IControllerFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerFactory"/> class.
        /// </summary>
        public ControllerFactory()
            : base("Http", "Controllers")
        {
        }

        /// <summary>
        /// Gets the name of an adapter type.
        /// </summary>
        /// <param name="controllerInterface">The controllers interface type.</param>
        /// <returns>The type name.</returns>
        public static string TypeName(Type controllerInterface)
        {
            return string.Format("{0}.{1}Controller", controllerInterface.Namespace, controllerInterface.Name.TrimStart('I'));
        }

        /// <summary>
        /// Gets the name of the controller type.
        /// </summary>
        /// <param name="controllerInterface">The controller interface type.</param>
        /// <returns>The type name.</returns>
        public static string GetTypeName(Type controllerInterface)
        {
            return GetTypeName(controllerInterface, out HttpControllerAttribute attr);
        }

        /// <summary>
        /// Gets the name of the controller type and the <see cref="HttpControllerAttribute"/> if one extists.
        /// </summary>
        /// <param name="controllerInterface">The controller interface type.</param>
        /// <param name="attr">A variable to receive the <see cref="HttpControllerAttribute"/> instance.</param>
        /// <returns>The type name.</returns>
        private static string GetTypeName(Type controllerInterface, out HttpControllerAttribute attr)
        {
            var controllerTypeName = TypeName(controllerInterface);
            attr = controllerInterface.GetCustomAttribute<HttpControllerAttribute>();
            if (attr != null &&
                attr.ControllerTypeName != null)
            {
                controllerTypeName = attr.ControllerTypeName;
            }

            return controllerTypeName;
        }

        /// <summary>
        /// Creates a controller type.
        /// </summary>
        /// <typeparam name="T">The interface describing the controller type.</typeparam>
        /// <param name="controllerServiceType">The controllers implementation type.</param>
        /// <returns>A new adapter type.</returns>
        public Type CreateControllerType<T>(Type controllerServiceType)
        {
            return this.CreateControllerType(typeof(T), controllerServiceType);
        }

        /// <summary>
        /// Create instance of a controller.
        /// </summary>
        /// <typeparam name="T">The interface type describing the controller.</typeparam>
        /// <param name="instance">The instance of the service the controlller will call into.</param>
        /// <returns>An instance of the controller.</returns>
        public T CreateController<T>(object instance)
        {
            if (instance == null)
            {
                return default(T);
            }

            return (T)this.CreateControllerInternal(instance, typeof(T));
        }

        /// <summary>
        /// Create instance of a controller.
        /// </summary>
        /// <param name="controllerType">The controller type.</param>
        /// <param name="instance">The instance of the service the controlller will call into.</param>
        /// <returns>An instance of the controller.</returns>
        public object CreateController(Type controllerType, object instance)
        {
            if (instance == null)
            {
                return null;
            }

            return this.CreateControllerInternal(instance, controllerType);
        }

        /// <summary>
        /// Creates an adapter object to represent the desired types.
        /// </summary>
        /// <param name="inst">The controllers service implementation.</param>
        /// <param name="controllerInterface">The controller interface to implement.</param>
        /// <returns>An instance of the adapter if valid; otherwise null.</returns>
        private object CreateControllerInternal(object inst, Type controllerInterface)
        {
            Type controllerType = this.CreateControllerType(controllerInterface, inst.GetType());
            return Activator.CreateInstance(controllerType, inst);
        }

        /// <summary>
        /// Creates an adapter type.
        /// </summary>
        /// <param name="controllerInterface">The controller interface to implement.</param>
        /// <param name="controllerServiceType">The controllers service implementation type.</param>
        /// <returns>A <see cref="Type"/> representing the new adapter.</returns>
        public Type CreateControllerType(Type controllerInterface, Type controllerServiceType)
        {
            string typeName = GetTypeName(controllerInterface);
            Type controllerType = this.GetType(typeName, true);

            if (controllerType == null)
            {
                controllerType = this.GenerateControllerType(controllerInterface, controllerServiceType);
            }

            return controllerType;
        }

        /// <summary>
        /// Generates the adapter type.
        /// </summary>
        /// <param name="controllerInterface">The controller interface to implement.</param>
        /// <param name="controllerServiceType">The controllers service implementation type.</param>
        /// <returns>A <see cref="Type"/> representing the new adapter.</returns>
        private Type GenerateControllerType(Type controllerInterface, Type controllerServiceType)
        {
            string controllerTypeName = GetTypeName(
                controllerInterface,
                out HttpControllerAttribute attr);

            var typeBuilder = this
                .NewType(controllerTypeName)
                    .Class()
                    .Public()
                    .BeforeFieldInit()
                    .InheritsFrom<Controller>();

            typeBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<ControllerAttribute>());

            if (attr != null &&
                string.IsNullOrEmpty(attr.RoutePrefix) == false)
            {
                typeBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<string, RouteAttribute>(attr.RoutePrefix));
            }

            typeBuilder.ProcessAttributes(controllerInterface);

            var controllerServiceTypeField = typeBuilder
                .NewField("controllerService", controllerServiceType)
                .Private();

            // Add a constructor to the type.
            var ctorBuilder = this.AddConstructor(
                typeBuilder,
                controllerServiceType,
                controllerServiceTypeField);

            var context = new ControllerFactoryContext(typeBuilder, controllerInterface, controllerServiceType, null, controllerServiceTypeField, null, ctorBuilder);

            this.ImplementInterface(context);

            // Create the type.
            return typeBuilder.CreateType();
        }

        /// <summary>
        /// Adds a constructor to the adapter type.
        /// </summary>
        /// <param name="typeBuilder">The <see cref="ITypeBuilder"/> use to construct the type.</param>
        /// <param name="controllerServiceType">The <see cref="Type"/> controllers service implementation.</param>
        /// <param name="controllerServiceTypeField">The <see cref="IFieldBuilder"/> which will hold the instance of the controllers service implementation type.</param>
        /// <returns>The <see cref="ConstructorBuilder"/> used to build the constructor.</returns>
        private IConstructorBuilder AddConstructor(
            ITypeBuilder typeBuilder,
            Type controllerServiceType,
            IFieldBuilder controllerServiceTypeField)
        {
            var constructorBuilder = typeBuilder
                .NewConstructor()
                .Public()
                .HideBySig()
                .SpecialName()
                .RTSpecialName()
                .CallingConvention(CallingConventions.HasThis)
                .Param(controllerServiceType, "controllerService");

            constructorBuilder
                .Body()
                    .LdArg0()
                    .LdArg1()
                    .StFld(controllerServiceTypeField)
                    .Ret();

            return constructorBuilder;
        }

        /// <summary>
        /// Implements the adapter types interfaces on the adapted type.
        /// </summary>
        /// <param name="context">The current adapter context.</param>
        private void ImplementInterface(ControllerFactoryContext context)
        {
            var propertyMethods = new Dictionary<string, IMethodBuilder>();

            // Iterate through the interafces members.
            foreach (var memberInfo in context.NewType.GetMembers())
            {
                if (memberInfo.MemberType == MemberTypes.Method)
                {
                    MethodInfo methodInfo = (MethodInfo)memberInfo;
                    Type[] methodArgs = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();

                    if (methodInfo.ContainsGenericParameters == false)
                    {
                        var controllerMethodBuilder = new ControllerMethodBuilder(
                            context,
                            methodInfo,
                            methodArgs);

                        var methodBuilder = controllerMethodBuilder.BuildMethod();

                        if (methodInfo.IsProperty() == true)
                        {
                            propertyMethods.Add(methodInfo.Name, methodBuilder);
                        }
                    }
                }
                else if (memberInfo.MemberType == MemberTypes.Property)
                {
                    var propertyBuilder = context
                        .TypeBuilder
                        .NewProperty(memberInfo.Name, ((PropertyInfo)memberInfo).PropertyType)
                        .Attributes(PropertyAttributes.SpecialName);

                    if (propertyMethods.TryGetValue(memberInfo.PropertyGetName(), out IMethodBuilder getMethod) == true)
                    {
                        propertyBuilder.GetMethod = getMethod;
                    }

                    if (propertyMethods.TryGetValue(memberInfo.PropertySetName(), out IMethodBuilder setMethod) == true)
                    {
                        propertyBuilder.SetMethod = setMethod;
                    }
                }
            }
        }
    }
}
