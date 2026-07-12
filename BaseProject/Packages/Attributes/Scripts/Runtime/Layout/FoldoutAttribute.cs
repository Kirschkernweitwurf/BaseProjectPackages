using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Groups consecutive serialized fields under a collapsible foldout with the given name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class FoldoutAttribute : PropertyAttribute
    {
        /// <summary>Display name and grouping key of the foldout.</summary>
        public string Name { get; }

        /// <summary>Creates the attribute with the given foldout name.</summary>
        public FoldoutAttribute(string name) => Name = name;
    }
}