using System;

namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>The toolbar state, in one object, so the entry factory only takes a single argument.</summary>
    public sealed class GraphFilter
    {
        /// <summary>Assembly to restrict the view to, or null for all scanned assemblies.</summary>
        public string AssemblyName { get; set; }

        /// <summary>Search text. Empty means no search.</summary>
        public string Search { get; set; } = string.Empty;

        /// <summary>Finding to narrow the view down to. None shows everything.</summary>
        public EFinding Finding { get; set; }

        /// <summary>True to include private members in the member view.</summary>
        public bool ShowPrivate { get; set; } = true;

        /// <summary>True to include fields, consts and properties in the member view.</summary>
        public bool ShowDataMembers { get; set; } = true;

        /// <summary>How many steps out from a focused entry the neighborhood reaches.</summary>
        public int Hops { get; set; } = 1;

        /// <summary>Checks a display name against the current search text.</summary>
        /// <param name="text">Name to test.</param>
        /// <returns>True when the name should be shown.</returns>
        public bool IsMatch(string text)
            => string.IsNullOrEmpty(Search)
                || (text != null && text.IndexOf(Search, StringComparison.OrdinalIgnoreCase) >= 0);
    }
}
