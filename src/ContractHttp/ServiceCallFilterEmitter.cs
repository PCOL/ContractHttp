namespace ContractHttp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Reflection.Emit;
    using ContractHttp.Reflection;
    using ContractHttp.Reflection.Emit;
    using FluentIL;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// An IL emitter for emitting calls to <see cref="ServiceCallFilterAttribute"/> instances.
    /// </summary>
    public class ServiceCallFilterEmitter
    {
        private static readonly ConstructorInfo ctorExecuting = typeof(ServiceCallExecutingContext).GetConstructor(new[] { typeof(Controller), typeof(IServiceProvider) });

        private static readonly ConstructorInfo ctorExecuted = typeof(ServiceCallExecutingContext).GetConstructor(new[] { typeof(Controller), typeof(IServiceProvider) });

        private static readonly MethodInfo onExecutingMethod = typeof(ServiceCallFilterAttribute).GetMethod("OnExecuting", new[] { typeof(ServiceCallExecutingContext) });

        private static readonly MethodInfo onExecutedMethod = typeof(ServiceCallFilterAttribute).GetMethod("OnExecuted", new[] { typeof(ServiceCallExecutedContext) });

        private static readonly MethodInfo getServiceMethod = typeof(IServiceProvider).GetMethod("GetService", new[] { typeof(Type) });

        private static readonly MethodInfo delegateInvokeMethod = typeof(Delegate).GetMethod("DynamicInvoke", new[] { typeof(object[]) });

        private static readonly MethodInfo toArrayTMethod =
            typeof(Enumerable)
                .BuildMethodInfo("ToArray")
                .IsGenericDefinition()
                .HasParameterTypes(typeof(IEnumerable<>))
                .FirstOrDefault();

        private IEmitter ilGen;

        private ILocal localServiceCallAttrs;

        /// <summary>
        /// Initialises a new instance of the <see cref="ServiceCallFilterEmitter"/> class.
        /// </summary>
        /// <param name="type">The type to be checked for <see cref="ServiceCallFilterAttributes"/> attributes..</param>
        /// <param name="ilGen">The IL generator to use.</param>
        public ServiceCallFilterEmitter(Type type, IEmitter ilGen)
        {
            this.Type = type;
            this.ilGen = ilGen;
            var attrs = this.Type.GetCustomAttributes<ServiceCallFilterAttribute>();
            if (attrs != null &&
                attrs.Any() == true)
            {
                this.HasAttributes = true;
            }
            else
            {
                var methods = this.Type.GetMethods();
                if (methods != null &&
                    methods.Any() == true)
                {
                    foreach (var method in methods)
                    {
                        var methodAttrs = method.GetCustomAttributes<ServiceCallFilterAttribute>();
                        if (methodAttrs != null &&
                            methodAttrs.Any() == true)
                        {
                            this.HasAttributes = true;
                            break;
                        }
                    }
                }
            }

            this.ilGen
                .DeclareLocal<ServiceCallFilterAttribute[]>(out this.localServiceCallAttrs);

            if (this.HasAttributes == true)
            {
                this.ilGen
                    .DeclareLocal<IEnumerable<ServiceCallFilterAttribute>>(out ILocal localAttrs)

                    .EmitGetCustomAttributes<ServiceCallFilterAttribute>(this.Type, localAttrs)

                    .LdLoc(localAttrs)
                    .Call(toArrayTMethod.MakeGenericMethod(typeof(ServiceCallFilterAttribute)))
                    .StLoc(this.localServiceCallAttrs);
            }
        }

        /// <summary>
        /// Gets the type being checked for <see cref="ServiceCallFilterAttribute"/> attributes.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// A value indicating whether or not the type has any of the attributes.
        /// </summary>
        public bool HasAttributes { get; }

        /// <summary>
        /// Emits IL to the 'OnExecuting' method on <see cref="ServiceCallFilterAttribute"/> instances.
        /// </summary>
        /// <param name="localController">A <see cref="LocalBuilder"/> containing a controller instance.</param>
        /// <param name="localServices">A <see cref="LocalBuilder"/> containing a services instance.</param>
        /// <param name="localResponse">A <see cref="ILocal"/> for any response.static</param>
        public void EmitExecuting(ILocal localController, ILocal localServices, ILocal localResponse)
        {
            if (this.HasAttributes == false)
            {
                return;
            }

            this.ilGen
                .EmitIfNotNullOrEmpty(
                    this.localServiceCallAttrs,
                    (il) =>
                    {
                    il
                        .DefineLabel(out ILabel afterLoop)

                        .For(
                            this.localServiceCallAttrs,
                            (em, index, item) =>
                            {
                                this.EmitExecuting(item, localController, localServices, localResponse);

                                il.LdLoc(localResponse)
                                    .BrTrue(afterLoop);
                            })

                        .MarkLabel(afterLoop);
                    });
        }

        /// <summary>
        /// Emits IL to the 'OnExecuted' method on <see cref="ServiceCallFilterAttribute"/> instances.
        /// </summary>
        /// <param name="localController">A <see cref="ILocal"/> containing a controller instance.</param>
        /// <param name="localServices">A <see cref="ILocal"/> containing a services instance.</param>
        /// <param name="localResponse">A <see cref="ILocal"/> for any response.static</param>
        public void EmitExecuted(ILocal localController, ILocal localServices, ILocal localResponse)
        {
            if (this.HasAttributes == false)
            {
                return;
            }

            this.ilGen.EmitIfNotNullOrEmpty(
                this.localServiceCallAttrs,
                (il) =>
                {
                    il
                        .DefineLabel(out ILabel afterLoop)

                        .For(
                            this.localServiceCallAttrs,
                            (em, index, item) =>
                            {
                                this.EmitExecuted(item, localController, localServices, localResponse);

                                em.LdLoc(localResponse)
                                    .BrTrue(afterLoop);
                            })

                        .MarkLabel(afterLoop);
                });
        }

        /// <summary>
        /// Emits IL to create a new instance of a <see cref="ServiceCallExecutingContext"/> and call the service call filter attributes
        /// 'OnExecuting' method.
        /// </summary>
        /// <param name="localAttr">A <see cref="ILocal"/> containing a attribute instance.</param>
        /// <param name="localController">A <see cref="ILocal"/> containing a controller instance.</param>
        /// <param name="localServices">A <see cref="ILocal"/> containing a services instance.</param>
        /// <param name="localResponse">A <see cref="ILocal"/> for any response.static</param>
        private void EmitExecuting(ILocal localAttr, ILocal localController, ILocal localServices, ILocal localResponse)
        {
            // Create new instance of attribute and call on executing method.
            this.ilGen
                .DeclareLocal<ServiceCallExecutedContext>(out ILocal context)

                .LdLoc(localController)
                .LdLoc(localServices)
                .Newobj(ctorExecuting)
                .StLocS(context)

                .LdLoc(localAttr)
                .LdLocS(context)
                .CallVirt(onExecutingMethod)
                .GetProperty("Response", context)
                .StLocS(localResponse);
        }

        /// <summary>
        /// Emits IL to create a new instance of a <see cref="ServiceCallExecutedContext"/> and call the service call filter attributes
        /// 'OnExecuted' method.
        /// </summary>
        /// <param name="localAttr">A <see cref="ILocal"/> containing a attribute instance.</param>
        /// <param name="localController">A <see cref="ILocal"/> containing a controller instance.</param>
        /// <param name="localServices">A <see cref="ILocal"/> containing a services instance.</param>
        /// <param name="localResponse">A <see cref="ILocal"/> for any response.static</param>
        private void EmitExecuted(ILocal localAttr, ILocal localController, ILocal localServices, ILocal localResponse)
        {
            // Create new instance of attribute and call on executed method.
            this.ilGen
                .DeclareLocal<ServiceCallExecutedContext>(out ILocal context)

                .LdLoc(localController)
                .LdLoc(localServices)
                .Newobj(ctorExecuted)
                .StLocS(context)

                .LdLoc(localAttr)
                .LdLocS(context)
                .CallVirt(onExecutedMethod)
                .GetProperty("Response", context)
                .StLocS(localResponse);
        }

        /// <summary>
        /// Emits IL to resolve <see cref="IServiceProvider"/> into a <see cref="IFrameworkServices"/> instance and
        /// setup the attribute and arguments to construct an instance of the attribute class.
        /// </summary>
        /// <param name="localAttr">A <see cref="ILocal"/> containing a attribute instance.</param>
        /// <param name="localController">A <see cref="ILocal"/> containing a controller instance.</param>
        /// <param name="localServices">A <see cref="ILocal"/> containing a <see cref="IServiceProvider"/> instance.</param>
        private void EmitPreamble(ILocal localAttr, ILocal localController, ILocal localServices)
        {
/*
            var funcType = typeof(Func<IServiceProvider, IFrameworkServices>);
            var func = this.ilGen.DeclareLocal(funcType);
            var argsArray = this.ilGen.DeclareLocal(typeof(object[]));

            // Build arguments array for delegate invoke.
            this.ilGen.EmitArray(
                typeof(object),
                argsArray,
                1,
                (il, index) =>
                {
                    il.Emit(OpCodes.Ldloc_S, localServices);
                });

            // Get the function from the IServiceProvider.
            this.ilGen.Emit(OpCodes.Ldloc_S, localServices);
            this.ilGen.EmitTypeOf(funcType);
            this.ilGen.Emit(OpCodes.Callvirt, getServiceMethod);
            this.ilGen.Emit(OpCodes.Stloc_S, func);
*/

            // Load the attribute instance + arguments
            this.ilGen
                .LdLoc(localAttr)
                .LdLoc(localController)
                .LdLoc(localServices);
/*
            // Call factory method to convert IServiceProvider into IFrameworkServices
            this.ilGen.Emit(OpCodes.Ldloc_S, func);
            this.ilGen.Emit(OpCodes.Ldloc_S, argsArray);
            this.ilGen.Emit(OpCodes.Callvirt, delegateInvokeMethod);
*/
        }
    }
}