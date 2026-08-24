namespace Base.SettingsPackage.Controls
{
    /// <summary>The two look axes an invert setting can flip.</summary>
    public enum ELookAxis : byte
    {
        /// <summary>Left and right, driven by the horizontal component of the look input.</summary>
        Horizontal = 0,

        /// <summary>Up and down, driven by the vertical component of the look input.</summary>
        Vertical = 1
    }
}