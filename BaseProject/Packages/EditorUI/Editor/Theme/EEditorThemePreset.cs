namespace Base.EditorUIPackage.Editor
{
    /// <summary>
    /// The looks a theme can be started from, named after the pigment or stone each one is built on.
    /// </summary>
    public enum EEditorThemePreset : byte
    {
        /// <summary>Candlelight: the darkest of the eight, warm walls and warm text.</summary>
        Amber = 0,
        /// <summary>Deep blue walls, with blue and orange carrying the three states.</summary>
        Cobalt = 1,
        /// <summary>Achromatic, with the three states told apart by lightness.</summary>
        Graphite = 2,
        /// <summary>Green stone, quiet and low in saturation.</summary>
        Malachite = 3,
        /// <summary>Black on white. The most legible of the eight.</summary>
        Onyx = 4,
        /// <summary>Rose quartz, pink and soft.</summary>
        Quartz = 5,
        /// <summary>The Base look.</summary>
        Slate = 6,
        /// <summary>The patina on copper. Red-green color blind safe.</summary>
        Verdigris = 7
    }
}