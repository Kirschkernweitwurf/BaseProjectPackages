using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Groups consecutive serialized fields into a tab bar.
    /// Fields sharing a group are shown under selectable tabs named by <see cref="Name"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TabAttribute : PropertyAttribute
    {
        /// <summary>Name of the tab this field belongs to.</summary>
        public string Name { get; }

        /// <summary>Key that separates independent tab bars. Empty groups all tabs into one bar.</summary>
        public string Group { get; }

        /// <summary>Creates the attribute with a tab name and an optional group key.</summary>
        public TabAttribute(string name, string group = "")
        {
            Name = name;
            Group = group;
        }
    }
}