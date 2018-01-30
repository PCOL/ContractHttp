namespace ContractHttp.Reflection.Emit
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// Contains <see cref="TypeFactory"/> extension methods.
    /// </summary>
    public static class TypeFactoryExtension
    {
        private static readonly MethodInfo GetTypeMethod = typeof(TypeFactory).GetMethodWithParameters("GetType", BindingFlags.Static | BindingFlags.Public, new Type[] { typeof(string), typeof(bool) });

        /// <summary>
        /// Emits IL to load the type for a given type name onto the evaluation stack.
        /// </summary>
        /// <param name="ilGen">The <see cref="ILGenerator"/> to use.</param>
        /// <param name="typeName">The <see cref="LocalBuilder"/> containing the type name.</param>
        /// <param name="dynamicOnly">A value indicating whether or not to only check for dynamically generated types.</param>
        public static void EmitGetType(this ILGenerator ilGen, LocalBuilder typeName, bool dynamicOnly = false)
        {
            ilGen.Emit(OpCodes.Ldloc_S, typeName);
            ilGen.Emit(dynamicOnly == false ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1);
            ilGen.Emit(OpCodes.Call, GetTypeMethod);
        }
    }
}
