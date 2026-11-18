namespace Base.TextPackage.Editor
{
    /// <summary>The pages of the Text Fit window.</summary>
    internal enum ETextFitTab : byte
    {
        /// <summary>Texts of a real language that do not fit.</summary>
        Problems = 0,

        /// <summary>Texts that do not fit in the pseudo-language, so they have little room left to grow.</summary>
        Tight = 1,

        /// <summary>Boxes with words in them that no source checks.</summary>
        Unchecked = 2,

        /// <summary>The largest size each text style could take per language.</summary>
        Sizes = 3
    }
}