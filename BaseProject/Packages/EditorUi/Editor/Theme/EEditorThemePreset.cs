namespace Base.EditorUiPackage
{
    /// <summary>
    /// The looks a theme can be started from.
    /// </summary>
    public enum EEditorThemePreset : byte
    {
        /// <summary>Warm greys under an amber accent, for evening work.</summary>
        Ember = 0,

        /// <summary>Red-green color blind safe.</summary>
        Harbor = 1,

        /// <summary>The most legible of the five.</summary>
        Ink = 2,

        /// <summary>Muted plum and rose, after the Rose Pine palette.</summary>
        Rose = 3,

        /// <summary>The Base look.</summary>
        Slate = 4
    }
}