using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindows
{
    /// <summary>
    /// Drag and drop of entries and groups. Holds the whole gesture: the armed state, the drop
    /// target search over the drawn rows, the ghost that follows the mouse and the move itself.
    /// A group cannot be dropped into itself, and no list of the read only shipped tree ever
    /// becomes a target.
    /// </summary>
    internal sealed class MenuDragController
    {
        private const float DragThreshold = 4f;
        private const string EntryFallbackLabel = "Entry";
        private const float GhostHeight = 20f;
        private const float GhostOffsetX = 12f;
        private const float GhostOffsetY = 4f;
        private const float GhostWidth = 220f;
        private const string GroupFallbackLabel = "Group";

        /// <summary>Whether a drag passed the threshold and is currently running.</summary>
        internal bool IsActive { get; private set; }

        /// <summary>Node being dragged, so a row can draw itself as the source.</summary>
        internal MenuNode Node { get; private set; }

        private readonly HashSet<List<MenuNode>> _forbidden = new();
        private readonly HashSet<List<MenuNode>> _lockedLists;
        private readonly IReadOnlyList<MenuRow> _rows;
        private readonly MenuUndoStack _undo;

        private List<MenuNode> _dropParent;
        private List<MenuNode> _sourceList;
        private Rect _dropLine;
        private Vector2 _start;
        private int _dropIndex;
        private bool _isArmed;
        private bool _isDropValid;
        private bool _isGroup;

        /// <summary>Creates a controller over the rows a window draws.</summary>
        /// <param name="undo">Stack a completed move pushes onto.</param>
        /// <param name="rows">Row list of the window, refilled every frame.</param>
        /// <param name="lockedLists">Node lists that belong to the read only tree.</param>
        public MenuDragController(MenuUndoStack undo, IReadOnlyList<MenuRow> rows,
            HashSet<List<MenuNode>> lockedLists)
        {
            _undo = undo ?? throw new ArgumentNullException(nameof(undo));
            _rows = rows ?? throw new ArgumentNullException(nameof(rows));
            _lockedLists = lockedLists ?? throw new ArgumentNullException(nameof(lockedLists));
        }

        /// <summary>Arms a drag on a single entry.</summary>
        /// <param name="current">Event that started the gesture.</param>
        /// <param name="node">The entry node under the mouse.</param>
        /// <param name="parent">List the node currently lives in.</param>
        internal void BeginEntry(Event current, MenuNode node, List<MenuNode> parent)
            => Arm(current, node, parent, false);

        /// <summary>Arms a drag on a group, together with everything below it.</summary>
        /// <param name="current">Event that started the gesture.</param>
        /// <param name="group">The group under the mouse.</param>
        /// <param name="parent">List the group currently lives in.</param>
        internal void BeginGroup(Event current, MenuGroupNode group, List<MenuNode> parent)
        {
            Arm(current, group, parent, true);
            CollectForbidden(group);
        }

        /// <summary>
        /// Advances the gesture for one event. Call this once per frame, after the rows were drawn
        /// and their rects are known.
        /// </summary>
        /// <param name="current">The event being processed.</param>
        /// <param name="root">List a drop below the last row falls back to.</param>
        /// <returns>What the window still has to do.</returns>
        internal EMenuDragOutcome Resolve(Event current, List<MenuNode> root)
        {
            if (!_isArmed)
                return EMenuDragOutcome.None;

            if (!IsActive
                && current.type == EventType.MouseDrag
                && Vector2.Distance(current.mousePosition, _start) > DragThreshold)
                IsActive = true;

            EMenuDragOutcome outcome = EMenuDragOutcome.None;

            if (IsActive)
            {
                ComputeDropTarget(current.mousePosition, root);

                if (current.type == EventType.Repaint)
                {
                    if (_isDropValid)
                        EditorGUI.DrawRect(_dropLine, MenuManagerTheme.AccentColor());

                    DrawGhost(current.mousePosition);
                }

                if (current.type == EventType.MouseDrag)
                {
                    current.Use();
                    outcome = EMenuDragOutcome.Repaint;
                }
            }

            if (current.type != EventType.MouseUp)
                return outcome;

            bool moved = false;

            if (IsActive)
            {
                moved = FinishDrag();
                current.Use();
            }

            Disarm();

            return moved
                ? EMenuDragOutcome.Moved
                : EMenuDragOutcome.Repaint;
        }

        private void Arm(Event current, MenuNode node, List<MenuNode> parent, bool isGroup)
        {
            _isArmed = true;
            IsActive = false;
            _isGroup = isGroup;
            Node = node;
            _sourceList = parent;
            _start = current.mousePosition;
            _forbidden.Clear();
        }

        private void Disarm()
        {
            _isArmed = false;
            IsActive = false;
            Node = null;
            _sourceList = null;
            _forbidden.Clear();
        }

        private void CollectForbidden(MenuGroupNode group)
        {
            _forbidden.Add(group.Children);

            foreach (MenuNode child in group.Children)
            {
                if (child is MenuGroupNode sub)
                    CollectForbidden(sub);
            }
        }

        private void ComputeDropTarget(Vector2 mouse, List<MenuNode> root)
        {
            _isDropValid = false;
            _dropParent = null;
            _dropIndex = 0;

            foreach (MenuRow row in _rows)
            {
                if (row.IsSectionHeader)
                    continue;

                if (mouse.y < row.Rect.yMin || mouse.y > row.Rect.yMax)
                    continue;

                if (row.IsDivider)
                {
                    SetTarget(row.ParentList, row.Index,
                        MenuManagerTheme.LineAt(row.Rect.center.y, row.Rect, row.Depth));

                    return;
                }

                bool topHalf = mouse.y < row.Rect.center.y;

                if (row.IsPlaceholder)
                {
                    SetTarget(row.ParentList, 0,
                        new Rect(row.Rect.x + 6f, row.Rect.center.y - 1f, row.Rect.width - 12f, 2f));

                    return;
                }

                if (row.IsGroup)
                {
                    if (topHalf)
                        SetTarget(row.ParentList, row.Index,
                            MenuManagerTheme.LineAt(row.Rect.yMin, row.Rect, row.Depth));
                    else if (row.Group.Expanded)
                        SetTarget(row.Group.Children, 0,
                            MenuManagerTheme.LineAt(row.Rect.yMax, row.Rect, row.Depth + 1));
                    else
                        SetTarget(row.ParentList, row.Index + 1,
                            MenuManagerTheme.LineAt(row.Rect.yMax, row.Rect, row.Depth));

                    return;
                }

                int index = topHalf
                    ? row.Index
                    : row.Index + 1;

                float y = topHalf
                    ? row.Rect.yMin
                    : row.Rect.yMax;

                SetTarget(row.ParentList, index, MenuManagerTheme.LineAt(y, row.Rect, row.Depth));
                return;
            }

            if (_rows.Count > 0 && mouse.y > _rows[^1].Rect.yMax)
                SetTarget(root, root.Count,
                    MenuManagerTheme.LineAt(_rows[^1].Rect.yMax, _rows[^1].Rect, 0));
        }

        private void SetTarget(List<MenuNode> parent, int index, Rect line)
        {
            if (_forbidden.Contains(parent) || _lockedLists.Contains(parent))
                return;

            _dropParent = parent;
            _dropIndex = index;
            _dropLine = line;
            _isDropValid = true;
        }

        private bool FinishDrag()
        {
            if (!_isDropValid
                || _dropParent == null
                || Node == null)
                return false;

            int sourceIndex = _sourceList.IndexOf(Node);

            if (sourceIndex < 0)
                return false;

            _undo.Push();
            _sourceList.RemoveAt(sourceIndex);
            int target = _dropIndex;

            if (_sourceList == _dropParent && sourceIndex < target)
                target--;

            target = Mathf.Clamp(target, 0, _dropParent.Count);
            _dropParent.Insert(target, Node);

            return true;
        }

        private void DrawGhost(Vector2 mouse)
        {
            string label = _isGroup
                ? ((MenuGroupNode)Node).Name
                : Node is MenuEntryNode entryNode
                    ? entryNode.Entry.Path
                    : EntryFallbackLabel;

            if (string.IsNullOrEmpty(label))
                label = _isGroup
                    ? GroupFallbackLabel
                    : EntryFallbackLabel;

            GUI.Box(new Rect(mouse.x + GhostOffsetX, mouse.y + GhostOffsetY, GhostWidth, GhostHeight), label,
                MenuManagerTheme.Ghost);
        }
    }
}