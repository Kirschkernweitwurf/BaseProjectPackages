namespace Base.EditorUIPackage.Editor
{
    /// <summary>
    /// The USS class names of the shared Base look, so a window adds them through
    /// <c>nameof</c>-safe constants rather than by writing the string again in every view.
    /// </summary>
    /// <remarks>
    /// A class here is a promise to two readers: the shared sheet, which gives it a shape, and
    /// <see cref="EditorUssTheme"/>, which paints it from the active theme. Adding a class to an
    /// element that neither knows about does nothing.
    /// </remarks>
    public static class EditorUIClass
    {
        /// <summary>Accent colored text.</summary>
        public const string Accent = "base-accent";

        /// <summary>A filled, rounded label carrying a state or a count.</summary>
        public const string Badge = "base-badge";

        /// <summary>Any button drawn in the Base look.</summary>
        public const string Button = "base-button";

        /// <summary>The one action a window is mostly opened for.</summary>
        public const string ButtonPrimary = "base-button--primary";

        /// <summary>An action next to the primary one.</summary>
        public const string ButtonSecondary = "base-button--secondary";

        /// <summary>A rounded block holding a group of controls.</summary>
        public const string Card = "base-card";

        /// <summary>A small rounded label, quieter than a badge.</summary>
        public const string Chip = "base-chip";

        /// <summary>Red text, for something that is broken.</summary>
        public const string Danger = "base-danger";

        /// <summary>Secondary text, for paths, counts and hints.</summary>
        public const string Dim = "base-dim";

        /// <summary>The centered block shown when there is nothing to list.</summary>
        public const string Empty = "base-empty";

        /// <summary>The explanation under an empty state headline.</summary>
        public const string EmptyHint = "base-empty__hint";

        /// <summary>The headline of an empty state.</summary>
        public const string EmptyTitle = "base-empty__title";

        /// <summary>The root element of a Base window.</summary>
        public const string Root = "base-root";

        /// <summary>One row of a list.</summary>
        public const string Row = "base-row";

        /// <summary>Every second row, which is the one that gets striped.</summary>
        public const string RowAlternate = "base-row--alt";

        /// <summary>The row the user picked.</summary>
        public const string RowSelected = "base-row--selected";

        /// <summary>The header of one section of a window.</summary>
        public const string SectionHeader = "base-section-header";

        /// <summary>A hairline between two blocks.</summary>
        public const string Separator = "base-separator";

        /// <summary>The sentence under a window title.</summary>
        public const string Subtitle = "base-subtitle";

        /// <summary>Green text, for a passed check.</summary>
        public const string Success = "base-success";

        /// <summary>The name a window carries at its top.</summary>
        public const string Title = "base-title";

        /// <summary>The bar of controls along the top of a window.</summary>
        public const string Toolbar = "base-toolbar";

        /// <summary>Orange text, for something worth a second look.</summary>
        public const string Warning = "base-warning";
    }
}