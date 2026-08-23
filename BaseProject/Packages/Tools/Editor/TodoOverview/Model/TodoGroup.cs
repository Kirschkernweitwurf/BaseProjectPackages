using System.Collections.Generic;
using UnityEngine;

namespace Base.ToolPackage.Editor.TodoOverview.Model
{
    /// <summary>One section of the list: a header, the color it is banded with, and its items.</summary>
    internal sealed class TodoGroup
    {
        /// <summary>The section header, and the key the collapsed state is remembered under.</summary>
        internal string Label { get; }

        /// <summary>The color of the header band.</summary>
        internal Color Accent { get; }

        /// <summary>The items in this section, already sorted.</summary>
        internal List<TodoEntry> Entries { get; }

        /// <summary>Creates a section.</summary>
        /// <param name="label">The section header.</param>
        /// <param name="accent">The color of the header band.</param>
        /// <param name="entries">The items in the section.</param>
        internal TodoGroup(string label, Color accent, List<TodoEntry> entries)
        {
            Label = label;
            Accent = accent;
            Entries = entries;
        }
    }
}