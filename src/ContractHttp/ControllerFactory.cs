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
            var controllerTypeName = TypeName(controllerInterface);
            HttpControllerAttribute attr = controllerInterface.GetCustomAttribute<HttpControllerAttribute>();
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
        /// <param name="controllerType">The controller type</param>
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
            string typeName = TypeName(controllerInterface);
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
            string controllerTypeName = TypeName(controllerInterface);
            HttpControllerAttribute attr = controllerInterface.GetCustomAttribute<HttpControllerAttribute>();
            if (attr != null &&
                attr.ControllerTypeName != null)
            {
                controllerTypeName = attr.ControllerTypeName;
            }

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

            this.ProcessAttributes(typeBuilder, controllerInterface);

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
/*
            ConstructorBuilder defaultConstructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                CallingConventions.HasThis,
                null);

            ILGenerator defaultIL = defaultConstructorBuilder.GetILGenerator();
            defaultIL.Emit(OpCodes.Ret);
*/
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

            // Type[] implementedInterfaces = context.NewType.GetInterfaces();
            // if (implementedInterfaces.IsNullOrEmpty() == false)
            // {
            //     foreach (Type iface in implementedInterfaces)
            //     {
            //         TypeFactoryContext ifaceContext = context.CreateTypeFactoryContext(iface);
            //         this.ImplementInterface(ifaceContext);
            //     }
            // }

            // Add the interface to the type.
            //context.TypeBuilder.AddInterfaceImplementation(context.NewType);

            // Iterate through the interafces members.
            foreach (var memberInfo in context.NewType.GetMembers())
            {
                if (memberInfo.MemberType == MemberTypes.Method)
                {
                    MethodInfo methodInfo = (MethodInfo)memberInfo;
                    Type[] methodArgs = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();

                    if (methodInfo.ContainsGenericParameters == false)
                    {
                        var methodBuilder = this.BuildMethod(context, methodInfo, methodArgs);

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

        /// <summary>
        /// Builds the method.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="methodInfo">The method to build.</param>
        /// <param name="methodArgs">The methods argument types.</param>
        /// <returns>The built method.</returns>
        private IMethodBuilder BuildMethod(
            ControllerFactoryContext context,
            MethodInfo methodInfo,
            Type[] methodArgs)
        {
            string name = methodInfo.Name;

            Type[] methodParmTypes = methodArgs;
            Dictionary<string, int> methodParmIndex = null;
            var methodParmAttrs = methodInfo.GetCustomAttributes<ControllerMethodParameterAttribute>();
            if (methodParmAttrs.Any() == true)
            {
                int len = methodParmAttrs.Count();
                methodParmTypes = new Type[len];
                methodParmIndex = new Dictionary<string, int>();

                int index = 0;
                foreach (var attr in methodParmAttrs)
                {
                    methodParmTypes[index] = attr.ParameterType;
                    methodParmIndex[attr.ParameterName] = index;
                    index++;
                }
            }

            MethodAttributes attrs = methodInfo.Attributes & ~MethodAttributes.Abstract; // & ~MethodAttributes.Virtual;

            //MethodAttributes attrs = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual;
            var methodBuilder = context
                .TypeBuilder
                .NewMethod(methodInfo.Name)
                .MethodAttributes(attrs)
                .CallingConvention(CallingConventions.HasThis)
                .Returns<IActionResult>();

                //methodParmTypes);

            var parms = methodInfo.GetParameters();
            if (methodParmAttrs.Any() == true)
            {
                this.BuildParameters(methodBuilder, methodParmAttrs);
            }
            else
            {
                this.BuildParameters(methodBuilder, parms);
            }

            Type returnModelType = methodInfo.ReturnType;
            int successStatusCode = 200;
            int failStatusCode = 500;
            var responseAttr = methodInfo.GetCustomAttribute<HttpResponseAttribute>();
            if (responseAttr != null)
            {
                successStatusCode = responseAttr.SuccessStatusCode;
                failStatusCode = responseAttr.FailureStatusCode;
                returnModelType = responseAttr.ModelType;
            }

            MethodInfo binderGetValueMethod = typeof(ContractHttp.Reflection.Binder).GetMethod("GetValue", new[] { typeof(object), typeof(string) });
            MethodInfo serviceMethod = context.BaseType.GetMethod(name, methodArgs);
            //MethodInfo getServiceCallAttrMethod = typeof(CustomAttributeExtensions).GetMethod("GetCustomAttribute", new[] { typeof(MemberInfo), typeof(bool) }).MakeGenericMethod(typeof(ServiceCallFilterAttribute));
            //MethodInfo getCurrentMethod = typeof(MethodBase).GetMethod("GetCurrentMethod", Type.EmptyTypes);
            //MethodInfo getMethod = typeof(ReflectionExtensions).GetMethod("GetMethod", new[] { typeof(Type), typeof(int) });
            PropertyInfo requestProp = typeof(ControllerBase).GetProperty("Request", typeof(HttpRequest));
            PropertyInfo headersProp = typeof(HttpRequest).GetProperty("Headers", typeof(IHeaderDictionary));
            PropertyInfo itemProp = typeof(IHeaderDictionary).GetProperty("Item", typeof(StringValues));
            PropertyInfo httpContextProp = typeof(HttpRequest).GetProperty("HttpContext", typeof(HttpContext));
            PropertyInfo requestServicesProp = typeof(HttpContext).GetProperty("RequestServices", typeof(IServiceProvider));

            // Emit Method IL
            IEmitter methodIL = methodBuilder.Body();
            methodIL.DeclareLocal<IActionResult>(out ILocal localResponse);
            ILocal localReturnValue = null;

            // Does the method return any data?
            if (methodInfo.ReturnType != typeof(void))
            {
                methodIL.DeclareLocal(serviceMethod.ReturnType, out localReturnValue);
            }

            methodIL.DeclareLocal<Exception>(out ILocal localEx);

            // Store Controller reference
            methodIL.DeclareLocal<Controller>(out ILocal localController);
            methodIL.Emit(OpCodes.Ldarg_0);
            methodIL.Emit(OpCodes.Stloc_S, localController);

            // Get Services
            methodIL.DeclareLocal<IServiceProvider>(out ILocal localServices);
            methodIL.Emit(OpCodes.Ldarg_0);
            methodIL.Emit(OpCodes.Callvirt, requestProp.GetGetMethod());
            methodIL.Emit(OpCodes.Callvirt, httpContextProp.GetGetMethod());
            methodIL.Emit(OpCodes.Callvirt, requestServicesProp.GetGetMethod());
            methodIL.Emit(OpCodes.Stloc_S, localServices);

            methodIL.BeginExceptionBlock(out ILabel blockLocal);

            // Check for service calls.
            var serviceCallEmitter = new ServiceCallFilterEmitter(methodInfo.DeclaringType, methodIL);
            serviceCallEmitter.EmitExecuting(localController, localServices);

            ILocal proxyArguments = null;
            if (methodParmAttrs.Any() == true)
            {
                // Load the proxy method arguments into an array.
                methodIL.DeclareLocal<object[]>(out proxyArguments);

                methodIL.Array(
                    typeof(object),
                    proxyArguments,
                    methodParmTypes.Length,
                    (i) =>
                    {
                        methodIL.LdArg(i + 1);
                        methodIL.Conv(methodParmTypes[i], typeof(object), false);
                    });

                methodIL.EmitWriteLine("------------------- PROXY ARGS --------------------");
                methodIL.EmitWriteLine(proxyArguments);

                // Load the service object.
                methodIL.Emit(OpCodes.Ldarg_0);
                methodIL.Emit(OpCodes.Ldfld, context.BaseObjectField);

                // Iterate through service parameters
                for (int i = 0; i < parms.Length; i++)
                {
                    var fromModelAttr = parms[i].GetCustomAttribute<FromParameterAttribute>();
                    if (fromModelAttr != null)
                    {
                        if (string.IsNullOrEmpty(fromModelAttr.PropertyName) == false)
                        {
                            methodIL.EmitLoadArrayElement(proxyArguments, methodParmIndex[fromModelAttr.ParameterName]);
                            methodIL.Emit(OpCodes.Ldstr, fromModelAttr.PropertyName);
                            methodIL.Emit(OpCodes.Call, binderGetValueMethod.MakeGenericMethod(parms[i].ParameterType));
                        }
                        else
                        {
                            int index = methodParmIndex[fromModelAttr.ParameterName];
                            methodIL.EmitLoadArrayElement(proxyArguments, index);
                            this.EmitConverter(methodIL, methodParmTypes[index], parms[i].ParameterType);
                        }
                        continue;
                    }

                    var fromHeaderAttr = parms[i].GetCustomAttribute<FromHeaderAttribute>();
                    if (fromHeaderAttr != null)
                    {
                        methodIL.Emit(OpCodes.Ldarg_0);
                        methodIL.Emit(OpCodes.Callvirt, requestProp.GetGetMethod());
                        methodIL.Emit(OpCodes.Callvirt, headersProp.GetGetMethod());
                        methodIL.Emit(OpCodes.Ldstr, fromHeaderAttr.Name);
                        methodIL.Emit(OpCodes.Callvirt, itemProp.GetGetMethod());

                        continue;
                    }

                    methodIL.LdArg(i + 1);
                }

                // Call the service method
                methodIL.Emit(OpCodes.Callvirt, serviceMethod);
            }
            else
            {
                methodIL.DeclareLocal<string>(out ILocal l);
                methodIL.Emit(OpCodes.Ldarg_1);
                methodIL.EmitToString();
                methodIL.Emit(OpCodes.Stloc_S, l);

                methodIL.EmitWriteLine("--------------------- ARGS --------------------");
                methodIL.EmitWriteLine(l);

                // Load the base object and method parameters then call the service method.
                methodIL.Emit(OpCodes.Ldarg_0);
                methodIL.Emit(OpCodes.Ldfld, context.BaseObjectField);
                methodIL.EmitLoadParameters(parms);
                methodIL.Emit(OpCodes.Callvirt, serviceMethod);
            }

            if (localReturnValue != null)
            {
                methodIL.Emit(OpCodes.Stloc_S, localReturnValue);

                methodIL.EmitIfNotNull(
                    localReturnValue,
                    (il) =>
                    {
                        il.EmitWriteLine("**************** SUCCESS *****************");
                        this.EmitStatusCodeCall(il, successStatusCode, localReturnValue, localResponse);
                    },
                    (il) =>
                    {
                        il.EmitWriteLine("**************** FAIL *****************");
                        this.EmitStatusCodeCall(il, failStatusCode, null, localResponse);
                    });
            }
            else
            {
                this.EmitStatusCodeCall(methodIL, successStatusCode, localResponse);
            }

            bool emitEx = true;
            var handlerAttrs = this.GetExceptionHandlers(methodInfo);
            if (handlerAttrs != null)
            {
                foreach (var attr in handlerAttrs)
                {
                    this.EmitCatchBlock(methodIL, attr, localResponse);
                    if (attr.ExceptionType == typeof(Exception))
                    {
                        emitEx = false;
                    }
                }
            }

            if (emitEx == true)
            {
                methodIL.BeginCatchBlock(typeof(Exception));
                methodIL.Emit(OpCodes.Stloc_S, localEx);

                methodIL.EmitWriteLine("--------------- EXCEPTION --------------");
                methodIL.EmitWriteLine(localEx);

                this.EmitStatusCodeCall(methodIL, StatusCodes.Status500InternalServerError, localResponse);
            }

            methodIL.BeginFinallyBlock();

            // Emit Service Call Attribute Executed calls.
            serviceCallEmitter.EmitExecuted(localController, localServices);

            methodIL.EndExceptionBlock();

            methodIL.Emit(OpCodes.Ldloc_S, localResponse);
            methodIL.Emit(OpCodes.Ret);

            if (this.ResolveMvcAttributes(methodBuilder, methodInfo) == false)
            {
                this.ResolveHttpControllerEndPointAttribute(methodBuilder, methodInfo);
            }

            this.ProcessAttributes(methodBuilder, methodInfo);

            return methodBuilder;
        }

        /// <summary>
        /// Build parameters.
        /// </summary>
        /// <param name="methodBuilder">The method builder.</param>
        /// <param name="parmInfos">An array of <see cref="Tuple{string, ParameterAttributes}"/> objects.</param>
        private void BuildParameters(
            IMethodBuilder methodBuilder,
            IEnumerable<ControllerMethodParameterAttribute> methodParms)
        {
            foreach (var methodParm in methodParms)
            {
                var parmBuilder = methodBuilder.Param(methodParm.ParameterType, methodParm.ParameterName);

                if (methodParm.From == ControllerMethodParameterFromOption.Body)
                {
                    parmBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<FromBodyAttribute>());
                }
                else if (methodParm.From == ControllerMethodParameterFromOption.Header)
                {
                    parmBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<FromHeaderAttribute>(
                        () =>
                        {
                            var prop = typeof(FromHeaderAttribute).GetProperty("Name");
                            return new[] { new Tuple<PropertyInfo, object>(prop, methodParm.FromName) };
                        }));

                }
                else if (methodParm.From == ControllerMethodParameterFromOption.Query)
                {
                    parmBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<FromQueryAttribute>(
                        () =>
                        {
                            var prop = typeof(FromQueryAttribute).GetProperty("Name");
                            return new[] { new Tuple<PropertyInfo, object>(prop, methodParm.FromName) };
                        }));

                }
                else if (methodParm.From == ControllerMethodParameterFromOption.Route)
                {
                    parmBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<FromRouteAttribute>(
                        () =>
                        {
                            var prop = typeof(FromQueryAttribute).GetProperty("Name");
                            return new[] { new Tuple<PropertyInfo, object>(prop, methodParm.FromName) };
                        }));
                }
            }
        }

        /// <summary>
        /// Build parameters.
        /// </summary>
        /// <param name="methodBuilder">The method builder.</param>
        /// <param name="parmInfos">An array of <see cref="ParameterInfo"/> objects.</param>
        private void BuildParameters(
            IMethodBuilder methodBuilder,
            ParameterInfo[] parmInfos)
        {
            for (int i = 0; i < parmInfos.Length; i++)
            {
                var parmBuilder = methodBuilder.Param(parmInfos[i].ParameterType, parmInfos[i].Name, parmInfos[i].Attributes); //  .DefineParameter(i + 1, parmInfos[i].Attributes, parmInfos[i].Name);
                var attrs = parmInfos[i].GetCustomAttributes();
                if (attrs == null)
                {
                    continue;
                }

                foreach (var attr in attrs)
                {
                    if (attr is FromBodyAttribute)
                    {
                        parmBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<FromBodyAttribute>());
                    }
                    else if (attr is FromHeaderAttribute)
                    {
                        parmBuilder.SetCustomAttribute(
                            AttributeUtility.BuildAttribute<FromHeaderAttribute>(
                            () =>
                            {
                                return this.GetPropertiesAndValues(attr, "Name");
                            }));
                    }
                    else if (attr is FromQueryAttribute)
                    {
                        parmBuilder.SetCustomAttribute(
                            AttributeUtility.BuildAttribute<FromQueryAttribute>(
                            () =>
                            {
                                return this.GetPropertiesAndValues(attr, "Name");
                            }));

                    }
                    else if (attr is FromRouteAttribute)
                    {
                        parmBuilder.SetCustomAttribute(
                            AttributeUtility.BuildAttribute<FromRouteAttribute>(
                            () =>
                            {
                                return this.GetPropertiesAndValues(attr, "Name");
                            }));
                    }
                    else if (attr is FromServicesAttribute)
                    {
                        parmBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<FromServicesAttribute>());                                
                    }
                }
            }
        }

        /// <summary>
        /// Gets a list of <see cref="PropertyInfo"/> objects and their values for a given object
        /// and list of property names.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="propertyNames">A list of property names.</param>
        /// <returns>A list of <see cref="PropertyInfo"/> and <see cref="object"/> instances.</returns>
        private Tuple<PropertyInfo, object>[] GetPropertiesAndValues(object obj, params string[] propertyNames)
        {
            Tuple<PropertyInfo, object>[] result = new Tuple<PropertyInfo, object>[propertyNames.Length];
            for (int i = 0; i < propertyNames.Length; i++)
            {
                var propertyInfo = obj.GetType().GetProperty(propertyNames[i]);
                result[i] = new Tuple<PropertyInfo, object>(propertyInfo, propertyInfo.GetValue(obj));
            }

            return result;
        }

        /// <summary>
        /// Returns a list of <see cref="ExceptionHandlerAttribute"/> instances for a method.
        /// </summary>
        /// <param name="methodInfo">The method info.</param>
        /// <returns>The list.</returns>
        private IEnumerable<ExceptionHandlerAttribute> GetExceptionHandlers(MethodInfo methodInfo)
        {
            var attrs = methodInfo.GetCustomAttributes().OfType<ExceptionHandlerAttribute>();
            return attrs;
        }

        /// <summary>
        /// Emits a catch block to return an <see cref="IActionResult"/>
        /// for a given <see cref="ExceptionHandlerAttribute"/>.
        /// </summary>
        /// <param name="methodIL">An IL Generator.</param>
        /// <param name="attr">Amn <see cref="ExceptionHandlerAttribute"/>.</param>
        /// <param name="localResponse">A local to receive the <see cref="IActionResult"/>.</param>
        private void EmitCatchBlock(IEmitter methodIL, ExceptionHandlerAttribute attr, ILocal localResponse)
        {
            methodIL.DeclareLocal(attr.ExceptionType, out ILocal localEx);

            methodIL.BeginCatchBlock(attr.ExceptionType);
            methodIL.Emit(OpCodes.Stloc_S, localEx);

            // Write out the exception
            methodIL.EmitWriteLine(localEx);

            if (attr.ModelType != null)
            {
                methodIL.DeclareLocal(attr.ModelType, out ILocal localModel);
                this.EmitModelBinder(methodIL, localEx, localModel);
                this.EmitStatusCodeCall(methodIL, attr.StatusCode, localModel, localResponse);
            }
            else
            {
                this.EmitStatusCodeCall(methodIL, attr.StatusCode, localResponse);
            }
        }

        private void EmitConverter(IEmitter methodIL, ILocal localFrom, Type toType)
        {
            methodIL.Emit(OpCodes.Ldloc_S, localFrom);
            EmitConverter(methodIL, localFrom.LocalType, toType);
        }

        private void EmitConverter(IEmitter methodIL, Type fromType, Type toType)
        {
            if (fromType == toType)
            {
                return;
            }

            if (toType == typeof(string))
            {
                if (fromType == typeof(byte[]))
                {
                    MethodInfo toBase64Method = typeof(Convert).GetMethod("ToBase64String", new[] { typeof(byte[]) });
                    methodIL.Emit(OpCodes.Call, toBase64Method);
                    return;
                }

                MethodInfo toStringMethod = typeof(Convert).GetMethod("ToString", new[] { fromType });
                methodIL.Emit(OpCodes.Call, toStringMethod);
                return;
            }
            else if (toType == typeof(byte[]))
            {
                if (fromType == typeof(string))
                {
                    MethodInfo fromBase64Method = typeof(Convert).GetMethod("FromBase64String", new[] { typeof(string) });
                    methodIL.Emit(OpCodes.Call, fromBase64Method);
                    return;
                }

                methodIL.ThrowException(typeof(NotSupportedException));
            }
            else if (toType == typeof(short))
            {
                MethodInfo toMethod = typeof(Convert).GetMethod("ToInt16", new[] { fromType });
                methodIL.Emit(OpCodes.Call, toMethod);
                return;
            }
            else if (toType == typeof(int))
            {
                MethodInfo toMethod = typeof(Convert).GetMethod("ToInt32", new[] { fromType });
                methodIL.Emit(OpCodes.Call, toMethod);
                return;
            }
            else if (toType == typeof(long))
            {
                MethodInfo toMethod = typeof(Convert).GetMethod("ToInt64", new[] { fromType });
                methodIL.Emit(OpCodes.Call, toMethod);
                return;
            }
            else if (toType == typeof(float))
            {
                MethodInfo toMethod = typeof(Convert).GetMethod("ToFloat", new[] { fromType });
                methodIL.Emit(OpCodes.Call, toMethod);
                return;
            }
            else if (toType == typeof(double))
            {
                MethodInfo toMethod = typeof(Convert).GetMethod("ToDouble", new[] { fromType });
                methodIL.Emit(OpCodes.Call, toMethod);
                return;
            }
            else if (toType == typeof(bool))
            {
                MethodInfo toMethod = typeof(Convert).GetMethod("ToBoolean", new[] { fromType });
                methodIL.Emit(OpCodes.Call, toMethod);
                return;
            }
            else if (toType == typeof(DateTime))
            {
                MethodInfo toMethod = typeof(Convert).GetMethod("ToDateTime", new[] { fromType });
                methodIL.Emit(OpCodes.Call, toMethod);
                return;
            }

            methodIL.ThrowException(typeof(NotSupportedException));
        }

        /// <summary>
        /// Binds a type to a model type.
        /// </summary>
        /// <param name="methodIL">An IL Generator.</param>
        /// <param name="localFrom">A local containing the type to bind from.</param>
        /// <param name="localTo">A local containing the type to bind to.</param>
        private void EmitModelBinder(IEmitter methodIL, ILocal localFrom, ILocal localTo)
        {
            //MethodInfo bindMethod = typeof(Binder).GetMethod("bind", new[] { typeof(Type), typeof(object) });
            MethodInfo getObjectMethod = typeof(Reflection.Binder).GetMethod("GetObject", Type.EmptyTypes).MakeGenericMethod(localTo.LocalType);
            ConstructorInfo binderCtor = typeof(Reflection.Binder).GetConstructor(new[] { typeof(object) });

            methodIL.DeclareLocal<Reflection.Binder>(out ILocal localBinder);

            // Create binder instance.
            methodIL.Emit(OpCodes.Ldloc_S, localFrom);
            methodIL.Emit(OpCodes.Newobj, binderCtor);
            methodIL.Emit(OpCodes.Stloc_S, localBinder);

            methodIL.Emit(OpCodes.Ldloc_S, localBinder);
            //methodIL.EmitTypeOf(localTo.LocalType);
            //methodIL.Emit(OpCodes.Callvirt, bindMethod);
            methodIL.Emit(OpCodes.Callvirt, getObjectMethod);
            methodIL.Emit(OpCodes.Stloc_S, localTo);
        }

        /// <summary>
        /// Emits a call to return a status code as an <see cref="IActionResult"/>.
        /// </summary>
        /// <param name="emitter">A IL Generator.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="localResponse">A local to receive the <see cref="IActionResult"/>.</param>
        private IEmitter EmitStatusCodeCall(IEmitter emitter, int statusCode, ILocal localResponse)
        {
            MethodInfo statusCodeMethod = typeof(ControllerBase).GetMethod("StatusCode", new Type[] { typeof(int)});

            emitter.Emit(OpCodes.Ldarg_0);
            emitter.Emit(OpCodes.Ldc_I4, statusCode);
            emitter.Emit(OpCodes.Callvirt, statusCodeMethod);
            emitter.Emit(OpCodes.Stloc_S, localResponse);

            return emitter;
        }

        /// <summary>
        /// Emits a call to the return a status code and result as an <see cref="IActionResult"/>
        /// </summary>
        /// <param name="emitter"></param>
        /// <param name="statusCode"></param>
        /// <param name="local"></param>
        /// <param name="localResponse"></param>
        private void EmitStatusCodeCall(IEmitter emitter, int statusCode, ILocal local, ILocal localResponse)
        {
            MethodInfo statusCodeWithResultMethod = typeof(ControllerBase).GetMethod("StatusCode", new Type[] { typeof(int), typeof(object)});
            emitter.Emit(OpCodes.Ldarg_0);
            emitter.Emit(OpCodes.Ldc_I4, statusCode);
            if (local != null)
            {
                emitter.Emit(OpCodes.Ldloc_S, local);
            }
            else
            {
                emitter.Emit(OpCodes.Ldnull);
            }

            emitter.Emit(OpCodes.Callvirt, statusCodeWithResultMethod);
            emitter.Emit(OpCodes.Stloc_S, localResponse);
        }


        private void ProcessAttributes(ITypeBuilder typeBuilder, Type type)
        {
            foreach (var attr in type.GetCustomAttributes())
            {
/*
                if (attr is SwaggerRequestHeaderParameterAttribute)
                {
                    typeBuilder.SetCustomAttribute(
                        AttributeUtility.BuildAttribute<string, SwaggerRequestHeaderParameterAttribute>(
                            ((SwaggerRequestHeaderParameterAttribute)attr).Header,
                            () => AttributeUtility.GetAttributePropertyValues<SwaggerRequestHeaderParameterAttribute>((SwaggerRequestHeaderParameterAttribute)attr, null)));
                }
                else
*/
                if (attr is ObsoleteAttribute)
                {
                    typeBuilder.SetCustomAttribute(
                        AttributeUtility.BuildAttribute<ObsoleteAttribute>(null));
                }
            }
        }

        private void ProcessAttributes(IMethodBuilder methodBuilder, MethodInfo methodInfo)
        {
            foreach (var attr in methodInfo.GetCustomAttributes())
            {
                if (attr is ProducesAttribute)
                {
                    methodBuilder.SetCustomAttribute(
                        AttributeUtility.BuildAttribute<Type, ProducesAttribute>(
                            ((ProducesAttribute)attr).Type,
                            () => AttributeUtility.GetAttributePropertyValues<ProducesAttribute>((ProducesAttribute)attr, new[] { "Type", "ContentTypes" })));
                }
                else if (attr is ProducesResponseTypeAttribute)
                {
                    methodBuilder.SetCustomAttribute(
                        AttributeUtility.BuildAttribute<int, ProducesResponseTypeAttribute>(
                            ((ProducesResponseTypeAttribute)attr).StatusCode,
                            () => AttributeUtility.GetAttributePropertyValues<ProducesResponseTypeAttribute>((ProducesResponseTypeAttribute)attr, new[] { "type" })));
                }
/*
                else if (attr is SwaggerParameterAttribute)
                {
                    methodBuilder.SetCustomAttribute(
                        AttributeUtility.BuildAttribute<string, SwaggerParameterAttribute>(
                            ((SwaggerParameterAttribute)attr).ParameterName,
                            () => AttributeUtility.GetAttributePropertyValues<SwaggerParameterAttribute>((SwaggerParameterAttribute)attr, null)));
                }
                else if (attr is SwaggerRequestHeaderParameterAttribute)
                {
                    methodBuilder.SetCustomAttribute(
                        AttributeUtility.BuildAttribute<string, SwaggerRequestHeaderParameterAttribute>(
                            ((SwaggerRequestHeaderParameterAttribute)attr).Header,
                            () => AttributeUtility.GetAttributePropertyValues<SwaggerRequestHeaderParameterAttribute>((SwaggerRequestHeaderParameterAttribute)attr, null)));
                }
*/
                else if (attr is ObsoleteAttribute)
                {
                    methodBuilder.SetCustomAttribute(
                        AttributeUtility.BuildAttribute<ObsoleteAttribute>(null));
                }
            }
        }

        /// <summary>
        /// Resolves any Mvc attributes on the method.
        /// </summary>
        /// <param name="methodBuilder">The method being built.</param>
        /// <param name="methodInfo">The method being called.</param>
        /// <returns>True if any resolved; otherwise false.</returns>
        private bool ResolveMvcAttributes(IMethodBuilder methodBuilder, MethodInfo methodInfo)
        {
            HttpGetAttribute getAttr = methodInfo.GetCustomAttribute<HttpGetAttribute>(true);
            if (getAttr != null)
            {
                methodBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<string, HttpGetAttribute>(getAttr.Template));
                return true;
            }

            HttpPostAttribute postAttr = methodInfo.GetCustomAttribute<HttpPostAttribute>(true);
            if (postAttr != null)
            {
                methodBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<string, HttpPostAttribute>(postAttr.Template));
                return true;
            }

            HttpPutAttribute putAttr = methodInfo.GetCustomAttribute<HttpPutAttribute>(true);
            if (putAttr != null)
            {
                methodBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<string, HttpPutAttribute>(putAttr.Template));
                return true;
            }

            HttpPatchAttribute patchAttr = methodInfo.GetCustomAttribute<HttpPatchAttribute>(true);
            if (patchAttr != null)
            {
                methodBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<string, HttpPatchAttribute>(patchAttr.Template));
                return true;
            }

            HttpDeleteAttribute deleteAttr = methodInfo.GetCustomAttribute<HttpDeleteAttribute>(true);
            if (deleteAttr != null)
            {
                methodBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<string, HttpDeleteAttribute>(deleteAttr.Template));
                return true;
            }

            return false;
        }

        private void ResolveHttpControllerEndPointAttribute(
            IMethodBuilder methodBuilder,
            MethodInfo methodInfo)
        {
            HttpControllerEndPointAttribute attr = methodInfo.GetCustomAttribute<HttpControllerEndPointAttribute>(false);
            if (attr != null)
            {
                if (attr.Method == HttpCallMethod.HttpGet)
                {
                    methodBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<string, HttpGetAttribute>(attr.Route));
                }
                else if (attr.Method == HttpCallMethod.HttpPost)
                {
                    methodBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<string, HttpPostAttribute>(attr.Route));
                }
                else if (attr.Method == HttpCallMethod.HttpPut)
                {
                    methodBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<string, HttpPutAttribute>(attr.Route));
                }
                else if (attr.Method == HttpCallMethod.HttpPatch)
                {
                    methodBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<string, HttpPatchAttribute>(attr.Route));
                }
                else if (attr.Method == HttpCallMethod.HttpDelete)
                {
                    methodBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<string, HttpDeleteAttribute>(attr.Route));
                }
            }
        }
    }
}
