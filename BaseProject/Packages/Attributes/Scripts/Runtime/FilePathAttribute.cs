using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Turns a string field into a file path field with a browse button.
    /// By default the path is stored relative to the project, for example "Assets/Data/config.json".
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class FilePathAttribute : PropertyAttribute
    {
        /// <summary>Extension filter without a dot, for example "png". Empty allows any file.</summary>
        public string Extension { get; }

        /// <summary>When true, stores the absolute system path instead of a project-relative path.</summary>
        public bool Absolute { get; }

        /// <summary>Creates the attribute with an optional extension filter and absolute-path mode.</summary>
        public FilePathAttribute(string extension = "", bool absolute = false)
        {
            Extension = extension;
            Absolute = absolute;
        }
    }
}
