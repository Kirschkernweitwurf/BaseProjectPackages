namespace Base.AttributePackage.Editor
{
    /// <summary>What a member contributes to the component header.</summary>
    public enum EHeaderItemKind : byte
    {
        /// <summary>A clickable button that runs the method.</summary>
        Button = 0,

        /// <summary>Read-only text showing the member's value.</summary>
        Label = 1,

        /// <summary>A rect handed to the method so it can draw whatever it likes.</summary>
        Draw = 2
    }
}
