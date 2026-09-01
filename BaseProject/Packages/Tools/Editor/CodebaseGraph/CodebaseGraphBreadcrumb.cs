using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// The path bar under the toolbar. It sits on its own row rather than among the toolbar buttons,
    /// because reading where you are and reaching for a control are two different jobs and they were
    /// competing for the same strip of pixels.
    /// <br/><br/>
    /// The right hand side carries the controls that only mean anything while the view is narrowed to
    /// one entry. They appear when that happens and are gone the rest of the time, instead of sitting
    /// greyed out in the toolbar with nothing to say for themselves.
    /// </summary>
    internal sealed class CodebaseGraphBreadcrumb : VisualElement
    {
        private const string SeparatorText = "\u203a";

        private readonly Action<int> _onSegmentClicked;
        private readonly VisualElement _pathBar;
        private readonly VisualElement _focusBar;
        private readonly Label _notice;

        /// <summary>Builds an empty path bar.</summary>
        /// <param name="onSegmentClicked">Raised with the index of the segment that was clicked.</param>
        public CodebaseGraphBreadcrumb(Action<int> onSegmentClicked)
        {
            _onSegmentClicked = onSegmentClicked;
            AddToClassList(CodebaseGraphStyle.BreadcrumbBarClass);

            _pathBar = new VisualElement();
            _pathBar.AddToClassList(CodebaseGraphStyle.BreadcrumbPathClass);
            Add(_pathBar);

            _focusBar = new VisualElement();
            _focusBar.AddToClassList(CodebaseGraphStyle.BreadcrumbFocusClass);
            Add(_focusBar);

            _notice = new Label(string.Empty);
            _notice.AddToClassList(CodebaseGraphStyle.BreadcrumbNoticeClass);
            _focusBar.Add(_notice);

            SetFocus(string.Empty);
        }

        /// <summary>Replaces the shown path. The last segment is where the view currently is.</summary>
        /// <param name="segments">Path segments, from the root inward.</param>
        internal void SetPath(IReadOnlyList<string> segments)
        {
            _pathBar.Clear();

            for (int index = 0; index < segments.Count; index++)
            {
                if (index > 0)
                    _pathBar.Add(BuildSeparator());

                _pathBar.Add(index == segments.Count - 1
                    ? BuildCurrent(segments[index])
                    : BuildLink(segments[index], index));
            }
        }

        /// <summary>Adds a control that is only shown while the view is narrowed to one entry.</summary>
        /// <param name="control">Control to place in the focus bar.</param>
        internal void AddFocusControl(VisualElement control) => _focusBar.Add(control);

        /// <summary>Shows or hides the focus bar and sets the line explaining what is being shown.</summary>
        /// <param name="notice">Explanation of the narrowed view, or an empty string to hide the bar.</param>
        internal void SetFocus(string notice)
        {
            bool isVisible = !string.IsNullOrEmpty(notice);

            _notice.text = notice;
            _focusBar.style.display = isVisible
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }

        private static Label BuildSeparator()
        {
            Label separator = new(SeparatorText);
            separator.AddToClassList(CodebaseGraphStyle.BreadcrumbSeparatorClass);
            return separator;
        }

        private static Label BuildCurrent(string text)
        {
            Label current = new(text);
            current.AddToClassList(CodebaseGraphStyle.BreadcrumbCurrentClass);
            return current;
        }

        private Button BuildLink(string text, int index)
        {
            Button link = new(() => _onSegmentClicked?.Invoke(index))
            {
                text = text
            };

            link.AddToClassList(CodebaseGraphStyle.BreadcrumbSegmentClass);
            return link;
        }
    }
}