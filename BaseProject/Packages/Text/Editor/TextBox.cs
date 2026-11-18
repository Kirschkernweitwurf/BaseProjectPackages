using System;
using TMPro;

namespace Base.TextPackage.Editor
{
    /// <summary>
    /// One text box loaded for the text fit check: the TextMeshPro text to render into, where it lives in the
    /// project and the Text Styles entry that sets its font and size per language, when it has a
    /// <see cref="LocalizedTextBox"/>. Made by <see cref="TextFitStage.Box"/>, which also marks the box as checked.
    /// </summary>
    public sealed class TextBox
    {
        /// <summary>Wraps <paramref name="text"/>, reading its style from a <see cref="LocalizedTextBox"/>.</summary>
        /// <param name="text">The text, already on the stage.</param>
        /// <param name="location">Where the box lives in the project.</param>
        /// <param name="prepare">Runs after every new text, for a box that code sizes to its text or null.</param>
        internal TextBox(TMP_Text text, TextBoxLocation location, Action<TMP_Text> prepare)
        {
            Text = text;
            Location = location;
            Prepare = prepare;
            Styled = text.GetComponent<LocalizedTextBox>();

            string key = Styled == null
                ? null
                : TextStyleTable.KeyOf(Styled);

            StyleKey = string.IsNullOrEmpty(key)
                ? null
                : key;
        }

        /// <summary>True when the box takes its font and size from the Text Styles table.</summary>
        public bool HasStyle => StyleKey != null;

        /// <summary>True when the box ends a longer text in "…" on purpose, as a short look at it.</summary>
        public bool CanCutLongText => Styled != null
            && Styled.CanCutLongText;

        /// <summary>True when the game breaks long words in this box with hyphens.</summary>
        public bool IsHyphenated => Styled != null
            && Styled.IsHyphenated;

        /// <summary>Where the box lives in the project.</summary>
        public TextBoxLocation Location { get; }

        /// <summary>Runs after every new text, before the layout or null.</summary>
        public Action<TMP_Text> Prepare { get; }

        /// <summary>The Text Styles entry or null for a box that keeps its own font and size.</summary>
        public string StyleKey { get; }

        /// <summary>The component that applies the style or null.</summary>
        public LocalizedTextBox Styled { get; }

        /// <summary>The text to render into.</summary>
        public TMP_Text Text { get; }
    }
}