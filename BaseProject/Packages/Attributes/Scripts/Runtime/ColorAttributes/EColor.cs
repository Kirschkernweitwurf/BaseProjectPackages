namespace Base.AttributePackage
{
    /// <summary>
    /// Predefined colors accepted by color-aware attributes.
    /// <see cref="Default"/> means no explicit color and falls back to the drawer default.
    /// </summary>
    public enum EColor : byte
    {
        /// <summary>No explicit color. Uses the drawer default.</summary>
        Default = 0,
        White = 1,
        Black = 2,
        Gray = 3,
        Red = 4,
        Orange = 5,
        Yellow = 6,
        Green = 7,
        Teal = 8,
        Cyan = 9,
        Blue = 10,
        Purple = 11,
        Pink = 12,
        Magenta = 13,
        Brown = 14,
        Lime = 15
    }
}