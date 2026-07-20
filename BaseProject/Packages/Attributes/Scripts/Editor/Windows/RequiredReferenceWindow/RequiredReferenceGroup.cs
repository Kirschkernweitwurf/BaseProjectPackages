using System.Collections.Generic;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.RequiredReferenceWindow
{
    /// <summary>A set of validation issues that all live on the same object or asset.</summary>
    public sealed class RequiredReferenceGroup
    {
        /// <summary>The owner of the issues. A GameObject for scene items, the asset otherwise.</summary>
        public Object Owner { get; }

        /// <summary>All issues on the owner.</summary>
        public List<RequiredReferenceEntry> Entries { get; } = new();

        /// <summary>Creates a group for the given owner.</summary>
        public RequiredReferenceGroup(Object owner) => Owner = owner;
    }
}