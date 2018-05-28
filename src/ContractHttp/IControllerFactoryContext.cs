namespace ContractHttp
{
    using System;
    using FluentIL;

    /// <summary>
    /// Defines the controller factory context interface.
    /// </summary>
    public interface IControllerFactoryContext
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
        /// Gets the <see cref="IFieldBuilder"/> which will contain the base object instance.
        /// </summary>
        IFieldBuilder BaseObjectField { get; }
    }
}