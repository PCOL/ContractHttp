namespace ContractHttp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    using ContractHttp.Reflection;
    using ContractHttp.Reflection.Emit;
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

        private ILGenerator ilGen;

        private LocalBuilder localServiceCallAttrs;

        public ServiceCallFilterEmitter(Type type, ILGenerator ilGen )
        {
            this.Type = type;
            this.ilGen = ilGen;
            var attrs = this.Type.GetTypeInfo().GetCustomAttributes<ServiceCallFilterAttribute>();
            if (attrs != null &&
                attrs.Any() == true)
            {
                this.HasAttributes = true;
            }
            else
            {
                var methods = this.Type.GetTypeInfo().GetMethods();
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

        public Type Type { get; }

        public bool HasAttributes { get; }

        /// <summary>
        /// Emits IL to the 'OnExecuting' method on <see cref="ServiceCallFilterAttribute"/> instances.
        /// </summary>
        /// <param name="localController">A <see cref="LocalBuilder"/> containing a controller instance.</param>
        /// <param name="localServices">A <see cref="LocalBuilder"/> containing a services instance.</param>
        public void EmitExecuting(LocalBuilder localController, LocalBuilder localServices)
        {
            if (this.HasAttributes == false)
            {
                return;
            }

            this.localServiceCallAttrs = ilGen.DeclareLocal(typeof(IEnumerable<ServiceCallFilterAttribute>));
            this.ilGen.EmitGetCustomAttributes<ServiceCallFilterAttribute>(this.Type, this.localServiceCallAttrs);
    
            this.ilGen.EmitIfNotNullOrEmpty(
                this.localServiceCallAttrs,
                (il) =>
                {
                    il.EmitForEach(
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
        /// <param name="localController">A <see cref="LocalBuilder"/> containing a controller instance.</param>
        /// <param name="localServices">A <see cref="LocalBuilder"/> containing a services instance.</param>
        public void EmitExecuted(LocalBuilder localController, LocalBuilder localServices)
        {
            if (this.HasAttributes == false)
            {
                return;
            }

            this.ilGen.EmitIfNotNullOrEmpty(
                this.localServiceCallAttrs,
                (il) =>
                {
                    il.EmitForEach(
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
        /// <param name="localAttr">A <see cref="LocalBuilder"/> containing a attribute instance.</param>
        /// <param name="localController">A <see cref="LocalBuilder"/> containing a controller instance.</param>
        /// <param name="localServices">A <see cref="LocalBuilder"/> containing a services instance.</param>
        private void EmitExecuting(LocalBuilder localAttr, LocalBuilder localController, LocalBuilder localServices)
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
        /// <param name="localAttr">A <see cref="LocalBuilder"/> containing a attribute instance.</param>
        /// <param name="localController">A <see cref="LocalBuilder"/> containing a controller instance.</param>
        /// <param name="localServices">A <see cref="LocalBuilder"/> containing a services instance.</param>
        private void EmitExecuted(LocalBuilder localAttr, LocalBuilder localController, LocalBuilder localServices)
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
        /// <param name="localAttr">A <see cref="LocalBuilder"/> containing a attribute instance.</param>
        /// <param name="localController">A <see cref="LocalBuilder"/> containing a controller instance.</param>
        /// <param name="localServices">A <see cref="LocalBuilder"/> containing a <see cref="IServiceProvider"/> instance.</param>
        private void EmitPreamble(LocalBuilder localAttr, LocalBuilder localController, LocalBuilder localServices)
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