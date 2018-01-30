namespace ContractHttp.Reflection.Emit
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// Represents a dynamic type factory.
    /// </summary>
    public abstract class TypeFactory
    {
        private static AssemblyBuilderCache cache = new AssemblyBuilderCache();

        private AssemblyBuilder assemblyBuilder;

        private ModuleBuilder moduleBuilder;

        private IServiceProvider services;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeFactory"/> class.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly used for dynamic types.</param>
        /// <param name="moduleName">The name of the module used for dynamic types.</param>
        protected TypeFactory(string assemblyName, string moduleName)
        {
            this.assemblyBuilder = cache.GetOrCreateAssemblyAndModuleBuilder(assemblyName, moduleName, out this.moduleBuilder);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeFactory"/> class.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly to use for dynamic types.</param>
        /// <param name="moduleName">The name of the module used for dynamic types.</param>
        /// <param name="services">The dependency scope to use.</param>
        protected TypeFactory(string assemblyName, string moduleName, IServiceProvider services)
            : this(assemblyName, moduleName)
        {
            this.services = services;
        }

        /// <summary>
        /// Gets the <see cref="AssemblyBuilder"/>
        /// </summary>
        protected AssemblyBuilder AssemblyBuilder
        {
            get
            {
                return this.assemblyBuilder;
            }
        }

        /// <summary>
        /// Gets the <see cref="ModuleBuilder"/>.
        /// </summary>
        protected ModuleBuilder ModuleBuilder
        {
            get
            {
                return this.moduleBuilder;
            }
        }

        /// <summary>
        /// Gets the dependency scope.
        /// </summary>
        protected IServiceProvider Services
        {
            get
            {
                return this.services;
            }
        }

        /// <summary>
        /// Gets a type by name from the current <see cref="AppDomain"/>.
        /// </summary>
        /// <param name="typeName">The name of the type.</param>
        /// <param name="dynamicOnly">A value indicating whether only dynamic assemblies should be checked or not.</param>
        /// <returns>A <see cref="Type"/> representing the type if found; otherwise null.</returns>
        public static Type GetType(string typeName, bool dynamicOnly)
        {
            Type dynamicType = cache.GetType(typeName);
            if (dynamicType != null)
            {
                return dynamicType;
            }

            if (dynamicOnly == false)
            {
                foreach (var ass in AssemblyCache.GetAssemblies())
                {
                    Type type = ass.GetType(typeName);
                    if (type != null)
                    {
                        return type;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if the argument is null and if it is throws an <see cref="ArgumentNullException"/>.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <param name="argument">The argument.</param>
        protected void ThrowIfArgumentIsNull(string argumentName, object argument)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Checks that the argument type is an interface, and if not throws an <see cref="ArgumentException"/>.
        /// </summary>
        /// <param name="argumentName">The name of the argument.</param>
        /// <param name="type">The argument type.</param>
        protected void ThrowIfArgumentIsNotAnInterface(string argumentName, Type type)
        {
            this.ThrowIfArgumentIsNull(argumentName, type);

            if (type.IsInterface == false)
            {
                throw new ArgumentException("Argument is not an interface", argumentName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the argument is null or and <see cref="ArgumentException"/> if the argument is an abstract type.
        /// </summary>
        /// <param name="argumentName">The argument name.</param>
        /// <param name="argument">The argument.</param>
        protected void ThrowIfArgumentIsNullOrAbstract(string argumentName, object argument)
        {
            this.ThrowIfArgumentIsNull(argumentName, argument);

            if (argument.GetType().IsAbstract == true)
            {
                throw new ArgumentException("Argument cannot be an abstract class", argumentName);
            }
        }

        /// <summary>
        /// Gets the generic type names for a method.
        /// </summary>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> of the method to get the generic argument names for.</param>
        /// <returns>An array of argument type names.</returns>
        protected string[] GetGenericArgumentNames(MethodInfo methodInfo)
        {
            return this.GetGenericArgumentNames(methodInfo.GetGenericArguments());
        }

        /// <summary>
        /// Gets the generic type names for list of types.
        /// </summary>
        /// <param name="genericArguments">An array of generic argument types.</param>
        /// <returns>An array of argument type names.</returns>
        protected string[] GetGenericArgumentNames(Type[] genericArguments)
        {
            string[] names = new string[genericArguments.Length];
            for (int k = 0; k < genericArguments.Length; k++)
            {
                names[k] = genericArguments[k].Name;
            }

            return names;
        }
    }
}
