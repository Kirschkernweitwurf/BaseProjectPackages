using UnityEngine.UIElements;

namespace Base.CorePackage.Editor.StateMachine
{
    /// <summary>
    /// A titled pane. Three of these sit in the split views, and the header is what makes the window read
    /// as three areas rather than as controls stacked on a gray background.
    /// </summary>
    internal sealed class StateMachinePane : VisualElement
    {
        /// <summary>Where the content of the pane goes.</summary>
        internal VisualElement Body { get; } = new();

        /// <summary>The row of the header right of the title, for chips.</summary>
        internal VisualElement HeaderRight { get; } = new();

        private readonly Label _note = new();
        private readonly Label _title;

        /// <summary>Builds a pane.</summary>
        /// <param name="title">The headline shown in the header.</param>
        internal StateMachinePane(string title)
        {
            AddToClassList(StateMachineStyle.PaneClass);

            VisualElement header = new();
            header.AddToClassList(StateMachineStyle.PaneHeaderClass);

            _title = new Label(title);
            _title.AddToClassList(StateMachineStyle.PaneTitleClass);

            _note.AddToClassList(StateMachineStyle.PaneNoteClass);

            HeaderRight.style.flexDirection = FlexDirection.Row;
            HeaderRight.style.flexGrow = 1f;
            HeaderRight.style.justifyContent = Justify.FlexEnd;

            header.Add(_title);
            header.Add(_note);
            header.Add(HeaderRight);

            Body.AddToClassList(StateMachineStyle.PaneBodyClass);

            Add(header);
            Add(Body);
        }

        /// <summary>Changes the headline.</summary>
        /// <param name="title">The new headline.</param>
        internal void SetTitle(string title) => _title.text = title;

        /// <summary>Sets the quiet line next to the title, used for counts.</summary>
        /// <param name="text">The note, or an empty string to hide it.</param>
        internal void SetNote(string text) => _note.text = text;
    }
}