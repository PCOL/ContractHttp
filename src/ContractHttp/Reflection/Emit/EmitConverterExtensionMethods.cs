namespace ContractHttp.Reflection.Emit
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using FluentIL;

    /// <summary>
    /// Emit converter extension methods.
    /// </summary>
    public static class EmitConverterExtensionMethods
    {
        /// <summary>
        /// Emits a type converter.
        /// </summary>
        /// <param name="methodIL">An emitter.</param>
        /// <param name="localFrom">The type to convert from.</param>
        /// <param name="toType">The type to cponvert to.</param>
        /// <returns>The emitter.</returns>
        public static IEmitter EmitConverter(this IEmitter methodIL, ILocal localFrom, Type toType)
        {
            return methodIL
                .Emit(OpCodes.Ldloc_S, localFrom)
                .EmitConverter(localFrom.LocalType, toType);
        }

        /// <summary>
        /// Emits a type converter.
        /// </summary>
        /// <param name="methodIL">An emitter.</param>
        /// <param name="fromType">The type to convert from.`</param>
        /// <param name="toType">The type to convert to.</param>
        /// <returns>The emitter.</returns>
        public static IEmitter EmitConverter(this IEmitter methodIL, Type fromType, Type toType)
        {
            if (fromType == toType)
            {
                return methodIL;
            }

            if (toType == typeof(string))
            {
                if (fromType == typeof(byte[]))
                {
                    var toBase64Method = typeof(Convert).GetMethod("ToBase64String", new[] { typeof(byte[]) });
                    return methodIL.Call(toBase64Method);
                }

                var toStringMethod = typeof(Convert).GetMethod("ToString", new[] { fromType });
                return methodIL.Call(toStringMethod);
            }

            if (toType == typeof(byte[]))
            {
                if (fromType == typeof(string))
                {
                    var fromBase64Method = typeof(Convert).GetMethod("FromBase64String", new[] { typeof(string) });
                    return methodIL.Call(fromBase64Method);
                }

                methodIL.ThrowException(typeof(NotSupportedException));
                return methodIL;
            }
            else if (toType == typeof(short))
            {
                var toMethod = typeof(Convert).GetMethod("ToInt16", new[] { fromType });
                return methodIL.Call(toMethod);
            }
            else if (toType == typeof(int))
            {
                var toMethod = typeof(Convert).GetMethod("ToInt32", new[] { fromType });
                return methodIL.Call(toMethod);
            }
            else if (toType == typeof(long))
            {
                var toMethod = typeof(Convert).GetMethod("ToInt64", new[] { fromType });
                return methodIL.Call(toMethod);
            }
            else if (toType == typeof(float))
            {
                var toMethod = typeof(Convert).GetMethod("ToFloat", new[] { fromType });
                return methodIL.Call(toMethod);
            }
            else if (toType == typeof(double))
            {
                var toMethod = typeof(Convert).GetMethod("ToDouble", new[] { fromType });
                return methodIL.Call(toMethod);
            }
            else if (toType == typeof(bool))
            {
                var toMethod = typeof(Convert).GetMethod("ToBoolean", new[] { fromType });
                return methodIL.Call(toMethod);
            }
            else if (toType == typeof(DateTime))
            {
                var toMethod = typeof(Convert).GetMethod("ToDateTime", new[] { fromType });
                return methodIL.Call(toMethod);
            }

            methodIL.ThrowException(typeof(NotSupportedException));
            return methodIL;
        }

        /// <summary>
        /// Binds a type to a model type.
        /// </summary>
        /// <param name="methodIL">An emitter.</param>
        /// <param name="localFrom">A local containing the type to bind from.</param>
        /// <param name="localTo">A local containing the type to bind to.</param>
        /// <returns>The emitter.</returns>
        public static IEmitter EmitModelBinder(this IEmitter methodIL, ILocal localFrom, ILocal localTo)
        {
            MethodInfo getObjectMethod = typeof(Reflection.Binder).GetMethod("GetObject", Type.EmptyTypes).MakeGenericMethod(localTo.LocalType);
            ConstructorInfo binderCtor = typeof(Reflection.Binder).GetConstructor(new[] { typeof(object) });

            return methodIL
                .DeclareLocal<Reflection.Binder>(out ILocal localBinder)

            // Create binder instance.
                .LdLocS(localFrom)
                .Newobj(binderCtor)
                .StLocS(localBinder)

                .LdLocS(localBinder)
                .CallVirt(getObjectMethod)
                .StLocS(localTo);
        }
    }
}