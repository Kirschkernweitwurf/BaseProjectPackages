using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Auto-assigns the field from the children when empty. Without a name it takes the first matching
    /// component in the children, with a name it searches descendants for a transform of that name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ChildAttribute : PropertyAttribute
    {
        /// <summary>Optional name of the child transform to search for.</summary>
        public string Name { get; }

        /// <summary>Whether inactive children are considered.</summary>
        public bool IncludeInactive { get; }

        /// <summary>Creates the attribute with an optional child name and inactive mode.</summary>
        public ChildAttribute(string name = null, bool includeInactive = true)
        {
            Name = name;
            IncludeInactive = includeInactive;
        }
    }
}