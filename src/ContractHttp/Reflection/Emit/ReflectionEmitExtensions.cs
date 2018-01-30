namespace ContractHttp.Reflection.Emit
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// System.Reflection.Emit extension methods.
    /// </summary>
    public static class ReflectionEmitExtensions
    {
        private static readonly OpCode[] ConvOpCodes = new OpCode[]
        {
            OpCodes.Nop,
            OpCodes.Nop,
            OpCodes.Nop,
            OpCodes.Conv_I1,
            OpCodes.Conv_I2,
            OpCodes.Conv_I1,
            OpCodes.Conv_U1,
            OpCodes.Conv_I2,
            OpCodes.Conv_U2,
            OpCodes.Conv_I4,
            OpCodes.Conv_U4,
            OpCodes.Conv_I8,
            OpCodes.Conv_U8,
            OpCodes.Conv_R4,
            OpCodes.Conv_R8,
            OpCodes.Nop,
            OpCodes.Nop,
            OpCodes.Nop,
            OpCodes.Nop
        };

        private static readonly OpCode[] LdindOpCodes = new OpCode[]
        {
            OpCodes.Ldobj,
            OpCodes.Ldobj,
            OpCodes.Ldobj,
            OpCodes.Ldind_I1,
            OpCodes.Ldind_I2,
            OpCodes.Ldind_I1,
            OpCodes.Ldind_U1,
            OpCodes.Ldind_I2,
            OpCodes.Ldind_U2,
            OpCodes.Ldind_I4,
            OpCodes.Ldind_U4,
            OpCodes.Ldind_I8,
            OpCodes.Ldind_I8,
            OpCodes.Ldind_R4,
            OpCodes.Ldind_R8,
            OpCodes.Ldobj,
            OpCodes.Ldobj,
            OpCodes.Ldobj,
            OpCodes.Ldobj
        };

        private static readonly OpCode[] StindOpCodes = new OpCode[]
        {
            OpCodes.Stobj,
            OpCodes.Stobj,
            OpCodes.Stobj,
            OpCodes.Stind_I1,
            OpCodes.Stind_I2,
            OpCodes.Stind_I1,
            OpCodes.Stind_I1,
            OpCodes.Stind_I2,
            OpCodes.Stind_I2,
            OpCodes.Stind_I4,
            OpCodes.Stind_I4,
            OpCodes.Stind_I8,
            OpCodes.Stind_I8,
            OpCodes.Stind_R4,
            OpCodes.Stind_R8,
            OpCodes.Stobj,
            OpCodes.Stobj,
            OpCodes.Stobj,
            OpCodes.Stobj
        };

        private static readonly OpCode[] LdcI4OpCodes = new OpCode[]
        {
            OpCodes.Ldc_I4_0,
            OpCodes.Ldc_I4_1,
            OpCodes.Ldc_I4_2,
            OpCodes.Ldc_I4_3,
            OpCodes.Ldc_I4_4,
            OpCodes.Ldc_I4_5,
            OpCodes.Ldc_I4_6,
            OpCodes.Ldc_I4_7,
            OpCodes.Ldc_I4_8
        };

        private static readonly MethodInfo DisposeMethod = typeof(IDisposable).GetMethod("Dispose");

        private static readonly MethodInfo GetMethodFromHandleMethod = typeof(MethodBase).GetMethod("GetMethodFromHandle", new[] { typeof(RuntimeMethodHandle) });

        private static readonly MethodInfo GetTypeFromHandleMethod = typeof(Type).GetMethod("GetTypeFromHandle");

        private static readonly MethodInfo GetFieldFromHandleMethod = typeof(FieldInfo).GetMethod("GetFiledFromHandle", new[] { typeof(RuntimeFieldHandle) });

        private static readonly MethodInfo getCustomAttributeTMethod = typeof(CustomAttributeExtensions).GetMethod("GetCustomAttribute", new[] { typeof(MemberInfo), typeof(bool) });

        private static readonly MethodInfo getCustomAttributesTMethod = typeof(CustomAttributeExtensions).GetMethod("GetCustomAttributes", 1, BindingFlags.Public | BindingFlags.Static, new[] { typeof(MemberInfo), typeof(bool) });

        private static readonly MethodInfo getMethodByMetadataToken = typeof(ReflectionExtensions).GetMethod("GetMethod", new[] { typeof(Type), typeof(int) });

        /// <summary>
        /// Emits optimized IL to load an argument.
        /// </summary>
        /// <param name="methodIL">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="index">The arguement index.</param>
        public static void EmitLdArg(this ILGenerator methodIL, int index)
        {
            if (index == 0)
            {
                methodIL.Emit(OpCodes.Ldarg_1);
            }
            else if (index == 1)
            {
                methodIL.Emit(OpCodes.Ldarg_2);
            }
            else if (index == 2)
            {
                methodIL.Emit(OpCodes.Ldarg_3);
            }
            else
            {
                methodIL.Emit(OpCodes.Ldarg, index + 1);
            }
        }

        /// <summary>
        /// Emit IL to load a constant value onto the evaluation stack.
        /// </summary>
        /// <param name="ilGen">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="value">The value to emit.</param>
        public static void EmitLdc_I4(this ILGenerator ilGen, int value)
        {
            if (value >= 0 && value <= 8)
            {
                ilGen.Emit(LdcI4OpCodes[value]);
            }
            else
            {
                ilGen.Emit(OpCodes.Ldc_I4, value);
            }
        }

        /// <summary>
        /// Emits 'typeof()' IL.
        /// </summary>
        /// <param name="ilGen">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="type">The <see cref="Type"/> to emit the 'typeof()' for.</param>
        public static void EmitTypeOf(this ILGenerator ilGen, Type type)
        {
            if (ilGen != null)
            {
                ilGen.Emit(OpCodes.Ldtoken, type);
                ilGen.Emit(OpCodes.Call, GetTypeFromHandleMethod);
            }
        }

        /// <summary>
        /// Emits IL to get a method.
        /// </summary>
        /// <param name="ilGen">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> to emit.</param>
        public static void EmitMethodInfo(this ILGenerator ilGen, MethodInfo methodInfo)
        {
            if (ilGen != null)
            {
                ilGen.Emit(OpCodes.Ldtoken, methodInfo);
                ilGen.Emit(OpCodes.Call, GetMethodFromHandleMethod);
            }
        }

        /// <summary>
        /// Emits IL to get a field.
        /// </summary>
        /// <param name="ilGen">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="fieldInfo">The <see cref="FieldInfo"/> to emit.</param>
        public static void EmitFieldInfo(this ILGenerator ilGen, FieldInfo fieldInfo)
        {
            if (ilGen != null)
            {
                ilGen.Emit(OpCodes.Ldtoken, fieldInfo);
                ilGen.Emit(OpCodes.Call, GetFieldFromHandleMethod);
            }
        }

        /// <summary>
        /// Emits IL for 'using' pattern.
        /// </summary>
        /// <param name="ilGen">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="disposableObj">The disposable object.</param>
        /// <param name="generateBlock">The code block inside the using block.</param>
        public static void EmitUsing(this ILGenerator ilGen, LocalBuilder disposableObj, Action<ILGenerator> generateBlock)
        {
            if (ilGen != null)
            {
                // Try
                ilGen.BeginExceptionBlock();

                generateBlock(ilGen);

                // Finally
                ilGen.BeginFinallyBlock();

                Label endFinally = ilGen.DefineLabel();

                ilGen.Emit(OpCodes.Ldloc, disposableObj);
                ilGen.Emit(OpCodes.Brfalse, endFinally);

                ilGen.Emit(OpCodes.Ldloc, disposableObj);
                ilGen.Emit(OpCodes.Callvirt, DisposeMethod);
                ilGen.Emit(OpCodes.Nop);

                ilGen.MarkLabel(endFinally);

                // End
                ilGen.EndExceptionBlock();
            }
        }

        /// <summary>
        /// Emits optimized IL to load parameters.
        /// </summary>
        /// <param name="methodIL">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> to emit the parameters for.</param>
        public static void EmitLoadParameters(this ILGenerator methodIL, MethodInfo methodInfo)
        {
            methodIL.EmitLoadParameters(methodInfo.GetParameters());
        }

        /// <summary>
        /// Emits optimized IL to load parameters.
        /// </summary>
        /// <param name="methodIL">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="parameters">The parameters loads to emit.</param>
        public static void EmitLoadParameters(this ILGenerator methodIL, ParameterInfo[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                methodIL.EmitLdArg(i);
            }
        }

        /// <summary>
        /// Emits the IL to allocate and fill an array.
        /// </summary>
        /// <param name="ilGen">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="local">The local to store the array in.</param>
        /// <param name="length">The size of the array.</param>
        /// <param name="action">The action to execute for each index in the array.</param>
        public static void EmitArray(this ILGenerator ilGen, LocalBuilder local, int length, Action<ILGenerator, int> action)
        {
            ArrayBuilder arrayBuilder = new ArrayBuilder(ilGen, local.LocalType, length, local);
            for (int i = 0; i < length; i++)
            {
                arrayBuilder.EmitSet(i, action);
            }
        }

        /// <summary>
        /// Emits the IL to allocate and fill an array.
        /// </summary>
        /// <param name="ilGen">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="arrayType">The <see cref="Type"/> to array to emit.</param>
        /// <param name="local">The local to store the array in.</param>
        /// <param name="length">The size of the array.</param>
        /// <param name="action">The action to execute for each index in the array.</param>
        public static void EmitArray(this ILGenerator ilGen, Type arrayType, LocalBuilder local, int length, Action<ILGenerator, int> action)
        {
            ArrayBuilder arrayBuilder = new ArrayBuilder(ilGen, arrayType, length, local);
            for (int i = 0; i < length; i++)
            {
                arrayBuilder.EmitSet(i, action);
            }
        }

        /// <summary>
        /// Emits the IL to allocate and fill an array.
        /// </summary>
        /// <param name="ilGen">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="local">The local to store the array in.</param>
        /// <param name="localTypes">The local variables to add to the array.</param>
        public static void EmitTypeArray(this ILGenerator ilGen, LocalBuilder local, params LocalBuilder[] localTypes)
        {
            ArrayBuilder arrayBuilder = new ArrayBuilder(ilGen, typeof(Type), localTypes.Length, local);
            for (int i = 0; i < localTypes.Length; i++)
            {
                arrayBuilder.EmitSet(i, (il) => il.Emit(OpCodes.Ldloc, localTypes[i]));
            }
        }

        /// <summary>
        /// Emits the IL to allocate and fill an array.
        /// </summary>
        /// <param name="ilGen">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="local">The local to store the array in.</param>
        /// <param name="types">The types to add to the array.</param>
        public static void EmitTypeArray(this ILGenerator ilGen, LocalBuilder local, params Type[] types)
        {
            ArrayBuilder arrayBuilder = new ArrayBuilder(ilGen, typeof(Type), types.Length, local);
            for (int i = 0; i < types.Length; i++)
            {
                arrayBuilder.EmitSet(i, (il) => il.EmitTypeOf(types[i]));
            }
        }

        /// <summary>
        /// Emits the IL to load an array element.
        /// </summary>
        /// <param name="ilGen">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="array">The <see cref="LocalBuilder"/> containing the array.</param>
        /// <param name="index">The index of the array to load.</param>
        public static void EmitLoadArrayElement(this ILGenerator ilGen, LocalBuilder array, int index)
        {
            ilGen.Emit(OpCodes.Ldloc, array);
            ilGen.EmitLdc_I4(index);
            ilGen.Emit(OpCodes.Ldelem_Ref);
        }

        /// <summary>
        /// Emits IL to convert one type to another.
        /// </summary>
        /// <param name="ilGen">The IL generator to use.</param>
        /// <param name="sourceType">The source type.</param>
        /// <param name="targetType">The destination type.</param>
        /// <param name="isAddress">A value indicating whether or not the convert is for an address.</param>
        public static void EmitConv(this ILGenerator ilGen, Type sourceType, Type targetType, bool isAddress)
        {
            if (sourceType != targetType)
            {
                if (sourceType.IsByRef == true)
                {
                    Type elementType = sourceType.GetElementType();
                    ilGen.EmitLdInd(elementType);
                    ilGen.EmitConv(elementType, targetType, isAddress);
                }
                else if (targetType.IsValueType == true)
                {
                    if (sourceType.IsValueType == true)
                    {
                        OpCode opCode = ConvOpCodes[(int)Type.GetTypeCode(targetType)];
                    }
                    else
                    {
                        ilGen.Emit(OpCodes.Unbox, targetType);
                        if (isAddress == false)
                        {
                            ilGen.EmitLdInd(targetType);
                        }
                    }
                }
                else if (targetType.IsAssignableFrom(sourceType) == true)
                {
                    if (sourceType.IsValueType == true)
                    {
                        if (isAddress == true)
                        {
                            ilGen.EmitLdInd(sourceType);
                        }

                        ilGen.Emit(OpCodes.Box, sourceType);
                    }
                }
                else if (targetType.IsGenericParameter == true)
                {
                    ilGen.Emit(OpCodes.Unbox_Any, targetType);
                }
                else
                {
                    ilGen.Emit(OpCodes.Castclass, targetType);
                }
            }
        }

        /// <summary>
        /// Emits the IL to load a value onto the evaluation stack.
        /// </summary>
        /// <param name="ilGen">The IL generator.</param>
        /// <param name="type">The type to load.</param>
        public static void EmitLdInd(this ILGenerator ilGen, Type type)
        {
            OpCode opCode = LdindOpCodes[(int)Type.GetTypeCode(type)];
            if (opCode.Equals(OpCodes.Ldobj) == true)
            {
                ilGen.Emit(opCode, type);
            }
            else
            {
                ilGen.Emit(opCode);
            }
        }

        /// <summary>
        /// Emits the IL to store the value from the top of the evaluation stack.
        /// </summary>
        /// <param name="ilGen">The IL generator.</param>
        /// <param name="type">The type to store.</param>
        public static void EmitStInd(this ILGenerator ilGen, Type type)
        {
            OpCode opCode = StindOpCodes[(int)Type.GetTypeCode(type)];
            if (opCode.Equals(OpCodes.Stobj) == true)
            {
                ilGen.Emit(opCode, type);
            }
            else
            {
                ilGen.Emit(opCode);
            }
        }

        /// <summary>
        /// Emits IL to perform a for loop over an array.
        /// </summary>
        /// <param name="ilGen">An IL generator.</param>
        /// <param name="local">The local variable holding the array.</param>
        /// <param name="action">An action to allow the injecting of the loop code.</param>
        public static void EmitFor(this ILGenerator ilGen, LocalBuilder local, Action<LocalBuilder, LocalBuilder> action)
        {
            ilGen.EmitFor(
                local,
                (index, item, loop) =>
                {
                    action(index, item);
                });
        }

        /// <summary>
        /// Emits IL to perform a for loop over an array.
        /// </summary>
        /// <param name="ilGen">An IL generator.</param>
        /// <param name="local">The local variable holding the array.</param>
        /// <param name="action">An action to allow the injecting of the loop code.</param>
        public static void EmitFor(this ILGenerator ilGen, LocalBuilder local, Action<LocalBuilder, LocalBuilder, Label> action)
        {
            Label beginLoop = ilGen.DefineLabel();
            Label loopCheck = ilGen.DefineLabel();

            LocalBuilder index = ilGen.DeclareLocal(typeof(int));
            LocalBuilder item = ilGen.DeclareLocal(local.LocalType.GetElementType());

            ilGen.Emit(OpCodes.Ldc_I4_0);
            ilGen.Emit(OpCodes.Stloc, index);
            ilGen.Emit(OpCodes.Br, loopCheck);
            ilGen.MarkLabel(beginLoop);

            ilGen.Emit(OpCodes.Ldloc, local);
            ilGen.Emit(OpCodes.Ldloc, index);
            ilGen.Emit(OpCodes.Ldelem_Ref);
            ilGen.Emit(OpCodes.Stloc, item);
            ilGen.Emit(OpCodes.Nop);

            action(index, item, loopCheck);

            ilGen.Emit(OpCodes.Nop);
            ilGen.Emit(OpCodes.Ldloc, index);
            ilGen.Emit(OpCodes.Ldc_I4_1);
            ilGen.Emit(OpCodes.Add);
            ilGen.Emit(OpCodes.Stloc, index);

            ilGen.MarkLabel(loopCheck);
            ilGen.Emit(OpCodes.Ldloc, index);
            ilGen.Emit(OpCodes.Ldloc, local);
            ilGen.Emit(OpCodes.Ldlen);
            ilGen.Emit(OpCodes.Conv_I4);
            ilGen.Emit(OpCodes.Blt, beginLoop);
        }

        /// <summary>
        /// Emits IL to perform a for loop over an array.
        /// </summary>
        /// <param name="ilGen">An IL generator.</param>
        /// <param name="local">The local variable holding the array.</param>
        /// <param name="action">An action to allow the injecting of the loop code.</param>
        public static void EmitForEach(this ILGenerator ilGen, LocalBuilder local, Action<LocalBuilder> action)
        {
            if (local.LocalType.IsArray == true)
            {
                ilGen.EmitFor(
                    local,
                    (index, item, loop) =>
                    {
                        action(item);
                    });

                return;
            }

            if (local.LocalType.IsGenericType == false ||
                typeof(IEnumerable<>).MakeGenericType(local.LocalType.GetGenericArguments()).IsAssignableFrom(local.LocalType) == false)
            {
                throw new InvalidOperationException("Not a enumerable type");
            }

            Type enumerableType = local.LocalType.GetGenericArguments()[0];
            Type enumeratorType = typeof(IEnumerator<>).MakeGenericType(enumerableType);

            MethodInfo getEnumerator = typeof(IEnumerable<>).MakeGenericType(enumerableType).GetMethod("GetEnumerator");
            MethodInfo getCurrent = enumeratorType.GetMethod("get_Current");
            MethodInfo moveNext = typeof(IEnumerator).GetMethod("MoveNext");
            MethodInfo dispose = typeof(IDisposable).GetMethod("Dispose");

            LocalBuilder localEnumerator = ilGen.DeclareLocal(enumeratorType);
            LocalBuilder localItem = ilGen.DeclareLocal(enumerableType);

            Label loopStart = ilGen.DefineLabel();
            Label loopCheck = ilGen.DefineLabel();
            Label loopEnd = ilGen.DefineLabel();
            Label endFinally = ilGen.DefineLabel();

            ilGen.Emit(OpCodes.Ldloc, local);
            ilGen.Emit(OpCodes.Callvirt, getEnumerator);
            ilGen.Emit(OpCodes.Stloc, localEnumerator);

            // Try
            ilGen.BeginExceptionBlock();

            ilGen.Emit(OpCodes.Br, loopCheck);
            ilGen.MarkLabel(loopStart);
            ilGen.Emit(OpCodes.Ldloc, localEnumerator);
            ilGen.Emit(OpCodes.Callvirt, getCurrent);
            ilGen.Emit(OpCodes.Stloc, localItem);
            ilGen.Emit(OpCodes.Nop);

            action(localItem);

            ilGen.Emit(OpCodes.Nop);
            ilGen.Emit(OpCodes.Nop);

            ilGen.MarkLabel(loopCheck);
            ilGen.Emit(OpCodes.Ldloc, localEnumerator);
            ilGen.Emit(OpCodes.Callvirt, moveNext);
            ilGen.Emit(OpCodes.Brtrue, loopStart);

            ilGen.Emit(OpCodes.Leave, loopEnd);

            // Finally
            ilGen.BeginFinallyBlock();

            ilGen.Emit(OpCodes.Ldloc, localEnumerator);
            ilGen.Emit(OpCodes.Brfalse, endFinally);

            ilGen.Emit(OpCodes.Ldloc, localEnumerator);
            ilGen.Emit(OpCodes.Callvirt, dispose);
            ilGen.Emit(OpCodes.Nop);

            ilGen.MarkLabel(endFinally);
            ilGen.EndExceptionBlock();

            ilGen.MarkLabel(loopEnd);
        }

        /// <summary>
        /// Throws an exception.
        /// </summary>
        /// <typeparam name="T">The type of exception to throw.</typeparam>
        /// <param name="ilGen">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="message">The exception message.</param>
        public static void ThrowException<T>(this ILGenerator ilGen, string message)
            where T : Exception
        {
            ConstructorInfo ctor = typeof(T).GetConstructor(new Type[] { typeof(string) });
            if (ctor == null)
            {
                throw new ArgumentException("Type T does not have a public constructor that takes a string argument");
            }

            ilGen.Emit(OpCodes.Ldstr, message);
            ilGen.Emit(OpCodes.Newobj, ctor);
            ilGen.Emit(OpCodes.Throw);
        }

        /// <summary>
        /// Emits IL to call writeline with the object of the top of the evaluation stack.
        /// </summary>
        /// <param name="ilGen">THe <see cref="ILGenerator"/> to use.</param>
        public static void EmitWriteLine(this ILGenerator ilGen)
        {
            MethodInfo writeLineMethod = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) });

            ilGen.Emit(OpCodes.Call, writeLineMethod);
        }

        /// <summary>
        /// Emits IL to call <c>ToString()</c> on the object at the top of the evaluation stack.
        /// </summary>
        /// <param name="ilGen">THe <see cref="ILGenerator"/> to use.</param>
        public static void EmitToString(this ILGenerator ilGen)
        {
            MethodInfo toStringMethod = typeof(object).GetMethod("ToString", Type.EmptyTypes);
            ilGen.Emit(OpCodes.Callvirt, toStringMethod);
        }

        /// <summary>
        /// Emits IL to call the static Format method on the <see cref="string"/> object.
        /// </summary>
        /// <param name="ilGen">The <see cref="ILGenerator"/> to use.</param>
        public static void EmitStringFormat(this ILGenerator ilGen)
        {
            MethodInfo stringFormatMethod = typeof(string).GetMethodWithParameters("Format", BindingFlags.Public | BindingFlags.Static, new Type[] { typeof(string), typeof(object[]) });
            ilGen.Emit(OpCodes.Call, stringFormatMethod);
        }

        /// <summary>
        /// Emits IL to call the static Format method on the <see cref="string"/> object.
        /// </summary>
        /// <param name="ilGen">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="format">The format to use.</param>
        /// <param name="locals">An array of <see cref="LocalBuilder"/> to use.</param>
        public static void EmitStringFormat(this ILGenerator ilGen, string format, params LocalBuilder[] locals)
        {
            MethodInfo stringFormatMethod = typeof(string).GetMethodWithParameters("Format", BindingFlags.Public | BindingFlags.Static, new Type[] { typeof(string), typeof(object[]) });
            LocalBuilder localArray = ilGen.DeclareLocal(typeof(object));
            ilGen.EmitArray(
                localArray,
                locals.Length,
                (il, index) =>
                {
                    ilGen.Emit(OpCodes.Ldloc, locals[index]);
                    if (locals[index].LocalType.IsValueType == true)
                    {
                        ilGen.Emit(OpCodes.Box, locals[index].LocalType);
                    }
                });

            ilGen.Emit(OpCodes.Ldstr, format);
            ilGen.Emit(OpCodes.Ldloc, localArray);
            ilGen.Emit(OpCodes.Call, stringFormatMethod);
        }

        /// <summary>
        /// Emits IL to check if the passed in local variable is null or not, executing the emitted body if not.
        /// </summary>
        /// <param name="ilGen">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="local">The locval variable to check.</param>
        /// <param name="emitBody">A function to emit the IL to be executed if the object is not null.</param>
        /// <param name="emitElse">A function to emit the IL to be executed if the object is null.</param>
        public static void EmitIfNotNull(this ILGenerator ilGen, LocalBuilder local, Action<ILGenerator> emitBody, Action<ILGenerator> emitElse = null)
        {
            ilGen.Emit(OpCodes.Ldloc, local);
            ilGen.EmitIfNotNull(emitBody, emitElse);
        }

        /// <summary>
        /// Emits IL to check if the object on the top of the evaluation stack is not null, executing the emitted body if not.
        /// </summary>
        /// <param name="ilGen">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="emitBody">A function to emit the IL to be executed if the object is not null.</param>
        /// <param name="emitElse">A function to emit the IL to be executed if the object is null.</param>
        public static void EmitIfNotNull(this ILGenerator ilGen, Action<ILGenerator> emitBody, Action<ILGenerator> emitElse = null)
        {
            Label endIf = ilGen.DefineLabel();

            if (emitElse != null)
            {
                Label notNull = ilGen.DefineLabel();
                ilGen.Emit(OpCodes.Brtrue, notNull);
                ilGen.Emit(OpCodes.Nop);
                emitElse(ilGen);
                ilGen.Emit(OpCodes.Br, endIf);
                ilGen.MarkLabel(notNull);
                emitBody(ilGen);
            }
            else
            {
                ilGen.Emit(OpCodes.Brfalse, endIf);
                ilGen.Emit(OpCodes.Nop);
                emitBody(ilGen);
            }

            ilGen.MarkLabel(endIf);
        }

        /// <summary>
        /// Emits IL to check if the passed in local variable is not null or empty, executing the emitted body if not.
        /// </summary>
        /// <param name="ilGen">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="local">The locval variable to check.</param>
        /// <param name="emitBody">A function to emit the IL to be executed if the object is not null.</param>
        /// <param name="emitElse">A function to emit the IL to be executed if the object is null.</param>
        public static void EmitIfNotNullOrEmpty(this ILGenerator ilGen, LocalBuilder local, Action<ILGenerator> emitBody, Action<ILGenerator> emitElse = null)
        {
            var type = typeof(IEnumerable<>).MakeGenericType(local.LocalType.GetGenericArguments());
            var anyMethod = typeof(Enumerable).GetMethod("Any", 1, BindingFlags.Public | BindingFlags.Static, new[] { type }).MakeGenericMethod(local.LocalType.GetGenericArguments());

            ilGen.Emit(OpCodes.Ldloc, local);
            ilGen.EmitIfNotNull(
                (il) =>
                {
                    il.Emit(OpCodes.Ldloc, local);
                    il.Emit(OpCodes.Call, anyMethod);

                    Label endAny = il.DefineLabel();
                    if (emitElse != null)
                    {
                        Label some = il.DefineLabel();
                        il.Emit(OpCodes.Brtrue, some);
                        il.Emit(OpCodes.Nop);
                        emitElse(il);
                        il.Emit(OpCodes.Br, endAny);
                        il.MarkLabel(some);
                        emitBody(il);
                    }
                    else
                    {
                        il.Emit(OpCodes.Brfalse, endAny);
                        il.Emit(OpCodes.Nop);
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
        public static void EmitGetCustomAttribute<T>(this ILGenerator ilGen, Type customType, LocalBuilder localStore)
        {
            ilGen.EmitGetCustomAttribute(customType, typeof(T), localStore);
        }

        /// <summary>
        /// Emits the IL to get a custom attribute from a <see cref="Type"/>
        /// </summary>
        /// <param name="ilGen">The IL generator</param>
        /// <param name="customType">The type to get the attributes from.</param>
        /// <param name="attributeType">The attribute type to get</param>
        /// <param name="localStore">A <see cref="LocalBuilder"/> to store the results in.</param>
        public static void EmitGetCustomAttribute(this ILGenerator ilGen, Type customType, Type attributeType, LocalBuilder localStore)
        {
            ilGen.EmitGetCustomAttribute(customType, attributeType);
            ilGen.Emit(OpCodes.Stloc, localStore);
        }

        /// <summary>
        /// Emits the IL to get a custom attribute from a <see cref="Type"/>
        /// </summary>
        /// <param name="ilGen">The IL generator</param>
        /// <param name="customType">The type to get the attributes from.</param>
        /// <param name="attributeType">The attribute type to get</param>
        public static void EmitGetCustomAttribute(this ILGenerator ilGen, Type customType, Type attributeType)
        {
            ilGen.EmitTypeOf(customType);
            ilGen.Emit(OpCodes.Ldc_I4_1);
            ilGen.Emit(OpCodes.Call, getCustomAttributeTMethod.MakeGenericMethod(attributeType));
        }

        /// <summary>
        /// Emits the IL to get custom attributes from a <see cref="Type"/>
        /// </summary>
        /// <typeparam name="T">The attribute type to get</typeparam>
        /// <param name="ilGen">The IL generator</param>
        /// <param name="customType">The type to get the attributes from.</param>
        /// <param name="localStore">A <see cref="LocalBuilder"/> to store the results in.</param>
        public static void EmitGetCustomAttributes<T>(this ILGenerator ilGen, Type customType, LocalBuilder localStore)
        {
            ilGen.EmitGetCustomAttributes(customType, typeof(T), localStore);
        }

        /// <summary>
        /// Emits the IL to get custom attributes from a <see cref="Type"/>
        /// </summary>
        /// <param name="ilGen">The IL generator</param>
        /// <param name="customType">The type to get the attributes from.</param>
        /// <param name="attributeType">The attribute type to get</param>
        /// <param name="localStore">A <see cref="LocalBuilder"/> to store the results in.</param>
        public static void EmitGetCustomAttributes(this ILGenerator ilGen, Type customType, Type attributeType, LocalBuilder localStore)
        {
            ilGen.EmitGetCustomAttributes(customType, attributeType);
            ilGen.Emit(OpCodes.Stloc, localStore);
        }

        /// <summary>
        /// Emits the IL to get custom attributes from a <see cref="Type"/>
        /// </summary>
        /// <param name="ilGen">The IL generator</param>
        /// <param name="customType">The type to get the attributes from.</param>
        /// <param name="attributeType">The attribute type to get</param>
        public static void EmitGetCustomAttributes(this ILGenerator ilGen, Type customType, Type attributeType)
        {
            ilGen.EmitTypeOf(customType);
            ilGen.Emit(OpCodes.Ldc_I4_1);
            ilGen.Emit(OpCodes.Call, getCustomAttributesTMethod.MakeGenericMethod(attributeType));
        }

        /// <summary>
        /// Emits the IL to get a custom attribute from a method.
        /// </summary>
        /// <typeparam name="T">The attribute type to get</typeparam>
        /// <param name="ilGen">The IL generator</param>
        /// <param name="methodInfo">The method to get the attributes from.</param>
        public static void EmitGetCustomAttribute<T>(this ILGenerator ilGen, MethodInfo methodInfo)
        {
            ilGen.EmitGetCustomAttribute(methodInfo, typeof(T));
        }

        /// <summary>
        /// Emits the IL to get a custom attribute from a method.
        /// </summary>
        /// <param name="ilGen">The IL generator</param>
        /// <param name="methodInfo">The method to get the attributes from.</param>
        /// <param name="attributeType">The attribute type to get</param>
        public static void EmitGetCustomAttribute(this ILGenerator ilGen, MethodInfo methodInfo, Type attributeType)
        {
            ilGen.EmitTypeOf(methodInfo.DeclaringType);
            ilGen.Emit(OpCodes.Ldc_I4, methodInfo.MetadataToken);
            ilGen.Emit(OpCodes.Call, getMethodByMetadataToken);
            ilGen.Emit(OpCodes.Ldc_I4_1);
            ilGen.Emit(OpCodes.Call, getCustomAttributeTMethod.MakeGenericMethod(attributeType));
        }

        /// <summary>
        /// Emits the IL to get a custom attribute from a method.
        /// </summary>
        /// <param name="ilGen">The IL generator</param>
        /// <param name="methodInfo">The method to get the attributes from.</param>
        /// <param name="attributeType">The attribute type to get</param>
        /// <param name="localStore">A <see cref="LocalBuilder"/> to store the results in.</param>
        public static void EmitGetCustomAttribute(this ILGenerator ilGen, MethodInfo methodInfo, Type attributeType, LocalBuilder localStore)
        {
            ilGen.EmitGetCustomAttribute(methodInfo, attributeType);
            ilGen.Emit(OpCodes.Stloc, localStore);
        }

        /// <summary>
        /// Emits the IL to get custom attributes from a method.
        /// </summary>
        /// <typeparam name="T">The attribute type to get</typeparam>
        /// <param name="ilGen">The IL generator</param>
        /// <param name="methodInfo">The method to get the attributes from.</param>
        public static void EmitGetCustomAttributes<T>(this ILGenerator ilGen, MethodInfo methodInfo)
        {
            ilGen.EmitGetCustomAttributes(methodInfo, typeof(T));
        }

        /// <summary>
        /// Emits the IL to get custom attributes from a method.
        /// </summary>
        /// <param name="ilGen">The IL generator</param>
        /// <param name="methodInfo">The method to get the attributes from.</param>
        /// <param name="attributeType">The attribute type to get</param>
        public static void EmitGetCustomAttributes(this ILGenerator ilGen, MethodInfo methodInfo, Type attributeType)
        {
            ilGen.EmitTypeOf(methodInfo.DeclaringType);
            ilGen.Emit(OpCodes.Ldc_I4, methodInfo.MetadataToken);
            ilGen.Emit(OpCodes.Call, getMethodByMetadataToken);
            ilGen.Emit(OpCodes.Ldc_I4_1);
            ilGen.Emit(OpCodes.Call, getCustomAttributesTMethod.MakeGenericMethod(attributeType));
        }

        /// <summary>
        /// Emits the IL to get custom attributes from a method.
        /// </summary>
        /// <param name="ilGen">The IL generator</param>
        /// <param name="methodInfo">The method to get the attributes from.</param>
        /// <param name="attributeType">The attribute type to get</param>
        /// <param name="localStore">A <see cref="LocalBuilder"/> to store the results in.</param>
        public static void EmitGetCustomAttributes(this ILGenerator ilGen, MethodInfo methodInfo, Type attributeType, LocalBuilder localStore)
        {
            ilGen.EmitGetCustomAttributes(methodInfo, attributeType);
            ilGen.Emit(OpCodes.Stloc, localStore);
        }
    }
}
