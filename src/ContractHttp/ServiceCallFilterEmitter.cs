namespace ContractHttp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
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
        public void EmitExecuting(ILocal localController, ILocal localServices)
        {
            if (this.HasAttributes == false)
            {
                return;
            }

            this.ilGen
                .DeclareLocal<IEnumerable<ServiceCallFilterAttribute>>(out this.localServiceCallAttrs)
                .EmitGetCustomAttributes<ServiceCallFilterAttribute>(this.Type, this.localServiceCallAttrs);

            this.ilGen.EmitIfNotNullOrEmpty(
                this.localServiceCallAttrs,
                (il) =>
                {
                    il.ForEach(
                        this.localServiceCallAttrs,
                        (item) =>
                        {
                            this.EmitExecuting(item, localController, localServices);
                        });
                });
        }

        /// <summary>
        /// Emits IL to the 'OnExecuted' method on <see cref="ServiceCallFilterAttribute"/> instances.
        /// </summary>
        /// <param name="localController">A <see cref="ILocal"/> containing a controller instance.</param>
        /// <param name="localServices">A <see cref="ILocal"/> containing a services instance.</param>
        public void EmitExecuted(ILocal localController, ILocal localServices)
        {
            if (this.HasAttributes == false)
            {
                return;
            }

            this.ilGen.EmitIfNotNullOrEmpty(
                this.localServiceCallAttrs,
                (il) =>
                {
                    il.ForEach(
                        this.localServiceCallAttrs,
                        (item) =>
                        {
                            this.EmitExecuted(item, localController, localServices);
                        });
                });
        }

        /// <summary>
        /// Emits IL to create a new instance of a <see cref="ServiceCallExecutingContext"/> and call the service call filter attributes
        /// 'OnExecuting' method.
        /// </summary>
        /// <param name="localAttr">A <see cref="ILocal"/> containing a attribute instance.</param>
        /// <param name="localController">A <see cref="ILocal"/> containing a controller instance.</param>
        /// <param name="localServices">A <see cref="ILocal"/> containing a services instance.</param>
        private void EmitExecuting(ILocal localAttr, ILocal localController, ILocal localServices)
        {
            this.EmitPreamble(localAttr, localController, localServices);

            // Create new instance of attribute and call on executing method.
            this.ilGen.Emit(OpCodes.Newobj, ctorExecuting);
            this.ilGen.Emit(OpCodes.Callvirt, onExecutingMethod);
        }

        /// <summary>
        /// Emits IL to create a new instance of a <see cref="ServiceCallExecutedContext"/> and call the service call filter attributes
        /// 'OnExecuted' method.
        /// </summary>
        /// <param name="localAttr">A <see cref="ILocal"/> containing a attribute instance.</param>
        /// <param name="localController">A <see cref="ILocal"/> containing a controller instance.</param>
        /// <param name="localServices">A <see cref="ILocal"/> containing a services instance.</param>
        private void EmitExecuted(ILocal localAttr, ILocal localController, ILocal localServices)
        {
            this.EmitPreamble(localAttr, localController, localServices);

            // Create new instance of attribute and call on executed method.
            this.ilGen.Emit(OpCodes.Newobj, ctorExecuted);
            this.ilGen.Emit(OpCodes.Callvirt, onExecutedMethod);
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
            this.ilGen.Emit(OpCodes.Ldloc_S, localAttr);
            this.ilGen.Emit(OpCodes.Ldloc_S, localController);
/*
            // Call factory method to convert IServiceProvider into IFrameworkServices
            this.ilGen.Emit(OpCodes.Ldloc_S, func);
            this.ilGen.Emit(OpCodes.Ldloc_S, argsArray);
            this.ilGen.Emit(OpCodes.Callvirt, delegateInvokeMethod);
*/
        }
    }
}