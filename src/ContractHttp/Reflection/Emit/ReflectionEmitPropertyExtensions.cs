namespace ContractHttp.Reflection.Emit
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// Contains Reflection emit property extension methods.
    /// </summary>
    public static class ReflectionEmitPropertyExtensions
    {
        /// <summary>
        /// Emits a static property.
        /// </summary>
        /// <typeparam name="T">The type of the property to emit.</typeparam>
        /// <param name="typeBuilder">The type builder.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="getImplementation">The action to call to implement the get method.</param>
        /// <param name="setImplementation">The action to call to implement the set method.</param>
        public static void EmitStaticProperty<T>(this TypeBuilder typeBuilder, string propertyName, Action<ILGenerator> getImplementation, Action<ILGenerator> setImplementation)
        {
            typeBuilder.EmitProperty<T>(
                propertyName,
                CallingConventions.Standard,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                getImplementation,
                setImplementation);
        }

        /// <summary>
        /// Emits a property
        /// </summary>
        /// <typeparam name="T">The type of the property to emit.</typeparam>
        /// <param name="typeBuilder">The type builder.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="getImplementation">The action to call to implement the get method.</param>
        /// <param name="setImplementation">The action to call to implement the set method.</param>
        public static void EmitProperty<T>(this TypeBuilder typeBuilder, string propertyName, Action<ILGenerator> getImplementation, Action<ILGenerator> setImplementation)
        {
            typeBuilder.EmitProperty<T>(
                propertyName,
                CallingConventions.HasThis,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                getImplementation,
                setImplementation);
        }

        /// <summary>
        /// Emits a property
        /// </summary>
        /// <typeparam name="T">The type of the property to emit.</typeparam>
        /// <param name="typeBuilder">The type builder.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="callingConvention">The calling convention.</param>
        /// <param name="methodAttributes">The method attributes.</param>
        /// <param name="getImplementation">The action to call to implement the get method.</param>
        /// <param name="setImplementation">The action to call to implement the set method.</param>
        public static void EmitProperty<T>(this TypeBuilder typeBuilder, string propertyName, CallingConventions callingConvention, MethodAttributes methodAttributes, Action<ILGenerator> getImplementation, Action<ILGenerator> setImplementation)
        {
            typeBuilder.EmitProperty(
                typeof(T),
                propertyName,
                callingConvention,
                methodAttributes,
                PropertyAttributes.None,
                getImplementation,
                setImplementation);
        }

        /// <summary>
        /// Emits a property
        /// </summary>
        /// <param name="typeBuilder">The type builder.</param>
        /// <param name="propertyType">The type of the property.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="callingConvention">The calling convention.</param>
        /// <param name="methodAttributes">The method attributes.</param>
        /// <param name="propertyAttributes">The property attributes.</param>
        /// <param name="getImplementation">The action to call to implement the get method.</param>
        /// <param name="setImplementation">The action to call to implement the set method.</param>
        public static void EmitProperty(this TypeBuilder typeBuilder, Type propertyType, string propertyName, CallingConventions callingConvention, MethodAttributes methodAttributes, PropertyAttributes propertyAttributes, Action<ILGenerator> getImplementation, Action<ILGenerator> setImplementation)
        {
            MethodBuilder getPropertyMethod = null;
            if (getImplementation != null)
            {
                getPropertyMethod = typeBuilder.DefineMethod(
                    string.Format("get_{0}", propertyName),
                    methodAttributes,
                    callingConvention,
                    propertyType,
                    Type.EmptyTypes);

                getImplementation(getPropertyMethod.GetILGenerator());
            }

            MethodBuilder setPropertyMethod = null;
            if (setImplementation != null)
            {
                setPropertyMethod = typeBuilder.DefineMethod(
                    string.Format("set_{0}", propertyName),
                    methodAttributes,
                    callingConvention,
                    propertyType,
                    Type.EmptyTypes);

                setImplementation(setPropertyMethod.GetILGenerator());
            }

            PropertyBuilder propertyProxiedObject = typeBuilder.DefineProperty(propertyName, PropertyAttributes.None, propertyType, Type.EmptyTypes);
            if (getPropertyMethod != null)
            {
                propertyProxiedObject.SetGetMethod(getPropertyMethod);
            }

            if (setPropertyMethod != null)
            {
                propertyProxiedObject.SetSetMethod(setPropertyMethod);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ilGen"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static ILGenerator EmitGetProperty<T>(this ILGenerator ilGen, string propertyName)
        {
            var property = typeof(T).GetProperty(propertyName);
            ilGen.Emit(OpCodes.Callvirt, property.GetGetMethod());
            return ilGen;
        }

        /// <summary>
        /// Emits IL to load the contents of a property onto the evaluation stack.
        /// </summary>
        /// <param name="ilGen">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="field">The <see cref="FieldBuilder"/> that contains the property to read.</param>
        public static void EmitGetProperty(this ILGenerator ilGen, string propertyName, FieldBuilder field)
        {
            MethodInfo getMethod = field.FieldType.GetMethod(string.Format("get_{0}", propertyName));
            ilGen.Emit(OpCodes.Ldfld, field);
            ilGen.Emit(OpCodes.Callvirt, getMethod);
        }

        /// <summary>
        /// Emits IL to load the contents of a property onto the evaluation stack.
        /// </summary>
        /// <param name="ilGen">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="local">The <see cref="LocalBuilder"/> that contains the property to read.</param>
        public static void EmitGetProperty(this ILGenerator ilGen, string propertyName, LocalBuilder local)
        {
            var property = local.LocalType.GetProperty(propertyName);
            ilGen.Emit(OpCodes.Ldloc, local);
            ilGen.Emit(OpCodes.Callvirt, property.GetGetMethod());
        }

        /// <summary>
        /// Emits IL to pass the value on the top of set evaluation stack to a property set method.
        /// </summary>
        /// <param name="ilGen">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="local">The <see cref="LocalBuilder"/> that contains the property to set.</param>
        public static void EmitSetProperty(this ILGenerator ilGen, string propertyName, LocalBuilder local)
        {
            var property = local.LocalType.GetProperty(propertyName);
            ilGen.Emit(OpCodes.Ldloc, local);
            ilGen.Emit(OpCodes.Callvirt, property.GetSetMethod());
        }
    }
}
