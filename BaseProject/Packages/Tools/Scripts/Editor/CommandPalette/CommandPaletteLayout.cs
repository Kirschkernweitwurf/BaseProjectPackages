using UnityEngine;

namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>
    /// Splits the window into its five bands. Doing the arithmetic once and in one place keeps the
    /// gaps above and below the hairlines symmetric no matter how the window is resized.
    /// </summary>
    internal readonly struct CommandPaletteLayout
    {
        /// <summary>The search box and the pills next to it.</summary>
        public Rect Search { get; }

        /// <summary>Hairline between the search box and the results.</summary>
        public Rect TopLine { get; }

        /// <summary>Everything the result list may use.</summary>
        public Rect List { get; }

        /// <summary>Hairline between the results and the hints.</summary>
        public Rect BottomLine { get; }

        /// <summary>The key hints and the status message.</summary>
        public Rect Footer { get; }

        /// <summary>Divides the window into bands.</summary>
        /// <param name="window">The window rectangle, starting at zero.</param>
        public CommandPaletteLayout(Rect window)
        {
            float padding = CommandPaletteStyles.WindowPadding;
            float gap = CommandPaletteStyles.SeparatorGap;
            float thickness = CommandPaletteStyles.SeparatorThickness;
            float width = window.width - padding * 2f;

            Search = new Rect(padding, padding, width, CommandPaletteStyles.SearchHeight);
            TopLine = new Rect(padding, Search.yMax + gap, width, thickness);

            Footer = new Rect(padding, window.height - gap - CommandPaletteStyles.FooterHeight, width,
                CommandPaletteStyles.FooterHeight);

            BottomLine = new Rect(padding, Footer.y - gap - thickness, width, thickness);

            float top = TopLine.yMax + gap;

            List = new Rect(padding, top, width, Mathf.Max(0f, BottomLine.y - gap - top));
        }
    }
}