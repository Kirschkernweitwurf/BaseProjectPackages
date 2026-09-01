using System;

namespace Base.ToolPackage.Editor.CodebaseGraph.Model
{
    /// <summary>The toolbar state, in one object, so the entry factory only takes a single argument.</summary>
    internal sealed class GraphFilter
    {
        /// <summary>Assembly to restrict the view to, or null for all scanned assemblies.</summary>
        internal string AssemblyName { get; set; }

        /// <summary>Search text. Empty means no search.</summary>
        internal string Search { get; set; } = string.Empty;

        /// <summary>Finding to narrow the view down to. None shows everything.</summary>
        internal EFinding Finding { get; set; }

        /// <summary>True to include private members in the member view.</summary>
        internal bool ShowPrivate { get; set; } = true;

        /// <summary>True to include fields, consts and properties in the member view.</summary>
        internal bool ShowDataMembers { get; set; } = true;

        /// <summary>True to list a type's members inside its node, so it reads like a class.</summary>
        internal bool ShowMembersOnTypes { get; set; } = true;

        /// <summary>How many relation lines the graph draws at once.</summary>
        internal EEdgeMode EdgeMode { get; set; } = EEdgeMode.Muted;

        /// <summary>True to show only what the previous scan did not report.</summary>
        internal bool OnlyNew { get; set; }

        /// <summary>How the graph arranges what it draws.</summary>
        internal ELayoutMode LayoutMode { get; set; } = ELayoutMode.Dependencies;

        /// <summary>How far a search reaches.</summary>
        internal ESearchScope SearchScope { get; set; } = ESearchScope.Everywhere;

        /// <summary>How many steps out from a focused entry the neighborhood reaches.</summary>
        internal int Hops { get; set; } = 1;

        /// <summary>Checks a display name against the current search text.</summary>
        /// <param name="text">Name to test.</param>
        /// <returns>True when the name should be shown.</returns>
        internal bool IsMatch(string text) => string.IsNullOrEmpty(Search)
            || text != null && text.IndexOf(Search, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}