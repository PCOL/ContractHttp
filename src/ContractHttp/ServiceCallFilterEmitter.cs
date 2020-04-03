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
        private static readonly ConstructorInfo CtorExecuting = typeof(ServiceCallExecutingContext).GetConstructor(new[] { typeof(Controller), typeof(IServiceProvider) });

        private static readonly ConstructorInfo CtorExecuted = typeof(ServiceCallExecutedContext).GetConstructor(new[] { typeof(Controller), typeof(IServiceProvider) });

        private static readonly MethodInfo OnExecutingMethod = typeof(ServiceCallFilterAttribute).GetMethod("OnExecuting", new[] { typeof(ServiceCallExecutingContext) });

        private static readonly MethodInfo OnExecutedMethod = typeof(ServiceCallFilterAttribute).GetMethod("OnExecuted", new[] { typeof(ServiceCallExecutedContext) });

        private static readonly MethodInfo GetServiceMethod = typeof(IServiceProvider).GetMethod("GetService", new[] { typeof(Type) });

        private static readonly MethodInfo DelegateInvokeMethod = typeof(Delegate).GetMethod("DynamicInvoke", new[] { typeof(object[]) });

        private static readonly MethodInfo ToArrayTMethod =
            typeof(Enumerable)
                .BuildMethodInfo("ToArray")
                .IsGenericDefinition()
                .HasParameterTypes(typeof(IEnumerable<>))
                .FirstOrDefault();

        private IEmitter emitter;

        private ILocal localServiceCallAttrs;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceCallFilterEmitter"/> class.
        /// </summary>
        /// <param name="type">The type to be checked for <see cref="ServiceCallFilterAttribute"/> attributes..</param>
        /// <param name="emitter">The IL generator to use.</param>
        public ServiceCallFilterEmitter(Type type, IEmitter emitter)
        {
            this.Type = type;
            this.emitter = emitter;
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

            this.emitter
                .DeclareLocal<ServiceCallFilterAttribute[]>("serviceCallAttributes", out this.localServiceCallAttrs);

            if (this.HasAttributes == true)
            {
                this.emitter
                    .DeclareLocal<IEnumerable<ServiceCallFilterAttribute>>(out ILocal localAttrs)

                    .EmitGetCustomAttributes<ServiceCallFilterAttribute>(this.Type, localAttrs)

                    .LdLoc(localAttrs)
                    .Call(ToArrayTMethod.MakeGenericMethod(typeof(ServiceCallFilterAttribute)))
                    .StLoc(this.localServiceCallAttrs);
            }
        }

        /// <summary>
        /// Gets the type being checked for <see cref="ServiceCallFilterAttribute"/> attributes.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets a value indicating whether or not the type has any of the attributes.
        /// </summary>
        public bool HasAttributes { get; }

        /// <summary>
        /// Emits IL to the 'OnExecuting' method on <see cref="ServiceCallFilterAttribute"/> instances.
        /// </summary>
        /// <param name="localController">A <see cref="LocalBuilder"/> containing a controller instance.</param>
        /// <param name="localServices">A <see cref="LocalBuilder"/> containing a services instance.</param>
        /// <param name="localResponse">A <see cref="ILocal"/> for any response.</param>
        public void EmitExecuting(ILocal localController, ILocal localServices, ILocal localResponse)
        {
            if (this.HasAttributes == false)
            {
                return;
            }

            this.emitter
                .Comment("== Service Call Filter Emit Executing ==")
                .EmitIfNotNullOrEmpty(
                    this.localServiceCallAttrs,
                    il => il
                        .DefineLabel("executingAfterLoop", out ILabel afterLoop)

                        .For(
                            this.localServiceCallAttrs,
                            (em, index, item) =>
                            {
                                this.EmitExecuting(item, localController, localServices, localResponse);

                                il
                                    .LdLoc(localResponse)
                                    .BrTrue(afterLoop);
                            })

                        .MarkLabel(afterLoop));
        }

        /// <summary>
        /// Emits IL to the 'OnExecuted' method on <see cref="ServiceCallFilterAttribute"/> instances.
        /// </summary>
        /// <param name="localController">A <see cref="ILocal"/> containing a controller instance.</param>
        /// <param name="localServices">A <see cref="ILocal"/> containing a services instance.</param>
        /// <param name="localResponse">A <see cref="ILocal"/> for any response.</param>
        public void EmitExecuted(ILocal localController, ILocal localServices, ILocal localResponse)
        {
            if (this.HasAttributes == false)
            {
                return;
            }

            this.emitter
                .Comment("== Service Call Filter Emit Executed ==")
                .EmitIfNotNullOrEmpty(
                    this.localServiceCallAttrs,
                    il => il
                        .DefineLabel("executedAfterLoop", out ILabel afterLoop)

                        .For(
                            this.localServiceCallAttrs,
                            (em, index, item) =>
                            {
                                this.EmitExecuted(item, localController, localServices, localResponse);

                                em
                                    .LdLoc(localResponse)
                                    .BrTrue(afterLoop);
                            })

                        .MarkLabel(afterLoop));
        }

        /// <summary>
        /// Emits IL to create a new instance of a <see cref="ServiceCallExecutingContext"/> and call the service call filter attributes
        /// 'OnExecuting' method.
        /// </summary>
        /// <param name="localAttr">A <see cref="ILocal"/> containing a attribute instance.</param>
        /// <param name="localController">A <see cref="ILocal"/> containing a controller instance.</param>
        /// <param name="localServices">A <see cref="ILocal"/> containing a services instance.</param>
        /// <param name="localResponse">A <see cref="ILocal"/> for any response.</param>
        private void EmitExecuting(ILocal localAttr, ILocal localController, ILocal localServices, ILocal localResponse)
        {
            // Create new instance of attribute and call on executing method.
            this.emitter
                .DeclareLocal<ServiceCallExecutingContext>("excutingContext", out ILocal context)
                .DeclareLocal<IActionResult>("executingResponse", out ILocal contextResponse)
                .DefineLabel("executingEnd", out ILabel executingEnd)

                .Comment("== Service Call Filter Executing ==")

                .LdLoc(localController)
                .LdLoc(localServices)
                .Newobj(CtorExecuting)
                .StLocS(context)

                .LdLoc(localAttr)
                .LdLocS(context)
                .CallVirt(OnExecutingMethod)
                .GetProperty("Response", context)
                .StLocS(contextResponse)

                .LdLocS(contextResponse)
                .BrFalseS(executingEnd)
                .Nop()

                .LdLocS(contextResponse)
                .StLoc(localResponse)
                .MarkLabel(executingEnd);
        }

        /// <summary>
        /// Emits IL to create a new instance of a <see cref="ServiceCallExecutedContext"/> and call the service call filter attributes
        /// 'OnExecuted' method.
        /// </summary>
        /// <param name="localAttr">A <see cref="ILocal"/> containing a attribute instance.</param>
        /// <param name="localController">A <see cref="ILocal"/> containing a controller instance.</param>
        /// <param name="localServices">A <see cref="ILocal"/> containing a services instance.</param>
        /// <param name="localResponse">A <see cref="ILocal"/> for any response.</param>
        private void EmitExecuted(ILocal localAttr, ILocal localController, ILocal localServices, ILocal localResponse)
        {
            // Create new instance of attribute and call on executed method.
            this.emitter
                .DeclareLocal<ServiceCallExecutedContext>("executedContext", out ILocal context)
                .DeclareLocal<IActionResult>("executedResponse", out ILocal executedResponse)
                .DefineLabel("executedEnd", out ILabel executedEnd)

                .Comment("== Service Call Filter Executed ==")

                .LdLoc(localController)
                .LdLoc(localServices)
                .Newobj(CtorExecuted)
                .StLocS(context)

                .LdLoc(localAttr)
                .LdLocS(context)
                .CallVirt(OnExecutedMethod)

                .GetProperty("Response", context)
                .StLocS(executedResponse)

                .LdLocS(executedResponse)
                .BrFalseS(executedEnd)
                .Nop()

                .LdLocS(executedResponse)
                .StLocS(localResponse)

                .MarkLabel(executedEnd);
        }
    }
}