using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Maps <see cref="EColor"/> values to concrete <see cref="Color"/> values.
    /// </summary>
    public static class EColorExtensions
    {
        /// <summary>Returns the concrete color for the given preset.</summary>
        public static Color ToColor(this EColor color)
        {
            switch (color)
            {
                case EColor.White:
                    return Color.white;
                case EColor.Black:
                    return Color.black;
                case EColor.Gray:
                    return new Color32(128, 128, 128, 255);
                case EColor.Red:
                    return new Color32(231, 76, 60, 255);
                case EColor.Orange:
                    return new Color32(230, 126, 34, 255);
                case EColor.Yellow:
                    return new Color32(241, 196, 15, 255);
                case EColor.Green:
                    return new Color32(46, 204, 113, 255);
                case EColor.Teal:
                    return new Color32(26, 188, 156, 255);
                case EColor.Cyan:
                    return new Color32(0, 188, 212, 255);
                case EColor.Blue:
                    return new Color32(52, 152, 219, 255);
                case EColor.Purple:
                    return new Color32(155, 89, 182, 255);
                case EColor.Pink:
                    return new Color32(232, 67, 147, 255);
                case EColor.Magenta:
                    return new Color32(255, 0, 255, 255);
                case EColor.Brown:
                    return new Color32(141, 110, 99, 255);
                case EColor.Lime:
                    return new Color32(205, 220, 57, 255);
                default:
                    return Color.white;
            }
        }
    }
}