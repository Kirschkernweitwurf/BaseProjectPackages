using System.Collections.Generic;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.RequiredReferenceWindow
{
    /// <summary>A set of missing required references that all live on the same object.</summary>
    public sealed class RequiredReferenceGroup
    {
        /// <summary>The object that owns the missing references.</summary>
        public GameObject Owner { get; }

        /// <summary>All missing references on the object.</summary>
        public List<RequiredReferenceEntry> Entries { get; } = new();

        /// <summary>Creates a group for the given owner.</summary>
        public RequiredReferenceGroup(GameObject owner) => Owner = owner;
    }
}