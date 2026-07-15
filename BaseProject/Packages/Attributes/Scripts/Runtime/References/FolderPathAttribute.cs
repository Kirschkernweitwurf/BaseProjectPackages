using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Turns a string field into a folder path field with a browse button.
    /// By default, the path is stored relative to the project, for example "Assets/Art".
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class FolderPathAttribute : PropertyAttribute
    {
        /// <summary>When true, stores the absolute system path instead of a project-relative path.</summary>
        public bool Absolute { get; }

        /// <summary>Creates the attribute with an optional absolute-path mode.</summary>
        public FolderPathAttribute(bool absolute = false) => Absolute = absolute;
    }
}