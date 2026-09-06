using System;
using System.Collections.Generic;
using System.Reflection;

namespace Base.ToolsPackage.Editor.AssemblyGraph
{
    /// <summary>
    /// Records, per assembly, which other assemblies declare an ancestor of one of its types.
    /// <br/><br/>
    /// A compilation that names a type has to be able to walk that type up to its base classes, its
    /// interfaces and its generic constraints, so it needs whatever assembly declares them. None of
    /// that has to reach the emitted metadata, so the compiled reference table never mentions it and
    /// a check built on that table alone calls the reference removable.
    /// </summary>
    internal static class AssemblyAncestryScanner
    {
        /// <summary>Scans the named assemblies for the assemblies their types inherit from.</summary>
        /// <param name="assemblyNames">Names of the assemblies to scan.</param>
        /// <returns>Assembly name mapped to the assemblies its declared types reach through ancestry.</returns>
        internal static Dictionary<string, HashSet<string>> Scan(HashSet<string> assemblyNames)
        {
            Dictionary<string, HashSet<string>> ancestry = new(StringComparer.Ordinal);

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string name = assembly.GetName().Name;
                if (!assemblyNames.Contains(name))
                    continue;

                if (ancestry.ContainsKey(name))
                    continue;

                ancestry[name] = CollectAncestors(assembly, name);
            }

            return ancestry;
        }

        private static HashSet<string> CollectAncestors(Assembly assembly, string ownerName)
        {
            HashSet<string> ancestors = new(StringComparer.Ordinal);

            foreach (Type type in AssemblyTypeReader.Read(assembly))
            {
                try
                {
                    CollectFromType(type, ownerName, ancestors);
                }
                catch (Exception)
                {
                    // A type whose ancestry will not load contributes nothing. That drops a credit
                    // rather than inventing one, so the worst case is a candidate to check by hand.
                }
            }

            return ancestors;
        }

        private static void CollectFromType(Type type, string ownerName, HashSet<string> ancestors)
        {
            for (Type current = type.BaseType; current != null; current = current.BaseType)
                Add(current, ownerName, ancestors);

            foreach (Type contract in type.GetInterfaces())
                Add(contract, ownerName, ancestors);

            foreach (Type argument in type.GetGenericArguments())
                CollectFromArgument(argument, ownerName, ancestors);
        }

        private static void CollectFromArgument(Type argument, string ownerName, HashSet<string> ancestors)
        {
            if (!argument.IsGenericParameter)
                return;

            foreach (Type constraint in argument.GetGenericParameterConstraints())
                Add(constraint, ownerName, ancestors);
        }

        private static void Add(Type type, string ownerName, HashSet<string> ancestors)
        {
            string name = type.Assembly.GetName().Name;
            if (name == ownerName)
                return;

            ancestors.Add(name);
        }
    }
}