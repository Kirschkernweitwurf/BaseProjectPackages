using UnityEngine.UIElements;

namespace Base.CorePackage.Editor.StateMachine
{
    /// <summary>
    /// The centered panel the window shows instead of a machine: nothing is running, or nothing is
    /// selected. A monitor is empty most of the time, so the empty state is the view most often seen.
    /// </summary>
    internal sealed class StateMachineMessageView : VisualElement
    {
        /// <summary>Glyph shown when nothing is running.</summary>
        internal const string IdleGlyph = "\u25cb";

        private readonly Label _glyph = new();
        private readonly Label _title = new();
        private readonly Label _body = new();

        /// <summary>Builds the panel.</summary>
        internal StateMachineMessageView()
        {
            AddToClassList(StateMachineStyle.EmptyClass);

            VisualElement ring = new();
            ring.AddToClassList(StateMachineStyle.EmptyRingClass);

            _glyph.AddToClassList(StateMachineStyle.EmptyGlyphClass);
            _title.AddToClassList(StateMachineStyle.EmptyTitleClass);
            _body.AddToClassList(StateMachineStyle.EmptyBodyClass);

            ring.Add(_glyph);

            Add(ring);
            Add(_title);
            Add(_body);
        }

        /// <summary>Shows a message.</summary>
        /// <param name="glyph">The symbol inside the ring.</param>
        /// <param name="title">The headline.</param>
        /// <param name="body">One or two lines of explanation.</param>
        internal void Show(string glyph, string title, string body)
        {
            _glyph.text = glyph;
            _title.text = title;
            _body.text = body;
        }
    }
}