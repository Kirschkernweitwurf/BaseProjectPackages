using System.Collections.Generic;
using Base.ToolPackage.Editor.Shared;
using UnityEngine;

namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>
    /// Draws a single result row: a colored kind chip, the highlighted menu path on the first line,
    /// the declaring type and the assigned tags on the second, plus the origin badge and the pin
    /// marker on the right.
    /// </summary>
    internal static class CommandRowDrawer
    {
        private const float AccentBarWidth = 3f;
        private const float BadgeWidth = 30f;
        private const float DetailHeight = 13f;
        private const float LineGap = 2f;
        private const float MainHeight = 16f;
        private const string PinGlyph = "\u2605";
        private const float PinWidth = 16f;
        private const float TagGap = 4f;
        private const float TextGap = 10f;
        private const float TopPadding = 5f;

        private static readonly GUIContent BuiltInBadge = new("lib", "This command is built into Unity");
        private static readonly GUIContent CreateChip = new("new", "Creates a new asset");
        private static readonly GUIContent MenuChip = new("run", "Runs a menu item");
        private static readonly GUIContent PackageBadge = new("pkg", "This command lives in a package");
        private static readonly GUIContent PinContent = new(PinGlyph, "Pinned to the top of the results");
        private static readonly GUIContent SettingsChip = new("set", "Opens a settings page");

        /// <summary>Draws one result row.</summary>
        /// <param name="row">The full row rectangle.</param>
        /// <param name="match">The entry to draw.</param>
        /// <param name="selected">Whether the keyboard selection sits on this row.</param>
        /// <param name="hover">Whether the mouse sits on this row.</param>
        /// <param name="term">Lowercase search term, used to pick out the matched characters.</param>
        /// <param name="tags">The tags assigned to the entry.</param>
        internal static void Draw(Rect row, CommandMatch match, bool selected, bool hover, string term,
            IReadOnlyList<string> tags)
        {
            CommandEntry entry = match.Entry;

            DrawBackground(row, selected, hover);

            float left = row.x + CommandPaletteStyles.RowInset;
            float right = row.xMax - CommandPaletteStyles.RowInset;

            Rect chip = new(left, row.y + (row.height - CommandPaletteStyles.ChipHeight) * 0.5f,
                CommandPaletteStyles.ChipWidth, CommandPaletteStyles.ChipHeight);

            float textLeft = chip.xMax + TextGap;
            float mainRight = right;

            if (match.IsPinned)
            {
                Rect pin = new(right - PinWidth, row.y + TopPadding, PinWidth, MainHeight);

                GUI.Label(pin, PinContent, CommandPaletteStyles.PinLabel);
                mainRight = pin.x - TagGap;
            }

            Rect badge = new(mainRight - BadgeWidth, row.y + TopPadding, BadgeWidth, MainHeight);
            Rect main = new(textLeft, row.y + TopPadding, Mathf.Max(0f, badge.x - TextGap - textLeft), MainHeight);
            Rect detail = new(textLeft, main.yMax + LineGap, Mathf.Max(0f, right - textLeft), DetailHeight);

            DrawChip(chip, entry.Kind);

            GUI.Label(main, new GUIContent(CommandHighlighter.Build(entry, term), entry.Path),
                CommandPaletteStyles.PathLabel);

            DrawBadge(badge, entry.Origin);
            DrawDetail(detail, entry, tags);
        }

        private static Color ChipColor(ECommandKind kind) => kind switch
        {
            ECommandKind.CreateAsset => CommandPaletteStyles.NewChipColor(),
            ECommandKind.Settings => CommandPaletteStyles.SettingsChipColor(),
            _ => CommandPaletteStyles.RunChipColor()
        };

        private static GUIContent ChipContent(ECommandKind kind) => kind switch
        {
            ECommandKind.CreateAsset => CreateChip,
            ECommandKind.Settings => SettingsChip,
            _ => MenuChip
        };

        private static void DrawBackground(Rect row, bool selected, bool hover)
        {
            if (selected)
            {
                CommandPaletteChrome.DrawFill(row, CommandPaletteStyles.RowSelectedColor(),
                    CommandPaletteStyles.CornerRadius);

                Rect bar = new(row.x, row.y + TopPadding, AccentBarWidth, row.height - TopPadding * 2f);

                CommandPaletteChrome.DrawFill(bar, CommandPaletteStyles.AccentColor(), AccentBarWidth * 0.5f);
                return;
            }

            if (hover)
                CommandPaletteChrome.DrawFill(row, CommandPaletteStyles.RowHoverColor(),
                    CommandPaletteStyles.CornerRadius);
        }

        private static void DrawBadge(Rect rect, EAssetOrigin origin)
        {
            if (origin == EAssetOrigin.Package)
                GUI.Label(rect, PackageBadge, CommandPaletteStyles.BadgeLabel);
            else if (origin == EAssetOrigin.BuiltIn)
                GUI.Label(rect, BuiltInBadge, CommandPaletteStyles.BadgeLabel);
        }

        private static void DrawChip(Rect rect, ECommandKind kind)
        {
            Color fill = ChipColor(kind);

            CommandPaletteChrome.DrawFill(rect, fill, CommandPaletteStyles.PillRadius);
            GUI.Label(rect, ChipContent(kind), CommandPaletteStyles.ChipLabelFor(fill));
        }

        private static void DrawDetail(Rect rect, CommandEntry entry, IReadOnlyList<string> tags)
        {
            float used = DrawTags(rect, tags);
            Rect label = new(rect.x, rect.y, Mathf.Max(0f, rect.width - used), rect.height);

            GUI.Label(label, new GUIContent(entry.Detail, TypeTooltip(entry)), CommandPaletteStyles.DetailLabel);
        }

        // Tags are laid out from the right edge inward so the type name keeps the left side. Returns
        // how much width the tags took, so the type name can be clipped against them.
        private static float DrawTags(Rect rect, IReadOnlyList<string> tags)
        {
            float x = rect.xMax;

            for (int i = tags.Count - 1; i >= 0; i--)
            {
                GUIContent content = new(tags[i]);
                float width = CommandPaletteStyles.TagLabel.CalcSize(content).x
                    + CommandPaletteStyles.PillPadding * 2f;

                if (x - width < rect.center.x)
                    break;

                Rect pill = new(x - width, rect.y, width, rect.height);

                CommandPaletteChrome.DrawFill(pill, CommandPaletteStyles.TagPillColor(),
                    CommandPaletteStyles.PillRadius);

                GUI.Label(pill, content, CommandPaletteStyles.TagLabel);

                x = pill.x - TagGap;
            }

            return rect.xMax - x;
        }

        private static string TypeTooltip(CommandEntry entry) => entry.Owner != null
            ? entry.Owner.FullName
            : string.Empty;
    }
}