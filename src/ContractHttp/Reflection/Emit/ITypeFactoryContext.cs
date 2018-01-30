namespace ContractHttp.Reflection.Emit
{
    using System;
    using System.Reflection.Emit;

    /// <summary>
    /// Defines the type factory context interface.
    /// </summary>
    public interface ITypeFactoryContext
    {
        /// <summary>
        /// Gets the new type.
        /// </summary>
        Type NewType { get; }

        /// <summary>
        /// Gets the base type.
        /// </summary>
        Type BaseType { get; }

        /// <summary>
        /// Gets the base types.
        /// </summary>
        Type[] BaseTypes { get; }

        /// <summary>
        /// Gets the <see cref="FieldBuilder"/> which will contain the base object instance.
        /// </summary>
        FieldBuilder BaseObjectField { get; }
    }
}