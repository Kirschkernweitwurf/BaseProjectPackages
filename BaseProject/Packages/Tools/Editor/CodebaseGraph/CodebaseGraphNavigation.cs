using System.Collections.Generic;
using Base.ToolsPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolsPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// Where in the graph the window is currently looking, and what that reads as on screen.
    /// <para>
    /// Two things at once, because they are the same thing. The level being browsed and the entry the
    /// graph is pointed at decide both what the list holds and every piece of text above it, and a
    /// heading built from a copy of that state would be one drill-down behind the moment either
    /// changed.
    /// </para>
    /// <para>
    /// The window keeps its own serialized copy so a domain reload can put the reader back where they
    /// were. This holds the live state that copy is read into and written out of.
    /// </para>
    /// </summary>
    internal sealed class CodebaseGraphNavigation
    {
        private const string AllTypesSegment = "All types";
        private const string FocusNoticeFormat = "showing {0} and its neighbors, {1} step{2} out";
        private const string MembersHeadingFormat = "Members of {0}";
        private const string NamespacesHeadingFormat = "Namespaces ({0})";
        private const string NamespacesSegment = "All namespaces";
        private const string SearchCappedHeadingFormat = "Showing {0} of {1} matches for \"{2}\"";
        private const string SearchHeadingFormat = "{0} matches for \"{1}\"";
        private const string SearchSegmentFormat = "Search: {0}";

        private const string TypesCappedHeadingFormat = "Types in {0}, showing {1} of {2}. Narrow the "
            + "filter to see the rest.";

        private const string TypesHeadingFormat = "Types in {0}";

        /// <summary>The level being browsed: namespaces, the types in one, or the members of one.</summary>
        internal EGraphScope Scope { get; set; } = EGraphScope.Namespace;

        /// <summary>The namespace whose types are listed, or null while every namespace is listed.</summary>
        internal string CurrentNamespace { get; set; }

        /// <summary>The type whose members are listed, or null while no type is open.</summary>
        internal TypeNodeInfo CurrentType { get; set; }

        /// <summary>The namespace the graph is pointed at, or null.</summary>
        internal NamespaceNodeInfo FocusedNamespace { get; set; }

        /// <summary>The type the graph is pointed at, or null.</summary>
        internal TypeNodeInfo FocusedType { get; set; }

        /// <summary>The member the graph is pointed at, or null.</summary>
        internal MemberNodeInfo FocusedMember { get; set; }

        /// <summary>Whether the graph is pointed at anything at all.</summary>
        internal bool HasFocus => FocusedNamespace != null || FocusedType != null || FocusedMember != null;

        /// <summary>The id of whatever the graph is pointed at, or null when it is pointed at nothing.</summary>
        internal string FocusedId
        {
            get
            {
                if (FocusedMember != null)
                    return GraphEntryFactory.MakeMemberId(FocusedMember.Key);

                if (FocusedType != null)
                    return GraphEntryFactory.MakeTypeId(FocusedType.Key);

                return FocusedNamespace != null
                    ? GraphEntryFactory.MakeNamespaceId(FocusedNamespace.Name)
                    : null;
            }
        }

        /// <summary>Points the graph at nothing, without changing the level being browsed.</summary>
        internal void ClearFocus()
        {
            FocusedNamespace = null;
            FocusedType = null;
            FocusedMember = null;
        }

        /// <summary>
        /// Finds a type by the name a reload was saved under. The keys are rebuilt by every scan, so a
        /// saved selection can only be matched on the full name.
        /// </summary>
        /// <param name="graph">The scanned graph to search.</param>
        /// <param name="fullName">The full name that was saved.</param>
        /// <returns>The type, or null when the scan no longer holds it.</returns>
        internal static TypeNodeInfo FindType(CodebaseGraphData graph, string fullName)
        {
            if (graph == null || string.IsNullOrEmpty(fullName))
                return null;

            foreach (TypeNodeInfo type in graph.Types.Values)
            {
                if (type.FullName == fullName)
                    return type;
            }

            return null;
        }

        /// <summary>The heading above the list, which names the level and how much of it is shown.</summary>
        /// <param name="isSearching">Whether a search is filtering the list.</param>
        /// <param name="search">The text being searched for.</param>
        /// <param name="shownCount">How many entries the list holds.</param>
        /// <param name="searchTotal">How many entries the search matched before the cap.</param>
        /// <param name="typeTotal">How many types the open namespace holds before the cap.</param>
        /// <returns>The heading text.</returns>
        internal string Heading(bool isSearching, string search, int shownCount, int searchTotal, int typeTotal)
        {
            if (isSearching)
                return shownCount < searchTotal
                    ? string.Format(SearchCappedHeadingFormat, shownCount, searchTotal, search)
                    : string.Format(SearchHeadingFormat, shownCount, search);

            switch (Scope)
            {
                case EGraphScope.Type:
                    return shownCount < typeTotal
                        ? string.Format(TypesCappedHeadingFormat,
                            CurrentNamespace ?? AllTypesSegment,
                            shownCount,
                            typeTotal)
                        : string.Format(TypesHeadingFormat, CurrentNamespace ?? AllTypesSegment);

                case EGraphScope.Member:
                    return CurrentType == null
                        ? string.Empty
                        : string.Format(MembersHeadingFormat, CurrentType.ShortName);

                default:
                    return string.Format(NamespacesHeadingFormat, shownCount);
            }
        }

        /// <summary>The breadcrumb, one segment per level between all namespaces and here.</summary>
        /// <param name="isSearching">Whether a search is filtering the list.</param>
        /// <param name="search">The text being searched for.</param>
        /// <returns>The segments, from the root outwards.</returns>
        internal List<string> Path(bool isSearching, string search)
        {
            List<string> path = new()
            {
                NamespacesSegment
            };

            if (isSearching)
            {
                path.Add(string.Format(SearchSegmentFormat, search));
                return path;
            }

            if (Scope == EGraphScope.Namespace)
                return path;

            path.Add(CurrentNamespace ?? AllTypesSegment);

            if (Scope == EGraphScope.Member && CurrentType != null)
                path.Add(CurrentType.ShortName);

            return path;
        }

        /// <summary>The line that says what the graph is pointed at and how far out it reaches.</summary>
        /// <param name="hops">How many steps out from the focused entry the graph shows.</param>
        /// <returns>The notice, or an empty string while the graph is pointed at nothing.</returns>
        internal string FocusNotice(int hops)
        {
            string focusedName = FocusedMember?.Name ?? FocusedType?.ShortName ?? FocusedNamespace?.Name;

            if (string.IsNullOrEmpty(focusedName))
                return string.Empty;

            string plural = hops == 1
                ? string.Empty
                : CodebaseGraphStyle.SClass;

            return string.Format(FocusNoticeFormat, focusedName, hops, plural);
        }
    }
}