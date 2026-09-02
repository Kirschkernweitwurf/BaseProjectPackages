using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.CommandPalette
{
    /// <summary>
    /// The tag editor that replaces the search box while tags are being written. Owns which entry
    /// is being tagged and the text typed so far; the window only asks whether it is active.
    /// </summary>
    internal sealed class CommandTagEditor
    {
        /// <summary>Name of the text control, so the window can put the caret in it.</summary>
        internal const string ControlName = "BaseCommandPaletteTags";

        private const string EmptyHint = "No tags yet. Write a few, separated by commas.";
        private const string KnownPrefix = "In use: ";
        private const string Placeholder = "gameplay, build, debug";
        private const string PrefixFormat = "Tags for {0}";
        private const string TagSeparator = ", ";

        /// <summary>Whether the editor currently replaces the search box.</summary>
        internal bool IsActive => _target != null;

        private static readonly char[] TagSeparators =
        {
            ',',
            ' '
        };

        private CommandEntry _target;
        private string _text = string.Empty;

        /// <summary>The line shown in the footer while the editor is open.</summary>
        /// <returns>The tags already used somewhere in the project.</returns>
        internal static string Hint()
        {
            IReadOnlyList<string> known = CommandTagStore.instance.KnownTags();

            return known.Count == 0
                ? EmptyHint
                : KnownPrefix + string.Join(TagSeparator, known);
        }

        /// <summary>Starts editing the tags of an entry.</summary>
        /// <param name="entry">The entry to tag.</param>
        internal void Begin(CommandEntry entry)
        {
            _target = entry;
            _text = string.Join(TagSeparator, CommandTagStore.instance.TagsFor(entry.Id));
        }

        /// <summary>Drops the changes and closes the editor.</summary>
        internal void Cancel()
        {
            _target = null;
            _text = string.Empty;
        }

        /// <summary>Writes the tags to the store and closes the editor.</summary>
        internal void Commit()
        {
            CommandTagStore.instance.SetTags(_target.Id,
                _text.Split(TagSeparators, StringSplitOptions.RemoveEmptyEntries));

            Cancel();
        }

        /// <summary>Draws the editor in place of the search box.</summary>
        /// <param name="box">The area the search box would occupy.</param>
        internal void Draw(Rect box)
        {
            CommandPaletteChrome.DrawFill(box, CommandPaletteStyles.FieldColor(), CommandPaletteStyles.CornerRadius);
            CommandPaletteChrome.DrawBorder(box, CommandPaletteStyles.PinColor(), CommandPaletteStyles.CornerRadius,
                CommandPaletteStyles.BorderWidth);

            Rect inner = CommandPaletteChrome.Inset(box, CommandPaletteStyles.RowInset);
            GUIContent prefix = new(string.Format(PrefixFormat, LeafOf(_target)));
            float width = CommandPaletteStyles.PrefixLabel.CalcSize(prefix).x;

            GUI.Label(new Rect(inner.x, inner.y, width, inner.height), prefix, CommandPaletteStyles.PrefixLabel);

            Rect field = new(inner.x + width + CommandPaletteStyles.Gap, inner.y,
                inner.width - width - CommandPaletteStyles.Gap, inner.height);

            GUI.SetNextControlName(ControlName);
            _text = EditorGUI.TextField(field, _text, CommandPaletteStyles.SearchField);

            if (_text.Length == 0)
                GUI.Label(field, Placeholder, CommandPaletteStyles.Placeholder);
        }

        private static string LeafOf(CommandEntry entry) => entry.Path[entry.LeafStart..];
    }
}