namespace Base.AttributePackage
{
    /// <summary>
    /// Controls in which editor state a button drawn by <see cref="ButtonAttribute"/> is enabled.
    /// </summary>
    public enum EButtonMode : byte
    {
        /// <summary>Enabled in both edit and play mode.</summary>
        Always = 0,

        /// <summary>Enabled only while in play mode.</summary>
        PlayMode = 1,

        /// <summary>Enabled only while not in play mode.</summary>
        EditMode = 2
    }
}