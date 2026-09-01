using System;
using UnityEngine;

namespace Base.EditorUiPackage
{
    /// <summary>
    /// Every color the Base editor windows share, for one editor theme. A theme holds two of these,
    /// one for the dark editor theme and one for the light one, and <see cref="EditorPalette"/> reads the
    /// pair that matches the editor theme currently active.
    /// </summary>
    /// <remarks>
    /// The values are stored as finished colors rather than as a base color plus an opacity, so a
    /// user can change any one of them without a second one moving with it. What used to be a
    /// neutral overlay is simply white or black at a low alpha here.
    /// </remarks>
    [Serializable]
    public sealed class EditorThemeColors
    {
        [SerializeField] private Color accent;
        [SerializeField] private Color accentText;
        [SerializeField] private Color background;
        [SerializeField] private Color border;
        [SerializeField] private Color card;
        [SerializeField] private Color danger;
        [SerializeField] private Color dimText;
        [SerializeField] private Color divider;
        [SerializeField] private Color field;
        [SerializeField] private Color focus;
        [SerializeField] private Color hover;
        [SerializeField] private Color keyCap;
        [SerializeField] private Color secondary;
        [SerializeField] private Color secondaryText;
        [SerializeField] private Color selection;
        [SerializeField] private Color selectionFill;
        [SerializeField] private Color separator;
        [SerializeField] private Color stripe;
        [SerializeField] private Color success;
        [SerializeField] private Color text;
        [SerializeField] private Color warning;

        /// <summary>Blue used for selection, drop targets and the primary button.</summary>
        public Color Accent => accent;

        /// <summary>Text drawn on top of <see cref="Accent"/>.</summary>
        public Color AccentText => accentText;

        /// <summary>Fill behind a whole window.</summary>
        public Color Background => background;

        /// <summary>Outline of a field or a card.</summary>
        public Color Border => border;

        /// <summary>Fill of a card or a grouped block.</summary>
        public Color Card => card;

        /// <summary>Red used for errors and destructive actions.</summary>
        public Color Danger => danger;

        /// <summary>Secondary text used for paths, counts and hints.</summary>
        public Color DimText => dimText;

        /// <summary>Draggable line between two columns.</summary>
        public Color Divider => divider;

        /// <summary>Fill of a text field or a search box.</summary>
        public Color Field => field;

        /// <summary>Amber used for pins, overrides and anything a window links to.</summary>
        public Color Focus => focus;

        /// <summary>Tint of the row under the mouse.</summary>
        public Color Hover => hover;

        /// <summary>Fill of a keyboard cap or a muted chip.</summary>
        public Color KeyCap => keyCap;

        /// <summary>Fill of a button that is not the primary action.</summary>
        public Color Secondary => secondary;

        /// <summary>Text drawn on top of <see cref="Secondary"/>.</summary>
        public Color SecondaryText => secondaryText;

        /// <summary>Outline of the selected row.</summary>
        public Color Selection => selection;

        /// <summary>Fill of the selected row.</summary>
        public Color SelectionFill => selectionFill;

        /// <summary>Hairline between two blocks of a window.</summary>
        public Color Separator => separator;

        /// <summary>Tint of every second row.</summary>
        public Color Stripe => stripe;

        /// <summary>Green used for a passed check or an empty problem list.</summary>
        public Color Success => success;

        /// <summary>Primary text color.</summary>
        public Color Text => text;

        /// <summary>Orange used for a warning that is not yet an error.</summary>
        public Color Warning => warning;

        /// <summary>Creates an empty set. Required by the serializer and by the inspector.</summary>
        public EditorThemeColors() { }

        /// <summary>Creates a full set of colors for one editor theme.</summary>
        /// <param name="accent">Blue used for selection, drop targets and the primary button.</param>
        /// <param name="accentText">Text drawn on top of the accent.</param>
        /// <param name="background">Fill behind a whole window.</param>
        /// <param name="border">Outline of a field or a card.</param>
        /// <param name="card">Fill of a card or a grouped block.</param>
        /// <param name="danger">Red used for errors and destructive actions.</param>
        /// <param name="dimText">Secondary text used for paths, counts and hints.</param>
        /// <param name="divider">Draggable line between two columns.</param>
        /// <param name="field">Fill of a text field or a search box.</param>
        /// <param name="focus">Amber used for pins, overrides and links.</param>
        /// <param name="hover">Tint of the row under the mouse.</param>
        /// <param name="keyCap">Fill of a keyboard cap or a muted chip.</param>
        /// <param name="secondary">Fill of a button that is not the primary action.</param>
        /// <param name="secondaryText">Text drawn on top of the secondary fill.</param>
        /// <param name="selection">Outline of the selected row.</param>
        /// <param name="selectionFill">Fill of the selected row.</param>
        /// <param name="separator">Hairline between two blocks of a window.</param>
        /// <param name="stripe">Tint of every second row.</param>
        /// <param name="success">Green used for a passed check.</param>
        /// <param name="text">Primary text color.</param>
        /// <param name="warning">Orange used for a warning that is not yet an error.</param>
        public EditorThemeColors(Color accent, Color accentText, Color background, Color border, Color card,
            Color danger, Color dimText, Color divider, Color field, Color focus, Color hover, Color keyCap,
            Color secondary, Color secondaryText, Color selection, Color selectionFill, Color separator,
            Color stripe, Color success, Color text, Color warning)
        {
            this.accent = accent;
            this.accentText = accentText;
            this.background = background;
            this.border = border;
            this.card = card;
            this.danger = danger;
            this.dimText = dimText;
            this.divider = divider;
            this.field = field;
            this.focus = focus;
            this.hover = hover;
            this.keyCap = keyCap;
            this.secondary = secondary;
            this.secondaryText = secondaryText;
            this.selection = selection;
            this.selectionFill = selectionFill;
            this.separator = separator;
            this.stripe = stripe;
            this.success = success;
            this.text = text;
            this.warning = warning;
        }

        /// <summary>
        /// Whether every color in this set is the same as in another.
        /// </summary>
        /// <remarks>
        /// Lets a theme say which preset it still matches, so the settings page can mark that preset
        /// rather than leave the user guessing what they are looking at.
        /// </remarks>
        /// <param name="other">The set to compare against.</param>
        /// <returns>True when every color matches.</returns>
        public bool Matches(EditorThemeColors other)
        {
            if (other == null)
                return false;

            return Accent == other.Accent
                && AccentText == other.AccentText
                && Background == other.Background
                && Border == other.Border
                && Card == other.Card
                && Danger == other.Danger
                && DimText == other.DimText
                && Divider == other.Divider
                && Field == other.Field
                && Focus == other.Focus
                && Hover == other.Hover
                && KeyCap == other.KeyCap
                && Secondary == other.Secondary
                && SecondaryText == other.SecondaryText
                && Selection == other.Selection
                && SelectionFill == other.SelectionFill
                && Separator == other.Separator
                && Stripe == other.Stripe
                && Success == other.Success
                && Text == other.Text
                && Warning == other.Warning;
        }
    }
}