namespace Base.ToolPackage.Editor.StaticResetChecker
{
    /// <summary>
    /// Options for scanning the project for static fields that are not reset on Enter Play Mode.
    /// </summary>
    internal sealed class ScanOptions
    {
        /// <summary>The folder the scan starts from.</summary>
        internal string RootFolder = "Assets";

        /// <summary>
        /// The attributes that count as resetting statics. A method carrying one of these clears the
        /// fields its type declares, so those fields are not reported.
        /// </summary>
        internal string[] ResetAttributes =
        {
            "InitializeOnEnterPlayMode",
            "RuntimeInitializeOnLoadMethod"
        };

        /// <summary>
        /// A comment marker that silences one field. Used where a static is cleared somewhere the scan
        /// cannot see, such as an OnDestroy on the object that owns it.
        /// </summary>
        internal string IgnoreMarker = "reset-ignore";

        /// <summary>
        /// Whether static events are reported. They are the most common leak of the lot, since
        /// handlers from the previous session survive and fire into destroyed objects.
        /// </summary>
        internal bool IncludeEvents = true;

        /// <summary>Whether static auto-properties are reported alongside plain fields.</summary>
        internal bool IncludeAutoProperties = true;

        /// <summary>
        /// Whether editor folders are passed over. Editor code is not affected by Enter Play Mode
        /// options, so its statics are usually a false positive.
        /// </summary>
        internal bool SkipEditorFolders = true;

        /// <summary>
        /// Whether a reset method is followed into the helpers it calls, so clearing done one level
        /// down still counts.
        /// </summary>
        internal bool ExpandHelpers = true;

        /// <summary>
        /// Whether readonly statics are passed over. A readonly reference cannot be reassigned, though
        /// what it points at can still hold state, which is why this is a choice rather than a rule.
        /// </summary>
        internal bool IgnoreReadonly = true;
    }
}