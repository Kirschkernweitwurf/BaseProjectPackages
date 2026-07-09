using System;
using UnityEngine;

namespace Base.AttributePackage.References
{
    /// <summary>
    /// Draws a string field as a dropdown of project tags.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TagAttribute : PropertyAttribute
    {
        /// <summary>When true, only existing tags can be picked and no new tag can be created.</summary>
        public bool OnlyExisting { get; }

        /// <summary>Creates the attribute with an optional restriction to existing tags.</summary>
        public TagAttribute(bool onlyExisting = false) => OnlyExisting = onlyExisting;
    }
}