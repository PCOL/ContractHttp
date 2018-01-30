namespace ContractHttp.Reflection.Emit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// Represents an <see cref="AssemblyBuilder"/> cache.
    /// </summary>
    public class AssemblyBuilderCache
    {
        private Dictionary<string, AssemblyModule> cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyBuilderCache"/> class.
        /// </summary>
        public AssemblyBuilderCache()
        {
            this.cache = new Dictionary<string, AssemblyModule>();
        }

        /// <summary>
        /// Gets or creates an <see cref="AssemblyBuilder"/> and <see cref="ModuleBuilder"/> pair.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly builder.</param>
        /// <param name="moduleName">The name of the module builder.</param>
        /// <param name="moduleBuilder">A variable to receive the module builder.</param>
        /// <returns>An assembly builder instance.</returns>
        public AssemblyBuilder GetOrCreateAssemblyAndModuleBuilder(string assemblyName, string moduleName, out ModuleBuilder moduleBuilder)
        {
            moduleBuilder = null;

            AssemblyBuilder builder;
            AssemblyModule details;
            if (this.cache.TryGetValue(assemblyName, out details) == false)
            {
                AssemblyName name = new AssemblyName(assemblyName);
                builder = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndCollect);
                details = new AssemblyModule(builder);

                this.cache.Add(assemblyName, details);
            }
            else
            {
                builder = details.AssemblyBuilder;
            }

            moduleBuilder = details.GetOrCreateModuleBuilder(moduleName);

            return builder;
        }

        /// <summary>
        /// Removes an assembly builder and all of its module builders.
        /// </summary>
        /// <param name="name">The name of the assembly builder.</param>
        /// <returns>True if removed; otherwise false.</returns>
        public bool RemoveAssemblyBuilder(string name)
        {
            return this.cache.Remove(name);
        }

        /// <summary>
        /// Gets a type by name.
        /// </summary>
        /// <param name="typeName">The name of the type.</param>
        /// <returns>A <see cref="Type"/> if found; otherwise null.</returns>
        public Type GetType(string typeName)
        {
            foreach (var assemblyModule in this.cache.Values)
            {
                Type type = assemblyModule.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        /// <summary>
        /// Represents an <see cref="AssemblyBuilder"/> and its <see cref="ModuleBuilder"/> instances.
        /// </summary>
        private class AssemblyModule
        {
            private Dictionary<string, ModuleDetails> moduleBuilders;

            /// <summary>
            /// Initializes a new instance of the <see cref="AssemblyModule"/> class.
            /// </summary>
            /// <param name="assemblyBuilder">An <see cref="AssemblyBuilder"/>.</param>
            public AssemblyModule(AssemblyBuilder assemblyBuilder)
            {
                // Debugging...
                Type daType = typeof(DebuggableAttribute);
                ConstructorInfo daCtor = daType.GetConstructor(new Type[] { typeof(DebuggableAttribute.DebuggingModes) });
                CustomAttributeBuilder attrBuilder = new CustomAttributeBuilder(daCtor, new object[] { DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.Default });
                assemblyBuilder.SetCustomAttribute(attrBuilder);

                this.AssemblyBuilder = assemblyBuilder;
                this.moduleBuilders = new Dictionary<string, ModuleDetails>();
            }

            /// <summary>
            /// Gets the assembly builder.
            /// </summary>
            public AssemblyBuilder AssemblyBuilder { get; }

            /// <summary>
            /// Gets or creates a <see cref="ModuleBuilder"/>
            /// </summary>
            /// <param name="name">The name of the module builder.</param>
            /// <returns>A <see cref="ModuleBuilder"/> instance.</returns>
            public ModuleBuilder GetOrCreateModuleBuilder(string name)
            {
                ModuleDetails moduleDetails;
                if (this.moduleBuilders.TryGetValue(name, out moduleDetails) == false)
                {
                    ModuleBuilder moduleBuilder = this.AssemblyBuilder.DefineDynamicModule(name);
                    moduleDetails = new ModuleDetails(moduleBuilder);
                    this.moduleBuilders.Add(name, moduleDetails);
                }

                return moduleDetails.ModuleBuilder;
            }

            /// <summary>
            /// Removes a module builder.
            /// </summary>
            /// <param name="name">The name of the module builder.</param>
            /// <returns>True if removed; otherwise false.</returns>
            public bool RemoveModuleBuilder(string name)
            {
                return this.moduleBuilders.Remove(name);
            }

            /// <summary>
            /// Gets a type by name.
            /// </summary>
            /// <param name="typeName">The name of the type.</param>
            /// <returns>A <see cref="Type"/> if found; otherwise null.</returns>
            public Type GetType(string typeName)
            {
                foreach (var moduleDetails in this.moduleBuilders.Values)
                {
                    Type type = moduleDetails.ModuleBuilder.GetType(typeName, false, false);
                    if (type != null)
                    {
                        return type;
                    }
                }

                return null;
            }
        }

        private class ModuleDetails
        {
            public ModuleDetails(ModuleBuilder moduleBuilder)
            {
                this.ModuleBuilder = moduleBuilder;
            }

            /// <summary>
            /// Gets the module builder.
            /// </summary>
            public ModuleBuilder ModuleBuilder { get; }
        }
    }
}
