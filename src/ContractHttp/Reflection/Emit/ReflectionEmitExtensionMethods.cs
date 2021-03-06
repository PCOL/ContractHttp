namespace ContractHttp.Reflection.Emit
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using ContractHttp.Reflection;
    using FluentIL;

    /// <summary>
    /// Reflection emit extension methods.
    /// </summary>
    public static class ReflectionEmitExtensionMethods
    {
        private static readonly MethodInfo GetCustomAttributeTMethod =
            typeof(CustomAttributeExtensions)
                .BuildMethodInfo("GetCustomAttribute")
                .HasParameterTypes(typeof(MemberInfo), typeof(bool))
                .FirstOrDefault();

        private static readonly MethodInfo GetCustomAttributesTMethod =
            typeof(CustomAttributeExtensions)
                .BuildMethodInfo("GetCustomAttributes")
                .IsGenericDefinition()
                .HasParameterTypes(typeof(MemberInfo), typeof(bool))
                .FirstOrDefault();

        private static readonly MethodInfo GetMethodByMetadataToken =
            typeof(FluentIL.ReflectionExtensions)
                .GetMethod("GetMethod", new[] { typeof(Type), typeof(int) });

        private static readonly MethodInfo AnyTMethod =
            typeof(Enumerable)
                .BuildMethodInfo("Any")
                .IsGenericDefinition()
                .HasParameterTypes(typeof(IEnumerable<>))
                .FirstOrDefault();

        /// <summary>
        /// Emits IL to call the 'ToString' method on the object on the top of the evaluation stack.
        /// </summary>
        /// <param name="emitter">THe <see cref="IEmitter"/> to use.</param>
        public static IEmitter EmitToString(this IEmitter emitter)
        {
            MethodInfo toStringMethod = typeof(object).GetMethod("ToString", Type.EmptyTypes);
            return emitter.CallVirt(toStringMethod);
        }

        /// <summary>
        /// Emits optimized IL to load parameters.
        /// </summary>
        /// <param name="emitter">The <see cref="IEmitter"/> to use.</param>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> to emit the parameters for.</param>
        public static IEmitter EmitLoadParameters(this IEmitter emitter, MethodInfo methodInfo)
        {
            return emitter.EmitLoadParameters(methodInfo.GetParameters());
        }

        /// <summary>
        /// Emits optimized IL to load parameters.
        /// </summary>
        /// <param name="emitter">The <see cref="IEmitter"/> to use.</param>
        /// <param name="parameters">The parameters loads to emit.</param>
        public static IEmitter EmitLoadParameters(this IEmitter emitter, ParameterInfo[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                emitter.LdArg(i + 1);
            }

            return emitter;
        }

        /// <summary>
        /// Emits the IL to load an array element.
        /// </summary>
        /// <param name="emitter">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="array">The <see cref="LocalBuilder"/> containing the array.</param>
        /// <param name="index">The index of the array to load.</param>
        public static IEmitter EmitLoadArrayElement(this IEmitter emitter, ILocal array, int index)
        {
            return emitter
                .LdLoc(array)
                .LdcI4(index)
                .LdElemRef();
        }

        /// <summary>
        /// Emits IL to check if the passed in local variable is null or not, executing the emitted body if not.
        /// </summary>
        /// <param name="emitter">The <see cref="IEmitter"/> to use.</param>
        /// <param name="local">The locval variable to check.</param>
        /// <param name="emitBody">A function to emit the IL to be executed if the object is not null.</param>
        /// <param name="emitElse">A function to emit the IL to be executed if the object is null.</param>
        public static IEmitter EmitIfNotNull(this IEmitter emitter, ILocal local, Action<IEmitter> emitBody, Action<IEmitter> emitElse = null)
        {
            return emitter
                .Emit(OpCodes.Ldloc, local)
                .EmitIfNotNull(emitBody, emitElse);
        }

        /// <summary>
        /// Emits IL to check if the object on the top of the evaluation stack is not null, executing the emitted body if not.
        /// </summary>
        /// <param name="emitter">The <see cref="IEmitter"/> to use.</param>
        /// <param name="emitBody">A function to emit the IL to be executed if the object is not null.</param>
        /// <param name="emitElse">A function to emit the IL to be executed if the object is null.</param>
        public static IEmitter EmitIfNotNull(this IEmitter emitter, Action<IEmitter> emitBody, Action<IEmitter> emitElse = null)
        {
            emitter.DefineLabel("endIf", out ILabel endIf);

            if (emitElse != null)
            {
                emitter
                    .DefineLabel("notNull", out ILabel notNull)
                    .Emit(OpCodes.Brtrue, notNull)
                    .Emit(OpCodes.Nop);

                emitElse(emitter);

                emitter
                    .Emit(OpCodes.Br, endIf)
                    .MarkLabel(notNull);

                emitBody(emitter);
            }
            else
            {
                emitter
                    .Emit(OpCodes.Brfalse, endIf)
                    .Emit(OpCodes.Nop);

                emitBody(emitter);
            }

            return emitter
                .MarkLabel(endIf);
        }

        /// <summary>
        /// Emits IL to check if the passed in local variable is not null or empty, executing the emitted body if not.
        /// </summary>
        /// <param name="emitter">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="local">The local variable to check.</param>
        /// <param name="emitBody">A function to emit the IL to be executed if the object is not null.</param>
        /// <param name="emitElse">A function to emit the IL to be executed if the object is null.</param>
        public static IEmitter EmitIfNotNullOrEmpty(this IEmitter emitter, ILocal local, Action<IEmitter> emitBody, Action<IEmitter> emitElse = null)
        {
            var anyMethod = AnyTMethod.MakeGenericMethod(local.LocalType.GetElementType());

            return emitter
                .LdLoc(local)
                .EmitIfNotNull(
                    il =>
                    {
                        il
                            .LdLoc(local)
                            .Call(anyMethod)

                            .DefineLabel("endAny", out ILabel endAny);

                        if (emitElse != null)
                        {
                            il.DefineLabel("some", out ILabel some)
                                .BrTrue(some)
                                .Nop();

                            emitElse(il);

                            il.Br(endAny)
                                .MarkLabel(some);

                            emitBody(il);
                        }
                        else
                        {
                            il.BrFalse(endAny)
                                .Nop();

                            emitBody(il);
                        }

                        il.MarkLabel(endAny);
                    },
                    emitElse);
        }

        /// <summary>
        /// Emits the IL to get a custom attribute from a <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">The attribute type to get.</typeparam>
        /// <param name="emitter">The IL generator.</param>
        /// <param name="customType">The type to get the attributes from.</param>
        /// <param name="localStore">A <see cref="LocalBuilder"/> to store the results in.</param>
        public static IEmitter EmitGetCustomAttribute<T>(this IEmitter emitter, Type customType, ILocal localStore)
        {
            return emitter
                .EmitGetCustomAttribute(customType, typeof(T), localStore);
        }

        /// <summary>
        /// Emits the IL to get a custom attribute from a <see cref="Type"/>.
        /// </summary>
        /// <param name="emitter">The IL generator.</param>
        /// <param name="customType">The type to get the attributes from.</param>
        /// <param name="attributeType">The attribute type to get.</param>
        /// <param name="localStore">A <see cref="LocalBuilder"/> to store the results in.</param>
        public static IEmitter EmitGetCustomAttribute(this IEmitter emitter, Type customType, Type attributeType, ILocal localStore)
        {
            return emitter
                .EmitGetCustomAttribute(customType, attributeType)
                .Emit(OpCodes.Stloc, localStore);
        }

        /// <summary>
        /// Emits the IL to get a custom attribute from a <see cref="Type"/>.
        /// </summary>
        /// <param name="emitter">The IL generator.</param>
        /// <param name="customType">The type to get the attributes from.</param>
        /// <param name="attributeType">The attribute type to get.</param>
        public static IEmitter EmitGetCustomAttribute(this IEmitter emitter, Type customType, Type attributeType)
        {
            return emitter
                .EmitTypeOf(customType)
                .Emit(OpCodes.Ldc_I4_1)
                .Emit(OpCodes.Call, GetCustomAttributeTMethod.MakeGenericMethod(attributeType));
        }

        /// <summary>
        /// Emits the IL to get custom attributes from a <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">The attribute type to get.</typeparam>
        /// <param name="emitter">The IL generator.</param>
        /// <param name="customType">The type to get the attributes from.</param>
        /// <param name="localStore">A <see cref="LocalBuilder"/> to store the results in.</param>
        public static IEmitter EmitGetCustomAttributes<T>(this IEmitter emitter, Type customType, ILocal localStore)
        {
            return emitter
                .EmitGetCustomAttributes(customType, typeof(T), localStore);
        }

        /// <summary>
        /// Emits the IL to get custom attributes from a <see cref="Type"/>.
        /// </summary>
        /// <param name="emitter">The IL generator.</param>
        /// <param name="customType">The type to get the attributes from.</param>
        /// <param name="attributeType">The attribute type to get.</param>
        /// <param name="localStore">A <see cref="LocalBuilder"/> to store the results in.</param>
        public static IEmitter EmitGetCustomAttributes(this IEmitter emitter, Type customType, Type attributeType, ILocal localStore)
        {
            return emitter
                .EmitGetCustomAttributes(customType, attributeType)
                .Emit(OpCodes.Stloc, localStore);
        }

        /// <summary>
        /// Emits the IL to get custom attributes from a <see cref="Type"/>.
        /// </summary>
        /// <param name="emitter">The IL generator.</param>
        /// <param name="customType">The type to get the attributes from.</param>
        /// <param name="attributeType">The attribute type to get.</param>
        public static IEmitter EmitGetCustomAttributes(this IEmitter emitter, Type customType, Type attributeType)
        {
            return emitter
                .EmitTypeOf(customType)
                .Emit(OpCodes.Ldc_I4_1)
                .Emit(OpCodes.Call, GetCustomAttributesTMethod.MakeGenericMethod(attributeType));
        }

        /// <summary>
        /// Emits the IL to get a custom attribute from a method.
        /// </summary>
        /// <typeparam name="T">The attribute type to get.</typeparam>
        /// <param name="emitter">The IL generator.</param>
        /// <param name="methodInfo">The method to get the attributes from.</param>
        public static IEmitter EmitGetCustomAttribute<T>(this IEmitter emitter, MethodInfo methodInfo)
        {
            return emitter
                .EmitGetCustomAttribute(methodInfo, typeof(T));
        }

        /// <summary>
        /// Emits the IL to get a custom attribute from a method.
        /// </summary>
        /// <param name="emitter">The IL generator.</param>
        /// <param name="methodInfo">The method to get the attributes from.</param>
        /// <param name="attributeType">The attribute type to get.</param>
        public static IEmitter EmitGetCustomAttribute(this IEmitter emitter, MethodInfo methodInfo, Type attributeType)
        {
            return emitter
                .EmitTypeOf(methodInfo.DeclaringType)
                .Emit(OpCodes.Ldc_I4, methodInfo.MetadataToken)
                .Emit(OpCodes.Call, GetMethodByMetadataToken)
                .Emit(OpCodes.Ldc_I4_1)
                .Emit(OpCodes.Call, GetCustomAttributeTMethod.MakeGenericMethod(attributeType));
        }

        /// <summary>
        /// Emits the IL to get a custom attribute from a method.
        /// </summary>
        /// <param name="emitter">The IL generator.</param>
        /// <param name="methodInfo">The method to get the attributes from.</param>
        /// <param name="attributeType">The attribute type to get.</param>
        /// <param name="localStore">A <see cref="LocalBuilder"/> to store the results in.</param>
        public static IEmitter EmitGetCustomAttribute(this IEmitter emitter, MethodInfo methodInfo, Type attributeType, ILocal localStore)
        {
            return emitter
                .EmitGetCustomAttribute(methodInfo, attributeType)
                .Emit(OpCodes.Stloc, localStore);
        }

        /// <summary>
        /// Emits the IL to get custom attributes from a method.
        /// </summary>
        /// <typeparam name="T">The attribute type to get.</typeparam>
        /// <param name="emitter">The IL generator.</param>
        /// <param name="methodInfo">The method to get the attributes from.</param>
        public static IEmitter EmitGetCustomAttributes<T>(this IEmitter emitter, MethodInfo methodInfo)
        {
            return emitter.EmitGetCustomAttributes(methodInfo, typeof(T));
        }

        /// <summary>
        /// Emits the IL to get custom attributes from a method.
        /// </summary>
        /// <param name="emitter">The IL generator.</param>
        /// <param name="methodInfo">The method to get the attributes from.</param>
        /// <param name="attributeType">The attribute type to get.</param>
        public static IEmitter EmitGetCustomAttributes(this IEmitter emitter, MethodInfo methodInfo, Type attributeType)
        {
            return emitter
                .EmitTypeOf(methodInfo.DeclaringType)
                .Emit(OpCodes.Ldc_I4, methodInfo.MetadataToken)
                .Emit(OpCodes.Call, GetMethodByMetadataToken)
                .Emit(OpCodes.Ldc_I4_1)
                .Emit(OpCodes.Call, GetCustomAttributesTMethod.MakeGenericMethod(attributeType));
        }

        /// <summary>
        /// Emits the IL to get custom attributes from a method.
        /// </summary>
        /// <param name="emitter">The IL generator.</param>
        /// <param name="methodInfo">The method to get the attributes from.</param>
        /// <param name="attributeType">The attribute type to get.</param>
        /// <param name="localStore">A <see cref="LocalBuilder"/> to store the results in.</param>
        public static IEmitter EmitGetCustomAttributes(this IEmitter emitter, MethodInfo methodInfo, Type attributeType, ILocal localStore)
        {
            return emitter
                .EmitGetCustomAttributes(methodInfo, attributeType)
                .Emit(OpCodes.Stloc, localStore);
        }
    }
}