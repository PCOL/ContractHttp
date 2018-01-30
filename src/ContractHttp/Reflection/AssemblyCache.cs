namespace ContractHttp.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Extensions.DependencyModel;

    /// <summary>
    /// 
    /// </summary>
    public static class AssemblyCache
    {
        /// <summary>
        /// Gets a list of loaded assemblies.
        /// </summary>
        /// <returns>A list of assemblies.</returns>
        public static IEnumerable<Assembly> GetAssemblies()
        {
             var compiled =
                from lib in DependencyContext.Default.GetDefaultAssemblyNames()
                let ass = Assembly.Load(lib)
                select ass;

            return FilterAssemblies(compiled);
        }

        private static IEnumerable<Assembly> FilterAssemblies(IEnumerable<Assembly> list)
        {
            if (list == null)
            {
                yield break;
            }

            foreach (var assembly in list)
            {
                yield return assembly;
            }
        }
    }
}