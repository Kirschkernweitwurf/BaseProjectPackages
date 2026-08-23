using System;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.AudioRules.Window
{
    /// <summary>
    /// The centered panel the window shows instead of a table: no rule set yet, nothing found, or
    /// everything clean. The clean state is the one that matters. Finishing a pass over a few
    /// thousand clips deserves something better than an empty list.
    /// </summary>
    internal sealed class AudioRulesMessageView : VisualElement
    {
        private const string BodyClass = "ar-empty__body";
        private const string ButtonClass = "ar-primary";
        private const string GlyphClass = "ar-empty__glyph";
        private const string GoodClass = "ar-good";
        private const string NoteGlyphClass = "ar-empty__glyph--note";
        private const string RingClass = "ar-empty__ring";
        private const string RootClass = "ar-empty";
        private const string TitleClass = "ar-empty__title";
        private const string WarnClass = "ar-warn";

        /// <summary>Glyph shown when there is nothing to work with yet.</summary>
        public const string NeutralGlyph = "\u266a";

        /// <summary>Glyph shown when everything matches its rules.</summary>
        public const string SuccessGlyph = "\u2713";

        private readonly VisualElement _ring = new();
        private readonly Label _glyph = new();
        private readonly Label _title = new();
        private readonly Label _body = new();
        private readonly Button _action;

        private Action _onClick;

        /// <summary>Builds the panel.</summary>
        public AudioRulesMessageView()
        {
            AddToClassList(RootClass);

            _ring.AddToClassList(RingClass);
            _glyph.AddToClassList(GlyphClass);
            _title.AddToClassList(TitleClass);
            _body.AddToClassList(BodyClass);

            _ring.Add(_glyph);

            _action = new Button(() => _onClick?.Invoke());
            _action.AddToClassList(ButtonClass);

            Add(_ring);
            Add(_title);
            Add(_body);
            Add(_action);
        }

        /// <summary>Shows a message, with a call to action when one is given.</summary>
        /// <param name="glyph">The symbol inside the ring.</param>
        /// <param name="title">The headline.</param>
        /// <param name="body">One or two lines of explanation.</param>
        /// <param name="variant">Extra class controlling the color, or null.</param>
        /// <param name="buttonText">Label of the button, or null to hide it.</param>
        /// <param name="onClick">What the button does.</param>
        public void Show(string glyph, string title, string body, string variant, string buttonText,
            Action onClick)
        {
            ClearVariants();

            if (!string.IsNullOrEmpty(variant))
            {
                _ring.AddToClassList(variant);
                _glyph.AddToClassList(variant);
            }

            // The note glyph sits low and left in its em box, so centering the label is not enough.
            _glyph.EnableInClassList(NoteGlyphClass, glyph == NeutralGlyph);

            _glyph.text = glyph;
            _title.text = title;
            _body.text = body;
            _onClick = onClick;

            bool hasButton = !string.IsNullOrEmpty(buttonText);

            _action.text = buttonText;
            _action.style.display = hasButton
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }

        private void ClearVariants()
        {
            _ring.RemoveFromClassList(GoodClass);
            _ring.RemoveFromClassList(WarnClass);
            _glyph.RemoveFromClassList(GoodClass);
            _glyph.RemoveFromClassList(WarnClass);
        }
    }
}