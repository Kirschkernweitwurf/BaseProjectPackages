using System.Collections.Generic;
using UnityEngine;

namespace Base.ToolPackage.Editor.CommandPalette
{
    /// <summary>
    /// The scrollable result list. Owns the matches, the selection and the scroll offset, and only
    /// draws the rows that are actually on screen so a few thousand results stay smooth.
    /// </summary>
    internal sealed class CommandResultList
    {
        private const string EmptyMessage = "Nothing matches. Try fewer letters.";
        private const int OverscanRows = 2;
        private const int PageStep = 10;
        private const int RightMouseButton = 1;

        private readonly List<CommandMatch> _matches = new();

        private Vector2 _scroll;
        private float _viewport;
        private int _selected;

        /// <summary>How many entries survived the current filter.</summary>
        internal int Count => _matches.Count;

        /// <summary>Whether there is an entry to act on.</summary>
        internal bool HasSelection => _matches.Count > 0;

        /// <summary>The entry the selection currently sits on.</summary>
        internal CommandEntry Selected => _matches[_selected].Entry;

        /// <summary>Replaces the results with everything that matches the filter.</summary>
        /// <param name="entries">Every known command.</param>
        /// <param name="filter">The parsed search box content.</param>
        /// <param name="projectOnly">Whether package and built-in commands are hidden.</param>
        internal void Fill(IReadOnlyList<CommandEntry> entries, CommandFilter filter, bool projectOnly)
        {
            CommandQuery.Run(entries, filter, projectOnly, _matches);

            _selected = Mathf.Clamp(_selected, 0, Mathf.Max(0, _matches.Count - 1));
        }

        /// <summary>Jumps back to the first result. Call when the query changes.</summary>
        public void Reset()
        {
            _selected = 0;
            _scroll = Vector2.zero;
        }

        /// <summary>Moves the selection and scrolls it back into view.</summary>
        /// <param name="action">The move that was requested.</param>
        internal void Move(ECommandPaletteAction action)
        {
            if (_matches.Count == 0)
                return;

            _selected = Mathf.Clamp(_selected + StepOf(action), 0, _matches.Count - 1);

            EnsureVisible();
        }

        /// <summary>Draws the list and reports what the mouse did.</summary>
        /// <param name="area">The area the list may use.</param>
        /// <param name="term">Lowercase search term, used to pick out the matched characters.</param>
        /// <returns>The action the mouse asked for.</returns>
        internal ECommandPaletteAction Draw(Rect area, string term)
        {
            _viewport = area.height;

            if (_matches.Count == 0)
            {
                DrawEmpty(area);
                return ECommandPaletteAction.None;
            }

            Rect content = ContentRect(area);
            _scroll = GUI.BeginScrollView(area, _scroll, content);

            ECommandPaletteAction action = DrawRows(content, term);

            GUI.EndScrollView();

            return action;
        }

        private static void DrawEmpty(Rect area)
            => GUI.Label(area, EmptyMessage, CommandPaletteStyles.EmptyLabel);

        private static int StepOf(ECommandPaletteAction action) => action switch
        {
            ECommandPaletteAction.MoveDown => 1,
            ECommandPaletteAction.MoveUp => -1,
            ECommandPaletteAction.PageDown => PageStep,
            ECommandPaletteAction.PageUp => -PageStep,
            _ => 0
        };

        private Rect ContentRect(Rect area)
        {
            float height = _matches.Count * CommandPaletteStyles.RowHeight;

            float width = height > area.height
                ? area.width - CommandPaletteStyles.ScrollbarWidth
                : area.width;

            return new Rect(0f, 0f, width, height);
        }

        private ECommandPaletteAction DrawRows(Rect content, string term)
        {
            float rowHeight = CommandPaletteStyles.RowHeight;
            int first = Mathf.Max(0, Mathf.FloorToInt(_scroll.y / rowHeight) - 1);
            int last = Mathf.Min(_matches.Count, first + Mathf.CeilToInt(_viewport / rowHeight) + OverscanRows);

            Vector2 mouse = Event.current.mousePosition;
            ECommandPaletteAction action = ECommandPaletteAction.None;

            for (int i = first; i < last; i++)
            {
                Rect row = new(content.x, i * rowHeight, content.width, rowHeight);
                CommandMatch match = _matches[i];

                CommandRowDrawer.Draw(row, match, i == _selected, row.Contains(mouse), term,
                    CommandTagStore.instance.TagsFor(match.Entry.Id));

                ECommandPaletteAction clicked = ReadClick(row, i);

                if (clicked != ECommandPaletteAction.None)
                    action = clicked;
            }

            return action;
        }

        private void EnsureVisible()
        {
            float rowHeight = CommandPaletteStyles.RowHeight;
            float top = _selected * rowHeight;

            if (top < _scroll.y)
                _scroll.y = top;
            else if (top + rowHeight > _scroll.y + _viewport)
                _scroll.y = top + rowHeight - _viewport;
        }

        private ECommandPaletteAction ReadClick(Rect row, int index)
        {
            Event current = Event.current;

            if (current.type != EventType.MouseDown || !row.Contains(current.mousePosition))
                return ECommandPaletteAction.None;

            _selected = index;
            current.Use();

            return current.button == RightMouseButton
                ? ECommandPaletteAction.ShowMenu
                : ECommandPaletteAction.Run;
        }
    }
}