using Base.EditorUIPackage.Editor;
using UnityEngine.UIElements;

namespace Base.ToolsPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// Attaches the codebase graph sheets and keeps the colors that mean the same thing across the
    /// Base windows in step with the active theme.
    /// </summary>
    /// <remarks>
    /// The sheet writes its colors out literally rather than through variables, and nothing in C# can
    /// write a USS custom property anyway, so themed colors arrive as inline styles through
    /// <see cref="EditorUssPainter"/>. What the sheet declares stays as the built-in look.
    /// <para>
    /// The registration list below is short on purpose. Most of what this window colors is either a
    /// meaning only a codebase graph has, such as the teal that marks a dismissal, or a state the
    /// painter cannot reach, and both are better left where they already work.
    /// </para>
    /// </remarks>
    internal static class CodebaseGraphStyle
    {
        /// <summary>A row of buttons under a finding, offering what can be done about it.</summary>
        internal const string ActionRowClass = "action-row";
        /// <summary>The trail across the top showing where in the graph the view sits.</summary>
        internal const string BreadcrumbBarClass = "breadcrumb-bar";
        /// <summary>The last segment of the trail, which is where the view is now.</summary>
        internal const string BreadcrumbCurrentClass = "breadcrumb-current";
        /// <summary>The segment the graph is focused on, when focus differs from the current level.</summary>
        internal const string BreadcrumbFocusClass = "breadcrumb-focus";
        /// <summary>A short note in the trail explaining why the view is showing what it is.</summary>
        internal const string BreadcrumbNoticeClass = "breadcrumb-notice";
        /// <summary>The container holding the trail segments and their separators.</summary>
        internal const string BreadcrumbPathClass = "breadcrumb-path";
        /// <summary>One clickable step in the trail.</summary>
        internal const string BreadcrumbSegmentClass = "breadcrumb-segment";
        /// <summary>The divider drawn between two trail segments.</summary>
        internal const string BreadcrumbSeparatorClass = "breadcrumb-separator";
        /// <summary>The window root. Everything the sheet styles sits under it.</summary>
        internal const string CodebaseGraphRootClass = "codebase-graph-root";
        /// <summary>One node on the graph canvas, whatever level it represents.</summary>
        internal const string CodebaseNodeClass = "codebase-node";
        /// <summary>The name of the member or type a dismissal covers.</summary>
        internal const string DismissalNameClass = "dismissal-name";
        /// <summary>One entry in the dismissals list.</summary>
        internal const string DismissalRowClass = "dismissal-row";
        /// <summary>Which finding a dismissal applies to, shown beside its name.</summary>
        internal const string DismissalScopeClass = "dismissal-scope";
        /// <summary>A dismissal whose finding no longer occurs, which is the moment to revisit it.</summary>
        internal const string DismissalStaleClass = "dismissal-stale";
        /// <summary>The reason recorded with a dismissal.</summary>
        internal const string DismissalTextClass = "dismissal-text";
        /// <summary>The button that silences a finding.</summary>
        internal const string DismissButtonClass = "dismiss-button";
        /// <summary>The banner shown when the current view is hiding dismissed findings.</summary>
        internal const string DismissedNoticeClass = "dismissed-notice";
        /// <summary>The glyph at the center of an empty pane.</summary>
        internal const string EmptyMarkClass = "empty-mark";
        /// <summary>The panel shown when a pane has nothing to list.</summary>
        internal const string EmptyStateClass = "empty-state";
        /// <summary>The headline of an empty pane.</summary>
        internal const string EmptyTitleClass = "empty-title";
        /// <summary>One suggested fix inside a finding card.</summary>
        internal const string FindingActionClass = "finding-action";
        /// <summary>The heading above a suggested fix.</summary>
        internal const string FindingActionTitleClass = "finding-action-title";
        /// <summary>The severity pill on a finding.</summary>
        internal const string FindingBadgeClass = "finding-badge";
        /// <summary>The panel describing one finding in full.</summary>
        internal const string FindingCardClass = "finding-card";
        /// <summary>The heading of a finding card.</summary>
        internal const string FindingCardTitleClass = "finding-card-title";
        /// <summary>The prose explaining why a finding matters.</summary>
        internal const string FindingExplanationClass = "finding-explanation";
        /// <summary>The other end of a finding that names two things, such as a cycle.</summary>
        internal const string FindingPartnerClass = "finding-partner";
        /// <summary>One finding in the list, before it is opened into a card.</summary>
        internal const string FindingRowClass = "finding-row";
        /// <summary>Set on anything holding at least one dismissal, so it can be marked without a count.</summary>
        internal const string HasDismissalsClass = "has-dismissals";
        /// <summary>Set on a single node or row that carries a finding.</summary>
        internal const string HasFindingClass = "has-finding";
        /// <summary>Set on a container holding at least one finding.</summary>
        internal const string HasFindingsClass = "has-findings";
        /// <summary>The tab or toggle currently in use.</summary>
        internal const string IsActiveClass = "is-active";
        /// <summary>A node or edge that is part of a package contract rather than internal wiring.</summary>
        internal const string IsContractClass = "is-contract";
        /// <summary>Something whose finding has been silenced. Kept visible but muted.</summary>
        internal const string IsDismissedClass = "is-dismissed";
        /// <summary>The node the graph is centered on.</summary>
        internal const string IsFocusedClass = "is-focused";
        /// <summary>High severity. Modifier on a finding or its badge.</summary>
        internal const string IsHighClass = "is-high";
        /// <summary>Medium severity. Modifier on a finding or its badge.</summary>
        internal const string IsMediumClass = "is-medium";
        /// <summary>Every second row, which is what draws the zebra striping.</summary>
        internal const string IsOddClass = "is-odd";
        /// <summary>The row or node the user picked.</summary>
        internal const string IsSelectedClass = "is-selected";
        /// <summary>A result with nothing to report. Modifier on a badge or an empty state.</summary>
        internal const string IsSuccessClass = "is-success";
        /// <summary>The body text of one issue.</summary>
        internal const string IssueDetailClass = "issue-detail";
        /// <summary>The heading above a group of issues.</summary>
        internal const string IssueHeadingClass = "issue-heading";
        /// <summary>The container holding the issue rows.</summary>
        internal const string IssueListClass = "issue-list";
        /// <summary>One issue in the list.</summary>
        internal const string IssueRowClass = "issue-row";
        /// <summary>The severity marker on an issue row.</summary>
        internal const string IssueSeverityClass = "issue-severity";
        /// <summary>The one-line summary of an issue.</summary>
        internal const string IssueTitleClass = "issue-title";
        /// <summary>The entries of the legend, below its title.</summary>
        internal const string LegendBodyClass = "legend-body";
        /// <summary>The legend explaining the colors used on the canvas.</summary>
        internal const string LegendClass = "legend";
        /// <summary>One swatch and label pair in the legend.</summary>
        internal const string LegendEntryClass = "legend-entry";
        /// <summary>The symbol shown for a legend entry that is not a plain color.</summary>
        internal const string LegendGlyphClass = "legend-glyph";
        /// <summary>The text naming what a legend entry means.</summary>
        internal const string LegendLabelClass = "legend-label";
        /// <summary>The color square of a legend entry.</summary>
        internal const string LegendSwatchClass = "legend-swatch";
        /// <summary>The swatch standing for dismissed items.</summary>
        internal const string LegendSwatchDismissedClass = "legend-swatch-dismissed";
        /// <summary>The swatch standing for items carrying a finding.</summary>
        internal const string LegendSwatchFindingClass = "legend-swatch-finding";
        /// <summary>The heading of the legend.</summary>
        internal const string LegendTitleClass = "legend-title";
        /// <summary>Set while the graph is showing members, which is the innermost level.</summary>
        internal const string LevelMemberClass = "level-member";
        /// <summary>Set while the graph is showing namespaces, which is the outermost level.</summary>
        internal const string LevelNamespaceClass = "level-namespace";
        /// <summary>Set while the graph is showing types.</summary>
        internal const string LevelTypeClass = "level-type";
        /// <summary>A generic row in one of the side panes.</summary>
        internal const string ListRowClass = "list-row";
        /// <summary>The secondary text on a list row, such as a count or a path.</summary>
        internal const string ListRowMetaClass = "list-row-meta";
        /// <summary>The primary text on a list row.</summary>
        internal const string ListRowTitleClass = "list-row-title";
        /// <summary>One member entry, wherever members are listed.</summary>
        internal const string MemberClass = "member";
        /// <summary>The symbol marking what kind of member an entry is.</summary>
        internal const string MemberGlyphClass = "member-glyph";
        /// <summary>The member name.</summary>
        internal const string MemberLabelClass = "member-label";
        /// <summary>The container holding the member rows of a type.</summary>
        internal const string MemberListClass = "member-list";
        /// <summary>One member row inside a node or a pane.</summary>
        internal const string MemberRowClass = "member-row";
        /// <summary>The overview of the whole graph shown in the corner.</summary>
        internal const string MinimapClass = "minimap";
        /// <summary>One node as it appears in the minimap.</summary>
        internal const string MinimapDotClass = "minimap-dot";
        /// <summary>The control that shows and hides the minimap.</summary>
        internal const string MinimapToggleClass = "minimap-toggle";
        /// <summary>The rectangle in the minimap marking what the canvas is showing.</summary>
        internal const string MinimapViewClass = "minimap-view";
        /// <summary>The colored strip on a node that carries its category color.</summary>
        internal const string NodeAccentClass = "node-accent";
        /// <summary>The symbol marking what kind of thing a node represents.</summary>
        internal const string NodeGlyphClass = "node-glyph";
        /// <summary>The counts and flags on a node, below its title.</summary>
        internal const string NodeMetaClass = "node-meta";
        /// <summary>The second line of a node, usually its namespace or owner.</summary>
        internal const string NodeSubtitleClass = "node-subtitle";
        /// <summary>Set while the view is filtered to findings that are not in the baseline.</summary>
        internal const string OnlyNewClass = "only-new";
        /// <summary>One of the window sections, each with a heading and a body.</summary>
        internal const string PaneClass = "pane";
        /// <summary>The heading of a pane.</summary>
        internal const string PaneHeadingClass = "pane-heading";
        /// <summary>The text a pane shows before anything is selected.</summary>
        internal const string PanePlaceholderClass = "pane-placeholder";
        /// <summary>A secondary line under a pane heading.</summary>
        internal const string PaneSubtitleClass = "pane-subtitle";
        /// <summary>One tab in a pane that holds several views.</summary>
        internal const string PaneTabClass = "pane-tab";
        /// <summary>The row of tabs across the top of such a pane.</summary>
        internal const string PaneTabRowClass = "pane-tab-row";
        /// <summary>One incoming or outgoing dependency listed for the selected item.</summary>
        internal const string RelationEntryClass = "relation-entry";
        /// <summary>A small count or status pill on a row.</summary>
        internal const string RowBadgeClass = "row-badge";
        /// <summary>A row with nothing to report, drawn muted so the ones that matter stand out.</summary>
        internal const string RowClearClass = "row-clear";
        /// <summary>A row whose finding has been silenced.</summary>
        internal const string RowDismissedClass = "row-dismissed";
        /// <summary>The plural suffix, appended to a label when its count is not one.</summary>
        internal const string SClass = "s";
        /// <summary>The heading of a titled block inside a pane body.</summary>
        internal const string SectionTitleClass = "section-title";
        /// <summary>The row of controls choosing how a list is ordered.</summary>
        internal const string SortRowClass = "sort-row";
        /// <summary>The divider between two groups of sort controls.</summary>
        internal const string SortSeparatorClass = "sort-separator";
        /// <summary>The strip along the bottom carrying counts and the scan state.</summary>
        internal const string StatusBarClass = "status-bar";
        /// <summary>The toolbar across the top of the window.</summary>
        internal const string TopBarClass = "top-bar";

        /// <summary>The GUID of the window's style sheet, from its meta file.</summary>
        private const string SheetGuid = "784a141b5d47f0c4c973d6666c7ced16";

        /// <summary>
        /// Attaches the shared and the window's own sheets and paints the tree from the active theme,
        /// repainting it whenever that theme changes.
        /// </summary>
        /// <param name="root">Element to style.</param>
        /// <returns>False when the window's own sheet is missing, so the caller can report it.</returns>
        internal static bool Apply(VisualElement root)
        {
            if (root == null)
                return false;

            EditorUssTheme.Apply(root, CreatePainter());

            return EditorStyleSheets.Apply(root, SheetGuid);
        }

        /// <summary>
        /// Maps the classes whose color carries a meaning the palette already names.
        /// </summary>
        /// <remarks>
        /// Only rows whose sheet gives a single edge a width are given a border color, because the
        /// painter writes all four and an issue row keeps a transparent three pixel left edge that it
        /// fills in by severity. Painting that one would draw a stripe down every row in the list.
        /// <para>
        /// Hover, selected and severity states stay with the sheet as well: an inline style is what
        /// the painter writes, and no pseudo state or compound selector can override one.
        /// </para>
        /// </remarks>
        /// <returns>The painter the window is drawn with.</returns>
        private static EditorUssPainter CreatePainter() => new EditorUssPainter()
            .Background(IsOddClass, color: () => EditorPalette.Stripe)
            .Background(IssueSeverityClass, color: () => EditorPalette.KeyCap)
            .Background(MinimapClass, color: () => EditorPalette.Background)
            .Background(MinimapToggleClass, color: () => EditorPalette.Card)
            .Background(LegendClass, color: () => EditorPalette.Card)
            .Background(FindingBadgeClass, color: () => EditorTableStyles.DangerBadgeColor)
            .Border(ListRowClass, color: () => EditorPalette.Separator)
            .Border(DismissalRowClass, color: () => EditorPalette.Separator)
            .Border(MemberListClass, color: () => EditorPalette.Separator)
            .Text(NodeSubtitleClass, color: () => EditorPalette.DimText)
            .Text(NodeMetaClass, color: () => EditorPalette.Text);
    }
}