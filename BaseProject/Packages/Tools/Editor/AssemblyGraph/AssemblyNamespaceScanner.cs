using System;
using System.Collections.Generic;
using System.Reflection;

namespace Base.ToolsPackage.Editor.AssemblyGraph
{
    /// <summary>
    /// Maps each namespace onto the assemblies that declare a type in it, which is what turns a
    /// using directive in the source back into the assembly the compiler had to be given.
    /// </summary>
    internal static class AssemblyNamespaceScanner
    {
        /// <summary>Scans the named assemblies for the namespaces they declare.</summary>
        /// <param name="assemblyNames">Names of the assemblies to scan.</param>
        /// <returns>Namespace mapped to the names of the assemblies declaring a type in it.</returns>
        internal static Dictionary<string, HashSet<string>> Scan(HashSet<string> assemblyNames)
        {
            Dictionary<string, HashSet<string>> owners = new(StringComparer.Ordinal);

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string name = assembly.GetName().Name;
                if (!assemblyNames.Contains(name))
                    continue;

                foreach (Type type in AssemblyTypeReader.Read(assembly))
                    Add(type.Namespace, name, owners);
            }

            return owners;
        }

        private static void Add(string namespaceName, string assemblyName,
            Dictionary<string, HashSet<string>> owners)
        {
            if (string.IsNullOrEmpty(namespaceName))
                return;

            if (!owners.TryGetValue(namespaceName, out HashSet<string> names))
            {
                names = new HashSet<string>(StringComparer.Ordinal);
                owners[namespaceName] = names;
            }

            names.Add(assemblyName);
        }
    }
}