using System;
using UnityEngine;

namespace Base.ToolsPackage.Editor.NamespaceConventionValidator
{
    /// <summary>
    /// Serializable mirror of the two fields of an assembly definition the scanner reads. The field
    /// names have to match the JSON keys, so they are not renamed to fit the usual style.
    /// </summary>
    [Serializable]
    internal sealed class AsmdefManifest
    {
        [SerializeField] private string name;
        [SerializeField] private string rootNamespace;

        /// <summary>The assembly name, which is the namespace root when none is declared.</summary>
        internal string Name => name;

        /// <summary>The declared root namespace, empty when the assembly leaves it to its name.</summary>
        internal string RootNamespace => rootNamespace;
    }
}