using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Groups consecutive fields into tabs. Fields sharing a group form one tab bar, and fields sharing
    /// a name within that group sit on the same tab.
    /// </summary>
    /// <remarks>
    /// <see cref="Foldout"/> and <see cref="DefaultExpanded"/> are read from the first field of the
    /// group, the same way <see cref="TitleAttribute"/> reads its collapsible state from the field that
    /// opens the section.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TabAttribute : PropertyAttribute
    {
        /// <summary>Name of the tab this field sits on.</summary>
        public string Name { get; }

        /// <summary>Name of the tab group, so several independent tab bars can coexist.</summary>
        public string Group { get; }

        /// <summary>Whether the whole tab group sits under a collapsible header.</summary>
        public bool Foldout { get; set; }

        /// <summary>Whether that header starts expanded. Ignored while <see cref="Foldout"/> is false.</summary>
        public bool DefaultExpanded { get; set; } = true;

        /// <summary>Creates the attribute.</summary>
        /// <param name="name">Name of the tab this field sits on.</param>
        /// <param name="group">Name of the tab group.</param>
        public TabAttribute(string name, string group = "")
        {
            Name = name;
            Group = group;
        }
    }
}
