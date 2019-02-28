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
    /// Builds controler methods.
    /// </summary>
    public class ControllerMethodBuilder
    {
        /// <summary>
        /// <see cref="ControllerBase.Request"/> property info.
        /// </summary>
        private static readonly PropertyInfo RequestPropertyInfo = typeof(ControllerBase).GetProperty("Request", typeof(HttpRequest));

        /// <summary>
        /// <see cref="HttpRequest.Headers"/> property info.
        /// </summary>
        private static readonly PropertyInfo HeadersPropertyInfo = typeof(HttpRequest).GetProperty("Headers", typeof(IHeaderDictionary));

        /// <summary>
        /// <see cref="IHeaderDictionary"/> item property info.
        /// </summary>
        private static readonly PropertyInfo ItemPropertyInfo = typeof(IHeaderDictionary).GetProperty("Item", typeof(StringValues));

        /// <summary>
        /// <see cref="HttpRequest.HttpContext"/> property info.
        /// </summary>
        private static readonly PropertyInfo HttpContextPropertyInfo = typeof(HttpRequest).GetProperty("HttpContext", typeof(HttpContext));

        /// <summary>
        /// <see cref="HttpContext.RequestServices"/> property info.
        /// </summary>
        private static readonly PropertyInfo RequestServicesPropertyInfo = typeof(HttpContext).GetProperty("RequestServices", typeof(IServiceProvider));

        /// <summary>
        /// <see cref="ContractHttp.Reflection.Binder.GetValue{T}(object, string)"/> method info.
        /// </summary>
        private static readonly MethodInfo BinderGetValueMethod = typeof(ContractHttp.Reflection.Binder).GetMethod("GetValue", new[] { typeof(object), typeof(string) });

        /// <summary>
        /// The controller factory context.
        /// </summary>
        private readonly ControllerFactoryContext context;

        /// <summary>
        /// The method being built.
        /// </summary>
        private readonly MethodInfo methodInfo;

        /// <summary>
        /// The methods argument types.
        /// </summary>
        private readonly Type[] methodArgs;

        /// <summary>
        /// The method parameters.
        /// </summary>
        private readonly ParameterInfo[] methodParms;

        /// <summary>
        /// Method parameters attributes.
        /// </summary>
        private IEnumerable<ControllerMethodParameterAttribute> methodParmAttrs;

        /// <summary>
        /// Method parameter types.
        /// </summary>
        private Type[] methodParmTypes;

        /// <summary>
        /// Method parameter index.
        /// </summary>
        private Dictionary<string, int> methodParmIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerMethodBuilder"/> class.
        /// </summary>
        /// <param name="context">The controller factory context.</param>
        /// <param name="methodInfo">The method to build.</param>
        /// <param name="methodArgs">The methods argument types.</param>
        public ControllerMethodBuilder(
            ControllerFactoryContext context,
            MethodInfo methodInfo,
            Type[] methodArgs)
        {
            this.context = context;
            this.methodInfo = methodInfo;
            this.methodArgs = methodArgs;

            this.methodParms = this.methodInfo.GetParameters();
            this.CheckForMethodParameterAttributes();
        }

        /// <summary>
        /// Checks for any method parameter attributes on the method.
        /// </summary>
        private void CheckForMethodParameterAttributes()
        {
            this.methodParmAttrs = this.methodInfo.GetCustomAttributes<ControllerMethodParameterAttribute>();
            if (this.methodParmAttrs.Any() == true)
            {
                int len = this.methodParmAttrs.Count();
                this.methodParmTypes = new Type[len];
                this.methodParmIndex = new Dictionary<string, int>();

                int index = 0;
                foreach (var attr in this.methodParmAttrs)
                {
                    this.methodParmTypes[index] = attr.ParameterType;
                    this.methodParmIndex[attr.ParameterName] = index;
                    index++;
                }
            }
        }

        /// <summary>
        /// Builds the method.
        /// </summary>
        /// <returns>The built method.</returns>
        public IMethodBuilder BuildMethod()
        {
            string name = this.methodInfo.Name;

            MethodAttributes attrs = this.methodInfo.Attributes & ~MethodAttributes.Abstract;

            var methodBuilder = this.context
                .TypeBuilder
                .NewMethod(this.methodInfo.Name)
                .MethodAttributes(attrs)
                .CallingConvention(CallingConventions.HasThis)
                .Returns<IActionResult>();

            if (this.methodParmAttrs.Any() == true)
            {
                this.BuildParameters(methodBuilder, this.methodParmAttrs);
            }
            else
            {
                this.BuildParameters(methodBuilder, this.methodParms);
            }

            if (methodBuilder.ResolveMvcAttributes(this.methodInfo) == false)
            {
                methodBuilder.ResolveHttpControllerEndPointAttribute(this.methodInfo);
            }

            methodBuilder.ProcessAttributes(this.methodInfo);

            Type returnModelType = this.methodInfo.ReturnType;
            int successStatusCode = 200;
            int failStatusCode = 500;
            var responseAttr = this.methodInfo.GetCustomAttribute<HttpResponseAttribute>();
            if (responseAttr != null)
            {
                successStatusCode = responseAttr.SuccessStatusCode;
                failStatusCode = responseAttr.FailureStatusCode;
                returnModelType = responseAttr.ModelType;
            }

            MethodInfo serviceMethod = this.context.BaseType.GetMethod(name, this.methodArgs);

            // Emit Method IL
            IEmitter methodIL = methodBuilder.Body();
            methodIL.DeclareLocal<IActionResult>("actionResponse", out ILocal localResponse);
            ILocal localReturnValue = null;

            // Does the method return any data?
            if (this.methodInfo.ReturnType != typeof(void))
            {
                methodIL.DeclareLocal(serviceMethod.ReturnType, "returnValue", out localReturnValue);
            }

            methodIL
                .DeclareLocal<Exception>("exception", out ILocal localEx)
                .DeclareLocal<Controller>("controller", out ILocal localController)
                .DeclareLocal<IServiceProvider>("services", out ILocal localServices)
                .DefineLabel("endMethod", out ILabel endMethod)

                // Store Controller reference
                .Comment("== Load and store controller ==")
                .LdArg0()
                .StLoc(localController)

                // Get Services
                .Comment("== Load and store request service provider ==")
                .LdArg0()
                .CallVirt(RequestPropertyInfo.GetGetMethod())
                .CallVirt(HttpContextPropertyInfo.GetGetMethod())
                .CallVirt(RequestServicesPropertyInfo.GetGetMethod())
                .StLoc(localServices);

            // Check for service calls.
            var serviceCallEmitter = new ServiceCallFilterEmitter(this.methodInfo.DeclaringType, methodIL);
            serviceCallEmitter.EmitExecuting(localController, localServices, localResponse);

            methodIL
                .LdLoc(localResponse)
                .BrTrue(endMethod)
                .Try()
                .Comment("== Service Method Call ==");

            this.EmitServiceMethodCall(
                methodIL,
                serviceMethod);

            if (localReturnValue != null)
            {
                methodIL
                    .StLoc(localReturnValue)

                    .EmitIfNotNull(
                        localReturnValue,
                        (il) =>
                        {
                            il.EmitWriteLine("**************** SUCCESS *****************");
                            il.EmitStatusCodeCall(successStatusCode, localReturnValue, localResponse);
                        },
                        (il) =>
                        {
                            il.EmitWriteLine("**************** FAIL *****************");
                            il.EmitStatusCodeCall(failStatusCode, null, localResponse);
                        });
            }
            else
            {
                methodIL.EmitStatusCodeCall(successStatusCode, localResponse);
            }

            if (this.EmitCatchBlocks(methodIL, localResponse) == false)
            {
                methodIL.BeginCatchBlock(typeof(Exception));
                methodIL.Emit(OpCodes.Stloc_S, localEx);

                methodIL.EmitWriteLine("--------------- EXCEPTION --------------");
                methodIL.EmitWriteLine(localEx);

                methodIL.EmitStatusCodeCall(StatusCodes.Status500InternalServerError, localResponse);
            }

            methodIL
                .Finally();

            // Emit Service Call Attribute Executed calls.
            serviceCallEmitter.EmitExecuted(localController, localServices, localResponse);

            methodIL
                .EndExceptionBlock()

                .MarkLabel(endMethod)

                .Comment("== Load Response ==")
                .LdLoc(localResponse)
                .Ret();

            return methodBuilder;
        }

        /// <summary>
        /// Build parameters.
        /// </summary>
        /// <param name="methodBuilder">The method builder.</param>
        /// <param name="methodParms">An array of <see cref="ControllerMethodParameterAttribute"/> objects.</param>
        private void BuildParameters(
            IMethodBuilder methodBuilder,
            IEnumerable<ControllerMethodParameterAttribute> methodParms)
        {
            foreach (var methodParm in methodParms)
            {
                methodBuilder.Param(
                    parmBuilder =>
                    {
                        parmBuilder
                            .Name(methodParm.ParameterName)
                            .Type(methodParm.ParameterType);

                        this.ApplyParameterAttributes(
                            parmBuilder,
                            methodParm);
                    });
            }
        }

        /// <summary>
        /// Applies method parameter attributes to a parameter builder.
        /// </summary>
        /// <param name="parmBuilder">A parameter builder.</param>
        /// <param name="methodParm">A method parameter attribute.</param>
        private void ApplyParameterAttributes(
            IParameterBuilder parmBuilder,
            ControllerMethodParameterAttribute methodParm)
        {
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
                methodBuilder.Param(
                    (parmBuilder) =>
                    {
                        parmBuilder
                            .Type(parmInfos[i].ParameterType)
                            .Name(parmInfos[i].Name);
                            ////parmInfos[i].Attributes);

                        this.ApplyParameterAttributes(
                            parmInfos[i],
                            parmBuilder);
                    });
            }
        }

        /// <summary>
        /// Applies 'From...' attributes from a source parameter to a parameter builder.
        /// </summary>
        /// <param name="parameterInfo">A parameter info.</param>
        /// <param name="parameterBuilder">A parameter builder.</param>
        private void ApplyParameterAttributes(
            ParameterInfo parameterInfo,
            IParameterBuilder parameterBuilder)
        {
            var attrs = parameterInfo.GetCustomAttributes();
            if (attrs == null)
            {
                return;
            }

            foreach (var attr in attrs)
            {
                if (attr is FromBodyAttribute)
                {
                    parameterBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<FromBodyAttribute>());
                }
                else if (attr is FromHeaderAttribute)
                {
                    parameterBuilder.SetCustomAttribute(
                        AttributeUtility.BuildAttribute<FromHeaderAttribute>(
                        () =>
                        {
                            return this.GetPropertiesAndValues(attr, "Name");
                        }));
                }
                else if (attr is FromQueryAttribute)
                {
                    parameterBuilder.SetCustomAttribute(
                        AttributeUtility.BuildAttribute<FromQueryAttribute>(
                        () =>
                        {
                            return this.GetPropertiesAndValues(attr, "Name");
                        }));
                }
                else if (attr is FromRouteAttribute)
                {
                    parameterBuilder.SetCustomAttribute(
                        AttributeUtility.BuildAttribute<FromRouteAttribute>(
                        () =>
                        {
                            return this.GetPropertiesAndValues(attr, "Name");
                        }));
                }
                else if (attr is FromServicesAttribute)
                {
                    parameterBuilder.SetCustomAttribute(AttributeUtility.BuildAttribute<FromServicesAttribute>());
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
        /// Emits the service method call.
        /// </summary>
        /// <param name="methodIL">The il emitter.</param>
        /// <param name="serviceMethod">The service method.</param>
        private void EmitServiceMethodCall(
            IEmitter methodIL,
            MethodInfo serviceMethod)
        {
            ILocal proxyArguments = null;
            if (this.methodParmAttrs.Any() == true)
            {
                // Load the proxy method arguments into an array.
                methodIL
                    .DeclareLocal<object[]>("proxyArguments", out proxyArguments)

                    .Array(
                        typeof(object),
                        proxyArguments,
                        this.methodParmTypes.Length,
                        (i) =>
                        {
                            methodIL
                                .LdArg(i + 1)
                                .Conv(this.methodParmTypes[i], typeof(object), false);
                        });

                methodIL.EmitWriteLine("------------------- PROXY ARGS --------------------");
                methodIL.EmitWriteLine(proxyArguments);

                // Load the service object.
                methodIL
                    .LdArg0()
                    .LdFld(this.context.BaseObjectField);

                // Iterate through service parameters
                for (int i = 0; i < this.methodParms.Length; i++)
                {
                    var fromModelAttr = this.methodParms[i].GetCustomAttribute<FromParameterAttribute>();
                    if (fromModelAttr != null)
                    {
                        if (string.IsNullOrEmpty(fromModelAttr.PropertyName) == false)
                        {
                            methodIL
                                .EmitLoadArrayElement(proxyArguments, this.methodParmIndex[fromModelAttr.ParameterName])
                                .LdStr(fromModelAttr.PropertyName)
                                .Call(BinderGetValueMethod.MakeGenericMethod(this.methodParms[i].ParameterType));
                        }
                        else
                        {
                            int index = this.methodParmIndex[fromModelAttr.ParameterName];
                            methodIL
                                .EmitLoadArrayElement(proxyArguments, index)
                                .EmitConverter(this.methodParmTypes[index], this.methodParms[i].ParameterType);
                        }

                        continue;
                    }

                    var fromHeaderAttr = this.methodParms[i].GetCustomAttribute<FromHeaderAttribute>();
                    if (fromHeaderAttr != null)
                    {
                        methodIL
                            .LdArg0()
                            .CallVirt(RequestPropertyInfo.GetGetMethod())
                            .CallVirt(HeadersPropertyInfo.GetGetMethod())
                            .LdStr(fromHeaderAttr.Name)
                            .CallVirt(ItemPropertyInfo.GetGetMethod());

                        continue;
                    }

                    methodIL.LdArg(i + 1);
                }

                // Call the service method
                methodIL
                    .CallVirt(serviceMethod);
            }
            else
            {
                // Load the base object and method parameters then call the service method.
                methodIL
                    .LdArg0()
                    .LdFld(this.context.BaseObjectField)
                    .EmitLoadParameters(this.methodParms)
                    .CallVirt(serviceMethod);
            }
        }

        /// <summary>
        /// Emits any catch blocks.
        /// </summary>
        /// <param name="methodIL">The IL emitter.</param>
        /// <param name="localResponse">A local to receive the <see cref="IActionResult"/>.</param>
        /// <returns>True if a <see cref="Exception"/> catch block has been emittd; otherwise false.</returns>
        private bool EmitCatchBlocks(IEmitter methodIL, ILocal localResponse)
        {
            bool exceptionEmitted = false;
            var handlerAttrs = this.GetExceptionHandlers(this.methodInfo);
            if (handlerAttrs != null)
            {
                foreach (var attr in handlerAttrs)
                {
                    this.EmitCatchBlock(methodIL, attr, localResponse);
                    if (attr.ExceptionType == typeof(Exception))
                    {
                        exceptionEmitted = true;
                    }
                }
            }

            return exceptionEmitted;
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
                methodIL
                    .DeclareLocal(attr.ModelType, out ILocal localModel)
                    .EmitModelBinder(localEx, localModel)
                    .EmitStatusCodeCall(attr.StatusCode, localModel, localResponse);
            }
            else
            {
                methodIL.EmitStatusCodeCall(attr.StatusCode, localResponse);
            }
        }
    }
}