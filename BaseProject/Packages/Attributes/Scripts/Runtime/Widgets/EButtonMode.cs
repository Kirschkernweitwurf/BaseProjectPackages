namespace Base.AttributePackage.Widgets
{
    /// <summary>
    /// Controls in which editor state a button drawn by <see cref="ButtonAttribute"/> is enabled.
    /// </summary>
    public enum EButtonMode
    {
        /// <summary>Enabled in both edit and play mode.</summary>
        Always,

        /// <summary>Enabled only while in play mode.</summary>
        PlayMode,

        /// <summary>Enabled only while not in play mode.</summary>
        EditMode
    }
}