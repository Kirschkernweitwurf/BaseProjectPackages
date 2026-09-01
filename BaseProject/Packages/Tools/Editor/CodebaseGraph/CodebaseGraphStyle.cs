using Base.EditorUiPackage;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.CodebaseGraph
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
        internal const string ActionRowClass = "action-row";
        internal const string BreadcrumbBarClass = "breadcrumb-bar";
        internal const string BreadcrumbCurrentClass = "breadcrumb-current";
        internal const string BreadcrumbFocusClass = "breadcrumb-focus";
        internal const string BreadcrumbNoticeClass = "breadcrumb-notice";
        internal const string BreadcrumbPathClass = "breadcrumb-path";
        internal const string BreadcrumbSegmentClass = "breadcrumb-segment";
        internal const string BreadcrumbSeparatorClass = "breadcrumb-separator";
        internal const string CodebaseGraphRootClass = "codebase-graph-root";
        internal const string CodebaseNodeClass = "codebase-node";
        internal const string DismissButtonClass = "dismiss-button";
        internal const string DismissalNameClass = "dismissal-name";
        internal const string DismissalRowClass = "dismissal-row";
        internal const string DismissalScopeClass = "dismissal-scope";
        internal const string DismissalStaleClass = "dismissal-stale";
        internal const string DismissalTextClass = "dismissal-text";
        internal const string DismissedNoticeClass = "dismissed-notice";
        internal const string EmptyMarkClass = "empty-mark";
        internal const string EmptyStateClass = "empty-state";
        internal const string EmptyTitleClass = "empty-title";
        internal const string FindingActionClass = "finding-action";
        internal const string FindingActionTitleClass = "finding-action-title";
        internal const string FindingBadgeClass = "finding-badge";
        internal const string FindingCardClass = "finding-card";
        internal const string FindingCardTitleClass = "finding-card-title";
        internal const string FindingExplanationClass = "finding-explanation";
        internal const string FindingPartnerClass = "finding-partner";
        internal const string FindingRowClass = "finding-row";
        internal const string HasDismissalsClass = "has-dismissals";
        internal const string HasFindingClass = "has-finding";
        internal const string HasFindingsClass = "has-findings";
        internal const string IsActiveClass = "is-active";
        internal const string IsContractClass = "is-contract";
        internal const string IsDismissedClass = "is-dismissed";
        internal const string IsFocusedClass = "is-focused";
        internal const string IsHighClass = "is-high";
        internal const string IsMediumClass = "is-medium";
        internal const string IsOddClass = "is-odd";
        internal const string IsSelectedClass = "is-selected";
        internal const string IsSuccessClass = "is-success";
        internal const string IssueDetailClass = "issue-detail";
        internal const string IssueHeadingClass = "issue-heading";
        internal const string IssueListClass = "issue-list";
        internal const string IssueRowClass = "issue-row";
        internal const string IssueSeverityClass = "issue-severity";
        internal const string IssueTitleClass = "issue-title";
        internal const string LegendBodyClass = "legend-body";
        internal const string LegendClass = "legend";
        internal const string LegendEntryClass = "legend-entry";
        internal const string LegendGlyphClass = "legend-glyph";
        internal const string LegendLabelClass = "legend-label";
        internal const string LegendSwatchClass = "legend-swatch";
        internal const string LegendSwatchDismissedClass = "legend-swatch-dismissed";
        internal const string LegendSwatchFindingClass = "legend-swatch-finding";
        internal const string LegendTitleClass = "legend-title";
        internal const string LevelMemberClass = "level-member";
        internal const string LevelNamespaceClass = "level-namespace";
        internal const string LevelTypeClass = "level-type";
        internal const string ListRowClass = "list-row";
        internal const string ListRowMetaClass = "list-row-meta";
        internal const string ListRowTitleClass = "list-row-title";
        internal const string MemberClass = "member";
        internal const string MemberGlyphClass = "member-glyph";
        internal const string MemberLabelClass = "member-label";
        internal const string MemberListClass = "member-list";
        internal const string MemberRowClass = "member-row";
        internal const string MinimapClass = "minimap";
        internal const string MinimapDotClass = "minimap-dot";
        internal const string MinimapToggleClass = "minimap-toggle";
        internal const string MinimapViewClass = "minimap-view";
        internal const string NodeAccentClass = "node-accent";
        internal const string NodeGlyphClass = "node-glyph";
        internal const string NodeMetaClass = "node-meta";
        internal const string NodeSubtitleClass = "node-subtitle";
        internal const string OnlyNewClass = "only-new";
        internal const string PaneClass = "pane";
        internal const string PaneHeadingClass = "pane-heading";
        internal const string PanePlaceholderClass = "pane-placeholder";
        internal const string PaneSubtitleClass = "pane-subtitle";
        internal const string PaneTabClass = "pane-tab";
        internal const string PaneTabRowClass = "pane-tab-row";
        internal const string RelationEntryClass = "relation-entry";
        internal const string RowBadgeClass = "row-badge";
        internal const string RowClearClass = "row-clear";
        internal const string RowDismissedClass = "row-dismissed";
        internal const string SClass = "s";
        internal const string SectionTitleClass = "section-title";
        internal const string SortRowClass = "sort-row";
        internal const string SortSeparatorClass = "sort-separator";
        internal const string StatusBarClass = "status-bar";
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
            .Background(IsOddClass, () => EditorPalette.Stripe)
            .Background(IssueSeverityClass, () => EditorPalette.KeyCap)
            .Background(MinimapClass, () => EditorPalette.Background)
            .Background(MinimapToggleClass, () => EditorPalette.Card)
            .Background(LegendClass, () => EditorPalette.Card)
            .Background(FindingBadgeClass, () => EditorTableStyles.DangerBadgeColor)
            .Border(ListRowClass, () => EditorPalette.Separator)
            .Border(DismissalRowClass, () => EditorPalette.Separator)
            .Border(MemberListClass, () => EditorPalette.Separator)
            .Text(NodeSubtitleClass, () => EditorPalette.DimText)
            .Text(NodeMetaClass, () => EditorPalette.Text);
    }
}