namespace ContractHttp.Reflection.Emit
{
    using System;
    using System.Linq;
    using System.Reflection.Emit;

    /// <summary>
    /// Represent contextual data used by <see cref="TypeFactory"/> implementations.
    /// </summary>
    public class TypeFactoryContext
        : ITypeFactoryContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeFactoryContext"/> class.
        /// </summary>
        /// <param name="typeBuilder">The <see cref="TypeBuilder"/> being use to create the type.</param>
        /// <param name="newType">The new type being built.</param>
        /// <param name="baseType">The base type being built on.</param>
        /// <param name="services">The current dependency injection scope</param>
        /// <param name="baseObjectField">The <see cref="FieldBuilder"/> that holds the base type instance.</param>
        /// <param name="servicesField">The <see cref="FieldBuilder"/> that holds the <see cref="IFrameworkServices"/> instance.</param>
        /// <param name="ctorBuilder">The <see cref="ConstructorBuilder"/> for the types constructor.</param>
        public TypeFactoryContext(
            TypeBuilder typeBuilder,
            Type newType,
            Type baseType,
            IServiceProvider services,
            FieldBuilder baseObjectField,
            FieldBuilder servicesField,
            ConstructorBuilder ctorBuilder = null)
            : this(
                typeBuilder,
                newType,
                new Type[] { baseType },
                services,
                baseObjectField,
                servicesField,
                ctorBuilder)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeFactoryContext"/> class.
        /// </summary>
        /// <param name="typeBuilder">The <see cref="TypeBuilder"/> being use to create the type.</param>
        /// <param name="newType">The new type being built.</param>
        /// <param name="baseTypes">The base types being built on.</param>
        /// <param name="services">The current dependency injection scope</param>
        /// <param name="baseObjectField">The <see cref="FieldBuilder"/> that holds the base type instance.</param>
        /// <param name="servicesField">The <see cref="FieldBuilder"/> that holds the <see cref="IFrameworkServices"/> instance.</param>
        /// <param name="ctorBuilder">The <see cref="ConstructorBuilder"/> for the types constructor.</param>
        public TypeFactoryContext(
            TypeBuilder typeBuilder,
            Type newType,
            Type[] baseTypes,
            IServiceProvider services,
            FieldBuilder baseObjectField,
            FieldBuilder servicesField,
            ConstructorBuilder ctorBuilder = null)
        {
            this.TypeBuilder = typeBuilder;
            this.NewType = newType;
            this.BaseTypes = baseTypes;
            this.Services = services;
            this.BaseObjectField = baseObjectField;
            this.ServicesField = servicesField;
            this.ConstructorBuilder = ctorBuilder;
        }

        /// <summary>
        ///  Gets the <see cref="TypeBuilder"/>
        /// </summary>
        public TypeBuilder TypeBuilder { get; }

        /// <summary>
        /// Gets the new type.
        /// </summary>
        public Type NewType { get; }

        /// <summary>
        /// Gets the base type.
        /// </summary>
        public Type BaseType
        {
            get
            {
                return this.BaseTypes[0];
            }
        }

        /// <summary>
        /// Gets the base types.
        /// </summary>
        public Type[] BaseTypes { get; }

        /// <summary>
        /// Gets the current dependency injection scope.
        /// </summary>
        public IServiceProvider Services { get; }

        /// <summary>
        /// Gets the <see cref="FieldBuilder"/> which will contain the base object instance.
        /// </summary>
        public FieldBuilder BaseObjectField { get; }

        /// <summary>
        /// Gets the <see cref="FieldBuilder"/> which will contain the dependency injection scope.
        /// </summary>
        public FieldBuilder ServicesField { get; }

        /// <summary>
        /// Gets the <see cref="ConstructorBuilder"/> used to construct the new object.
        /// </summary>
        public ConstructorBuilder ConstructorBuilder { get; }

        /// <summary>
        /// Creates a new <see cref="TypeFactoryContext"/> instance for a new interface type.
        /// </summary>
        /// <param name="interfaceType">The adapter <see cref="Type"/>.</param>
        /// <returns>The new <see cref="TypeFactoryContext"/> instance.</returns>
        public TypeFactoryContext CreateTypeFactoryContext(Type interfaceType)
        {
            var context = new TypeFactoryContext(this.TypeBuilder, interfaceType, this.BaseTypes, this.Services, this.BaseObjectField, this.ServicesField, this.ConstructorBuilder);
            return context;
        }

        /// <summary>
        /// Does the type build implement a given interface type
        /// </summary>
        /// <param name="ifaceType">Interface type.</param>
        /// <returns>True if it does; otherwise false.</returns>
        public bool DoesTypeBuilderImplementInterface(Type ifaceType)
        {
            return this.TypeBuilder.GetInterfaces().FirstOrDefault((type) => ifaceType == type) != null;
        }
    }
}
