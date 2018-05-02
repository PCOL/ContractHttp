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
    /// Reflection emit extension methods
    /// </summary>
    public static class ReflectionEmitExtensions
    {
        private static readonly MethodInfo getCustomAttributeTMethod =
            typeof(CustomAttributeExtensions)
                .BuildMethodInfo("GetCustomAttribute")
                .HasParameterTypes(typeof(MemberInfo), typeof(bool))
                .FirstOrDefault();

        private static readonly MethodInfo getCustomAttributesTMethod =
            typeof(CustomAttributeExtensions)
                .BuildMethodInfo("GetCustomAttributes")
                .IsGenericDefinition()
                .HasParameterTypes(typeof(MemberInfo), typeof(bool))
                .FirstOrDefault();

        private static readonly MethodInfo getMethodByMetadataToken =
            typeof(FluentIL.ReflectionExtensions)
                .GetMethod("GetMethod", new[] { typeof(Type), typeof(int) });

        private static readonly MethodInfo anyTMethod =
            typeof(Enumerable)
                .BuildMethodInfo("Any")
                .IsGenericDefinition()
                .HasParameterTypes(typeof(IEnumerable<>))
                .FirstOrDefault();

        /// <summary>
        /// Emits IL to call the 'ToString' method on the object on the top of the evaluation stack.
        /// </summary>
        /// <param name="ilGen">THe <see cref="ILGenerator"/> to use.</param>
        public static IEmitter EmitToString(this IEmitter ilGen)
        {
            MethodInfo toStringMethod = typeof(object).GetMethod("ToString", Type.EmptyTypes);
            return ilGen.CallVirt(toStringMethod);
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
        /// <param name="ilGen">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="array">The <see cref="LocalBuilder"/> containing the array.</param>
        /// <param name="index">The index of the array to load.</param>
        public static IEmitter EmitLoadArrayElement(this IEmitter ilGen, ILocal array, int index)
        {
            return ilGen
                .LdLoc(array)
                .LdcI4(index)
                .LdElemRef();
        }

        /// <summary>
        /// Emits IL to check if the passed in local variable is null or not, executing the emitted body if not.
        /// </summary>
        /// <param name="ilGen">The <see cref="IEmitter"/> to use.</param>
        /// <param name="local">The locval variable to check.</param>
        /// <param name="emitBody">A function to emit the IL to be executed if the object is not null.</param>
        /// <param name="emitElse">A function to emit the IL to be executed if the object is null.</param>
        public static IEmitter EmitIfNotNull(this IEmitter ilGen, ILocal local, Action<IEmitter> emitBody, Action<IEmitter> emitElse = null)
        {
            return ilGen
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
            emitter.DefineLabel(out ILabel endIf);

            if (emitElse != null)
            {
                emitter
                    .DefineLabel(out ILabel notNull)
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
        /// <param name="local">The locval variable to check.</param>
        /// <param name="emitBody">A function to emit the IL to be executed if the object is not null.</param>
        /// <param name="emitElse">A function to emit the IL to be executed if the object is null.</param>
        public static IEmitter EmitIfNotNullOrEmpty(this IEmitter emitter, ILocal local, Action<IEmitter> emitBody, Action<IEmitter> emitElse = null)
        {
            return emitter
                .LdLoc(local)
                .EmitIfNotNull(il =>
                {
                    var anyMethod = anyTMethod.MakeGenericMethod(local.LocalType.GetElementType());
                    il
                        .LdLoc(local)
                        .Call(anyMethod)

                        .DefineLabel(out ILabel endAny);

                    if (emitElse != null)
                    {
                        il.DefineLabel(out ILabel some)
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
        /// Emits the IL to get a custom attribute from a <see cref="Type"/>
        /// </summary>
        /// <typeparam name="T">The attribute type to get</typeparam>
        /// <param name="ilGen">The IL generator</param>
        /// <param name="customType">The type to get the attributes from.</param>
        /// <param name="localStore">A <see cref="LocalBuilder"/> to store the results in.</param>
        public static IEmitter EmitGetCustomAttribute<T>(this IEmitter ilGen, Type customType, ILocal localStore)
        {
            return ilGen
                .EmitGetCustomAttribute(customType, typeof(T), localStore);
        }

        /// <summary>
        /// Emits the IL to get a custom attribute from a <see cref="Type"/>
        /// </summary>
        /// <param name="ilGen">The IL generator</param>
        /// <param name="customType">The type to get the attributes from.</param>
        /// <param name="attributeType">The attribute type to get</param>
        /// <param name="localStore">A <see cref="LocalBuilder"/> to store the results in.</param>
        public static IEmitter EmitGetCustomAttribute(this IEmitter ilGen, Type customType, Type attributeType, ILocal localStore)
        {
            return ilGen
                .EmitGetCustomAttribute(customType, attributeType)
                .Emit(OpCodes.Stloc, localStore);
        }

        /// <summary>
        /// Emits the IL to get a custom attribute from a <see cref="Type"/>
        /// </summary>
        /// <param name="ilGen">The IL generator</param>
        /// <param name="customType">The type to get the attributes from.</param>
        /// <param name="attributeType">The attribute type to get</param>
        public static IEmitter EmitGetCustomAttribute(this IEmitter ilGen, Type customType, Type attributeType)
        {
            return ilGen
                .EmitTypeOf(customType)
                .Emit(OpCodes.Ldc_I4_1)
                .Emit(OpCodes.Call, getCustomAttributeTMethod.MakeGenericMethod(attributeType));
        }

        /// <summary>
        /// Emits the IL to get custom attributes from a <see cref="Type"/>
        /// </summary>
        /// <typeparam name="T">The attribute type to get</typeparam>
        /// <param name="ilGen">The IL generator</param>
        /// <param name="customType">The type to get the attributes from.</param>
        /// <param name="localStore">A <see cref="LocalBuilder"/> to store the results in.</param>
        public static IEmitter EmitGetCustomAttributes<T>(this IEmitter ilGen, Type customType, ILocal localStore)
        {
            return ilGen
                .EmitGetCustomAttributes(customType, typeof(T), localStore);
        }

        /// <summary>
        /// Emits the IL to get custom attributes from a <see cref="Type"/>
        /// </summary>
        /// <param name="ilGen">The IL generator</param>
        /// <param name="customType">The type to get the attributes from.</param>
        /// <param name="attributeType">The attribute type to get</param>
        /// <param name="localStore">A <see cref="LocalBuilder"/> to store the results in.</param>
        public static IEmitter EmitGetCustomAttributes(this IEmitter ilGen, Type customType, Type attributeType, ILocal localStore)
        {
            return ilGen
                .EmitGetCustomAttributes(customType, attributeType)
                .Emit(OpCodes.Stloc, localStore);
        }

        /// <summary>
        /// Emits the IL to get custom attributes from a <see cref="Type"/>
        /// </summary>
        /// <param name="ilGen">The IL generator</param>
        /// <param name="customType">The type to get the attributes from.</param>
        /// <param name="attributeType">The attribute type to get</param>
        public static IEmitter EmitGetCustomAttributes(this IEmitter ilGen, Type customType, Type attributeType)
        {
            return ilGen
                .EmitTypeOf(customType)
                .Emit(OpCodes.Ldc_I4_1)
                .Emit(OpCodes.Call, getCustomAttributesTMethod.MakeGenericMethod(attributeType));
        }

        /// <summary>
        /// Emits the IL to get a custom attribute from a method.
        /// </summary>
        /// <typeparam name="T">The attribute type to get</typeparam>
        /// <param name="ilGen">The IL generator</param>
        /// <param name="methodInfo">The method to get the attributes from.</param>
        public static IEmitter EmitGetCustomAttribute<T>(this IEmitter ilGen, MethodInfo methodInfo)
        {
            return ilGen
                .EmitGetCustomAttribute(methodInfo, typeof(T));
        }

        /// <summary>
        /// Emits the IL to get a custom attribute from a method.
        /// </summary>
        /// <param name="ilGen">The IL generator</param>
        /// <param name="methodInfo">The method to get the attributes from.</param>
        /// <param name="attributeType">The attribute type to get</param>
        public static IEmitter EmitGetCustomAttribute(this IEmitter ilGen, MethodInfo methodInfo, Type attributeType)
        {
            return ilGen
                .EmitTypeOf(methodInfo.DeclaringType)
                .Emit(OpCodes.Ldc_I4, methodInfo.MetadataToken)
                .Emit(OpCodes.Call, getMethodByMetadataToken)
                .Emit(OpCodes.Ldc_I4_1)
                .Emit(OpCodes.Call, getCustomAttributeTMethod.MakeGenericMethod(attributeType));
        }

        /// <summary>
        /// Emits the IL to get a custom attribute from a method.
        /// </summary>
        /// <param name="ilGen">The IL generator</param>
        /// <param name="methodInfo">The method to get the attributes from.</param>
        /// <param name="attributeType">The attribute type to get</param>
        /// <param name="localStore">A <see cref="LocalBuilder"/> to store the results in.</param>
        public static IEmitter EmitGetCustomAttribute(this IEmitter ilGen, MethodInfo methodInfo, Type attributeType, ILocal localStore)
        {
            return ilGen
                .EmitGetCustomAttribute(methodInfo, attributeType)
                .Emit(OpCodes.Stloc, localStore);
        }

        /// <summary>
        /// Emits the IL to get custom attributes from a method.
        /// </summary>
        /// <typeparam name="T">The attribute type to get</typeparam>
        /// <param name="ilGen">The IL generator</param>
        /// <param name="methodInfo">The method to get the attributes from.</param>
        public static IEmitter EmitGetCustomAttributes<T>(this IEmitter ilGen, MethodInfo methodInfo)
        {
            return ilGen.EmitGetCustomAttributes(methodInfo, typeof(T));
        }

        /// <summary>
        /// Emits the IL to get custom attributes from a method.
        /// </summary>
        /// <param name="ilGen">The IL generator</param>
        /// <param name="methodInfo">The method to get the attributes from.</param>
        /// <param name="attributeType">The attribute type to get</param>
        public static IEmitter EmitGetCustomAttributes(this IEmitter ilGen, MethodInfo methodInfo, Type attributeType)
        {
            return ilGen
                .EmitTypeOf(methodInfo.DeclaringType)
                .Emit(OpCodes.Ldc_I4, methodInfo.MetadataToken)
                .Emit(OpCodes.Call, getMethodByMetadataToken)
                .Emit(OpCodes.Ldc_I4_1)
                .Emit(OpCodes.Call, getCustomAttributesTMethod.MakeGenericMethod(attributeType));
        }

        /// <summary>
        /// Emits the IL to get custom attributes from a method.
        /// </summary>
        /// <param name="ilGen">The IL generator</param>
        /// <param name="methodInfo">The method to get the attributes from.</param>
        /// <param name="attributeType">The attribute type to get</param>
        /// <param name="localStore">A <see cref="LocalBuilder"/> to store the results in.</param>
        public static IEmitter EmitGetCustomAttributes(this IEmitter ilGen, MethodInfo methodInfo, Type attributeType, ILocal localStore)
        {
            return ilGen
                .EmitGetCustomAttributes(methodInfo, attributeType)
                .Emit(OpCodes.Stloc, localStore);
        }
    }
}