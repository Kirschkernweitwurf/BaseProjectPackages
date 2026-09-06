using System;
using System.Collections.Generic;

namespace Base.ToolsPackage.Editor.AssemblyGraph
{
    /// <summary>
    /// Decides whether a declared assembly reference is needed, from three sources that each cover a
    /// blind spot of the ones before it.
    /// <br/><br/>
    /// The compiled reference table lists what the runtime has to load, which is strictly less than
    /// what the compiler had to be given. Ancestry covers what is reached only by walking a type up
    /// to its bases, and the using directives cover everything the compiler folds into a literal.
    /// Anything that needs an assembly through some path none of the three see is still reported as
    /// a candidate, which is why a candidate is a reason to look rather than a verdict.
    /// </summary>
    internal sealed class ReferenceUsageResolver
    {
        private readonly Dictionary<string, HashSet<string>> _ancestryByAssembly;
        private readonly Dictionary<string, HashSet<string>> _metadataByAssembly;
        private readonly Dictionary<string, HashSet<string>> _namespaceOwners;

        /// <summary>Creates a resolver over the three lookups it decides from.</summary>
        /// <param name="metadataByAssembly">Assembly name mapped to what its compiled metadata names.</param>
        /// <param name="ancestryByAssembly">Assembly name mapped to what its types inherit from.</param>
        /// <param name="namespaceOwners">Namespace mapped to the assemblies declaring a type in it.</param>
        internal ReferenceUsageResolver(Dictionary<string, HashSet<string>> metadataByAssembly,
            Dictionary<string, HashSet<string>> ancestryByAssembly,
            Dictionary<string, HashSet<string>> namespaceOwners)
        {
            _metadataByAssembly = metadataByAssembly;
            _ancestryByAssembly = ancestryByAssembly;
            _namespaceOwners = namespaceOwners;
        }

        /// <summary>Collects what an assembly needs, before anything is read from its source.</summary>
        /// <param name="assemblyName">Assembly to collect for.</param>
        /// <returns>The credited assembly names, or null when the metadata could not be read.</returns>
        internal HashSet<string> CollectCredited(string assemblyName)
        {
            if (!_metadataByAssembly.TryGetValue(assemblyName, out HashSet<string> metadata))
                return null;

            HashSet<string> credited = new(metadata, StringComparer.Ordinal);

            foreach (string reference in metadata)
            {
                if (_ancestryByAssembly.TryGetValue(reference, out HashSet<string> ancestors))
                    credited.UnionWith(ancestors);
            }

            return credited;
        }

        /// <summary>Decides the status of one declared reference.</summary>
        /// <param name="credited">Result of <see cref="CollectCredited"/> for the declaring assembly.</param>
        /// <param name="referenceName">Name of the declared reference.</param>
        /// <param name="usedNamespaces">Namespaces the declaring assembly names in using directives.</param>
        /// <returns>The status to report.</returns>
        internal EReferenceStatus Resolve(HashSet<string> credited,
            string referenceName,
            HashSet<string> usedNamespaces)
        {
            if (credited == null)
                return EReferenceStatus.Unknown;

            if (credited.Contains(referenceName))
                return EReferenceStatus.Used;

            return IsNamedByUsing(referenceName, usedNamespaces)
                ? EReferenceStatus.Used
                : EReferenceStatus.Candidate;
        }

        private bool IsNamedByUsing(string referenceName, HashSet<string> usedNamespaces)
        {
            foreach (string usedNamespace in usedNamespaces)
            {
                if (!_namespaceOwners.TryGetValue(usedNamespace, out HashSet<string> owners))
                    continue;

                if (owners.Contains(referenceName))
                    return true;
            }

            return false;
        }
    }
}