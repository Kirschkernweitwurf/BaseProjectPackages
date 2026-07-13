using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Auto-assigns the field from the parents when empty, searching strictly upward and skipping the
    /// own GameObject. Without a name it takes the first matching component on an ancestor, with a name
    /// it looks for an ancestor of that name. Mirrors <see cref="ChildAttribute"/> for the other side.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class GetComponentInParentAttribute : PropertyAttribute
    {
        /// <summary>Optional name of the ancestor to search for.</summary>
        public string Name { get; }

        /// <summary>Whether ancestors on inactive GameObjects are considered.</summary>
        public bool IncludeInactive { get; }

        /// <summary>Creates the attribute with an optional ancestor name and inactive mode.</summary>
        public GetComponentInParentAttribute(string name = null, bool includeInactive = true)
        {
            Name = name;
            IncludeInactive = includeInactive;
        }
    }
}
