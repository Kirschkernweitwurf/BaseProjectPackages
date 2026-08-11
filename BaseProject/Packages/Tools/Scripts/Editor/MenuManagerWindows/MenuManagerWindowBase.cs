using System;
using System.Collections.Generic;
using Base.UtilityPackage.Logging;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindows
{
    /// <summary>
    /// Shared window logic. Shows the shipped package tree read only and the project
    /// overlay tree editable.
    /// <para/>
    /// The window draws and runs the toolbar commands. <see cref="MenuRowBuilder"/> flattens the
    /// trees into rows, <see cref="MenuDragController"/> owns the drag gesture,
    /// <see cref="MenuUndoStack"/> owns the history and <see cref="MenuManagerTheme"/> owns every
    /// color and style.
    /// </summary>
    internal abstract class MenuManagerWindowBase : EditorWindow
    {
        protected const int MenuPriority = 0;
        private const float DividerHeight = 8f;
        private const double FocusSeconds = 4d;
        private const float FoldWidth = 14f;
        private const float GripWidth = 18f;
        private const float HeaderHeight = 24f;
        private const int NoSplitter = -1;
        private const float Pad = 4f;
        private const float RowHeight = 22f;
        private const float SplitterWidth = 6f;
        private const float ToggleWidth = 18f;

        private static readonly GUIContent AutoContent = new("A", "Reset to automatic priority");
        private static readonly GUIContent AutoGroupContent = new("Auto Group",
            "Rebuilds groups from each entry's default path and shortens the entry to its last segment.");
        private static readonly GUIContent CleanContent =
            new("Clean Missing", "Removes every entry whose code no longer exists.");
        private static readonly GUIContent DividerContent = new(string.Empty, "Click to toggle a separator line here.");
        private static readonly GUIContent OpenContent = new("Open", "Open the script that defines this entry.");
        private static readonly GUIContent OverrideContent = new("M", "Override priority manually");
        private static readonly GUIContent ReloadContent = new("Reload",
            "Forces a script reload so Unity rebuilds the whole menu from scratch. "
            + "Use this when the layout looks stale.");
        private static readonly GUIContent RemoveContent = new("Remove",
            "The code behind this entry no longer exists. Click to drop it from the list.");
        private static readonly GUIContent SortContent =
            new("Sort A-Z", "Sorts groups and entries by name at every level.");

        /// <summary>Kind of entries this window manages.</summary>
        protected abstract EMenuEntryKind Kind { get; }

        /// <summary>Whether to show the asset file name column.</summary>
        protected virtual bool ShowFileName => false;

        private bool PackageLocked => _registry != null && _registry.IsReadOnly;

        private List<MenuNode> WritableRoot => PackageLocked
            ? _overlay.RootFor(Kind)
            : _registry.RootFor(Kind);

        private readonly HashSet<List<MenuNode>> _lockedLists = new();
        private readonly List<MenuRow> _rows = new();

        private Dictionary<string, ResolvedMenu> _resolved = new();
        private MenuDragController _drag;
        private MenuUndoStack _undo;
        private MenuRegistry _registry;
        private MenuOverlay _overlay;
        private Action _pending;
        private Vector2 _scroll;
        private string _hoverPreview = string.Empty;
        private int _activeSplitter = NoSplitter;

        private string _focusId;
        private double _focusUntil;
        private bool _focusScrollPending;

#region Unity Callbacks
        private void OnEnable()
        {
            _registry = MenuRegistry.Instance;
            _overlay = MenuOverlay.instance;
            _undo = new MenuUndoStack(CaptureState, ApplyState);
            _drag = new MenuDragController(_undo, _rows, _lockedLists);
            wantsMouseMove = true;
            RefreshScan();
        }

        private void OnGUI()
        {
            if (_registry == null)
                return;

            Event current = Event.current;

            if (current.type == EventType.MouseMove)
                Repaint();

            _undo.HandleCommands(current);
            ExpireFocus();
            _hoverPreview = string.Empty;

            DrawToolbar();

            if (PackageLocked)
                EditorGUILayout.HelpBox(
                    "The shipped layout is read only. Add and arrange your own entries under Project.",
                    MessageType.Info);

            MenuComposite.Recalculate();
            DrawColumnHeader(current);

            MenuRowBuilder.Build(_registry, _overlay, Kind, _rows, _lockedLists);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            foreach (MenuRow row in _rows)
                DrawRow(row, current);

            GUILayout.Space(6f);
            DrawFooter();

            ResolveDrag(current);
            ScrollToFocus(current);

            EditorGUILayout.EndScrollView();

            DrawStatusBar();

            if (_pending == null)
                return;

            _pending.Invoke();
            _pending = null;
            Persist();
            Repaint();
        }
#endregion

        /// <summary>
        /// Highlights the entry with the given id, expands every group above it and scrolls it
        /// into view. Used by the overview windows to link straight to a single entry.
        /// </summary>
        /// <param name="entryId">Stable id of the entry to focus.</param>
        public void FocusEntry(string entryId)
        {
            if (string.IsNullOrEmpty(entryId))
                return;

            if (_registry == null)
                _registry = MenuRegistry.Instance;

            if (_overlay == null)
                _overlay = MenuOverlay.instance;

            _focusId = entryId;
            _focusUntil = EditorApplication.timeSinceStartup + FocusSeconds;
            _focusScrollPending = true;

            // A hit in the shipped tree is invisible while that section is folded away.
            if (MenuTree.Expand(_registry.RootFor(Kind), entryId))
                _overlay.ShippedCollapsed = false;
            else
                MenuTree.Expand(_overlay.RootFor(Kind), entryId);

            Persist();
            Focus();
            Repaint();
        }

        private void RefreshScan()
        {
            _resolved = MenuScanner.Scan();
            MenuComposite.Sync(_resolved);
        }

        private void Persist()
        {
            _registry.Persist();
            _overlay.Persist();
        }

        private MenuUndoState CaptureState() => new()
        {
            Package = MenuNodeCloner.CloneNodes(_registry.RootFor(Kind)),
            Overlay = MenuNodeCloner.CloneNodes(_overlay.RootFor(Kind)),
            Start = _registry.StartFor(Kind)
        };

        private void ApplyState(MenuUndoState state)
        {
            GUIUtility.keyboardControl = 0;
            EditorGUIUtility.editingTextField = false;

            List<MenuNode> package = _registry.RootFor(Kind);
            package.Clear();
            package.AddRange(MenuNodeCloner.CloneNodes(state.Package));

            List<MenuNode> overlayRoot = _overlay.RootFor(Kind);
            overlayRoot.Clear();
            overlayRoot.AddRange(MenuNodeCloner.CloneNodes(state.Overlay));

            _registry.SetStart(Kind, state.Start);

            MenuComposite.Recalculate();
            Persist();
            Repaint();
        }

        private void ResolveDrag(Event current)
        {
            EMenuDragOutcome outcome = _drag.Resolve(current, WritableRoot);

            if (outcome == EMenuDragOutcome.None)
                return;

            if (outcome == EMenuDragOutcome.Moved)
                Persist();

            Repaint();
        }

        private bool IsFocused(MenuEntry entry) => _focusId != null
            && entry != null
            && entry.Id == _focusId;

        private void ExpireFocus()
        {
            if (_focusId == null)
                return;

            if (EditorApplication.timeSinceStartup > _focusUntil)
            {
                _focusId = null;
                _focusScrollPending = false;
            }

            Repaint(); // Keep the highlight animating down to its own expiry.
        }

        private void ScrollToFocus(Event current)
        {
            if (!_focusScrollPending
                || _focusId == null
                || current.type != EventType.Repaint)
                return;

            foreach (MenuRow row in _rows)
            {
                if (!IsFocused(row.Entry))
                    continue;

                _scroll.y = Mathf.Max(0f, row.Rect.y - position.height * 0.35f);
                _focusScrollPending = false;
                Repaint();
                return;
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70f)))
                RefreshScan();

            if (GUILayout.Button("Apply", EditorStyles.toolbarButton, GUILayout.Width(70f)))
            {
                MenuApplier.Apply(true);
                RefreshScan();
            }

            if (GUILayout.Button(ReloadContent, EditorStyles.toolbarButton, GUILayout.Width(62f)))
            {
                Persist();
                EditorUtility.RequestScriptReload();
            }

            using (new EditorGUI.DisabledScope(!_undo.CanUndo))
            {
                if (GUILayout.Button("Undo", EditorStyles.toolbarButton, GUILayout.Width(56f)))
                    _undo.Undo();
            }

            using (new EditorGUI.DisabledScope(!_undo.CanRedo))
            {
                if (GUILayout.Button("Redo", EditorStyles.toolbarButton, GUILayout.Width(56f)))
                    _undo.Redo();
            }

            using (new EditorGUI.DisabledScope(!MenuTree.HasMissing(WritableRoot)
                       && !MenuTree.HasEmptyGroup(WritableRoot)))
            {
                if (GUILayout.Button(CleanContent, EditorStyles.toolbarButton, GUILayout.Width(90f)))
                    _pending = CleanMissing;
            }

            using (new EditorGUI.DisabledScope(WritableRoot.Count == 0))
            {
                if (GUILayout.Button(AutoGroupContent, EditorStyles.toolbarButton, GUILayout.Width(80f)))
                    _pending = AutoGroup;

                if (GUILayout.Button(SortContent, EditorStyles.toolbarButton, GUILayout.Width(66f)))
                    _pending = SortNodes;
            }

            GUILayout.FlexibleSpace();

            EditorGUI.BeginChangeCheck();

            int newStart;

            using (new EditorGUI.DisabledScope(PackageLocked))
            {
                EditorGUILayout.LabelField("Start", GUILayout.Width(34f));
                newStart = EditorGUILayout.IntField(_registry.StartFor(Kind), EditorStyles.toolbarTextField,
                    GUILayout.Width(50f));
            }

            if (EditorGUI.EndChangeCheck())
            {
                _undo.Push();
                _registry.SetStart(Kind, newStart);
                Persist();
            }

            EditorGUILayout.EndHorizontal();

            string hint = ShowFileName
                ? "Group names build the path. Assets/Create is added automatically. "
                + "Click the space between two rows to add a separator line."
                : "Group names build the path. Click the space between two rows to add a separator line.";

            EditorGUILayout.LabelField(hint, EditorStyles.miniLabel);
        }

        private void DrawColumnHeader(Event current)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 18f, GUILayout.ExpandWidth(true));

            float statusW = _registry.ColumnStatusWidth;
            float priorityW = _registry.ColumnPriorityWidth;
            float fileW = _registry.ColumnFileWidth;

            float statusX = rect.xMax - statusW;
            float priorityX = statusX - Pad - priorityW;
            float fileX = ShowFileName
                ? priorityX - Pad - fileW
                : priorityX;

            float h = EditorGUIUtility.singleLineHeight;
            float y = rect.y + (rect.height - h) * 0.5f;
            float pathStart = rect.x + GripWidth + Pad + ToggleWidth + Pad;
            float pathEnd = ShowFileName
                ? fileX - Pad
                : priorityX - Pad;

            EditorGUI.LabelField(new Rect(pathStart, y, Mathf.Max(30f, pathEnd - pathStart), h), "Path",
                MenuManagerTheme.Column);

            if (ShowFileName)
                EditorGUI.LabelField(new Rect(fileX, y, fileW, h), "File Name", MenuManagerTheme.Column);

            EditorGUI.LabelField(new Rect(priorityX, y, priorityW, h), "Prio", MenuManagerTheme.Column);
            EditorGUI.LabelField(new Rect(statusX, y, statusW, h), "State", MenuManagerTheme.Column);

            DrawSplitter(rect, statusX, 2, current);
            DrawSplitter(rect, priorityX, 1, current);

            if (ShowFileName)
                DrawSplitter(rect, fileX, 0, current);

            if (_activeSplitter == NoSplitter)
                return;

            if (current.type == EventType.MouseDrag)
            {
                float mouseX = current.mousePosition.x;

                if (_activeSplitter == 2)
                    _registry.ColumnStatusWidth = rect.xMax - mouseX;
                else if (_activeSplitter == 1)
                    _registry.ColumnPriorityWidth = statusX - mouseX;
                else
                    _registry.ColumnFileWidth = priorityX - mouseX;

                current.Use();
                Repaint();
            }
            else if (current.type == EventType.MouseUp)
            {
                Persist();
                _activeSplitter = NoSplitter;
            }
        }

        private void DrawSplitter(Rect header, float x, int id, Event current)
        {
            Rect handle = new(x - SplitterWidth * 0.5f, header.y, SplitterWidth, header.height);
            EditorGUIUtility.AddCursorRect(handle, MouseCursor.ResizeHorizontal);

            if (current.type == EventType.Repaint)
                EditorGUI.DrawRect(new Rect(x - 0.5f, header.y, 1f, header.height), MenuManagerTheme.GuideColor());

            if (current.type == EventType.MouseDown
                && current.button == 0
                && handle.Contains(current.mousePosition))
            {
                _activeSplitter = id;
                current.Use();
            }
        }

        private void DrawRow(MenuRow row, Event current)
        {
            if (row.IsSectionHeader)
            {
                DrawSectionHeader(row, current);
                return;
            }

            if (row.IsDivider)
            {
                DrawDivider(row, current);
                return;
            }

            float height = row.IsGroup
                ? HeaderHeight
                : RowHeight;

            Rect full = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));
            row.Rect = full;

            if (current.type == EventType.Repaint)
            {
                MenuManagerTheme.DrawGuides(full, row.Depth);

                if (row.Locked)
                    EditorGUI.DrawRect(full, MenuManagerTheme.LockedColor());
            }

            float indent = row.Depth * MenuManagerTheme.Indent;
            Rect content = new(full.x + indent, full.y, full.width - indent, full.height);

            if (row.IsPlaceholder)
            {
                EditorGUI.LabelField(content, row.Locked
                    ? "No shipped entries"
                    : "Drop entries here", EditorStyles.centeredGreyMiniLabel);

                return;
            }

            if (row.IsGroup)
                DrawGroupRow(content, row, current);
            else
                DrawEntryRow(full, row, current);
        }

        private void DrawSectionHeader(MenuRow row, Event current)
        {
            Rect full = GUILayoutUtility.GetRect(0f, HeaderHeight, GUILayout.ExpandWidth(true));
            row.Rect = full;

            if (current.type == EventType.Repaint)
                EditorGUI.DrawRect(full, MenuManagerTheme.SectionColor());

            float h = EditorGUIUtility.singleLineHeight;
            float y = full.y + (full.height - h) * 0.5f;
            float x = full.x + 4f;

            if (row.Collapsible)
            {
                bool expanded = !_overlay.ShippedCollapsed;
                bool now = EditorGUI.Foldout(new Rect(x, y, FoldWidth, h), expanded, GUIContent.none);

                if (now != expanded)
                {
                    _overlay.ShippedCollapsed = !now;
                    _overlay.Persist();
                }

                x += FoldWidth + 2f;
            }

            EditorGUI.LabelField(new Rect(x, y, full.xMax - x - 4f, h), row.Header, EditorStyles.boldLabel);
        }

        private void DrawDivider(MenuRow row, Event current)
        {
            if (row.Node == null)
                return;

            Rect full = GUILayoutUtility.GetRect(0f, DividerHeight, GUILayout.ExpandWidth(true));
            row.Rect = full;

            float inset = full.x + row.Depth * MenuManagerTheme.Indent + 6f;
            Rect line = new(inset, full.center.y - 1f, full.xMax - inset - 6f, 2f);
            bool hover = !_drag.IsActive && full.Contains(current.mousePosition);

            if (current.type == EventType.Repaint)
            {
                MenuManagerTheme.DrawGuides(full, row.Depth);

                if (row.Node.Separator)
                    EditorGUI.DrawRect(line, MenuManagerTheme.SeparatorColor());
                else if (hover && !row.Locked)
                    EditorGUI.DrawRect(line, MenuManagerTheme.SeparatorHintColor());
            }

            if (row.Locked)
                return;

            GUI.Label(full, DividerContent);
            EditorGUIUtility.AddCursorRect(full, MouseCursor.Link);

            if (current.type != EventType.MouseDown
                || current.button != 0
                || !full.Contains(current.mousePosition))
                return;

            MenuNode node = row.Node;

            _pending = () =>
            {
                _undo.Push();
                node.Separator = !node.Separator;
            };

            current.Use();
        }

        private void DrawGroupRow(Rect content, MenuRow row, Event current)
        {
            MenuGroupNode group = row.Group;
            bool locked = row.Locked;
            bool isSource = _drag.IsActive && _drag.Node == group;

            if (current.type == EventType.Repaint)
                EditorGUI.DrawRect(content, MenuManagerTheme.HeaderColor(isSource));

            float h = EditorGUIUtility.singleLineHeight;
            float y = content.y + (content.height - h) * 0.5f;
            float x = content.x + 2f;

            bool expanded = EditorGUI.Foldout(new Rect(x, y, FoldWidth, h), group.Expanded, GUIContent.none);

            if (expanded != group.Expanded)
            {
                group.Expanded = expanded;
                Persist();
            }

            x += FoldWidth;
            Rect grip = new(x, content.y, GripWidth, content.height);
            DrawGrip(grip, current, onPress: () => _drag.BeginGroup(current, group, row.ParentList), locked);
            x += GripWidth + Pad;

            float addWidth = 54f;
            float deleteWidth = 54f;
            Rect deleteRect = new(content.xMax - deleteWidth - 2f, y, deleteWidth, h);
            Rect addRect = new(deleteRect.x - addWidth - Pad, y, addWidth, h);
            Rect nameRect = new(x, y, Mathf.Max(40f, addRect.x - x - Pad), h);

            string newName;

            using (new EditorGUI.DisabledScope(locked))
                newName = EditorGUI.DelayedTextField(nameRect, group.Name, MenuManagerTheme.Title);

            if (newName != group.Name)
            {
                _undo.Push();
                group.Name = newName;
                Persist();
            }

            using (new EditorGUI.DisabledScope(locked))
            {
                if (GUI.Button(addRect, "+ Group", EditorStyles.miniButton))
                {
                    MenuGroupNode captured = group;
                    _pending = () =>
                    {
                        _undo.Push();
                        captured.Expanded = true;
                        captured.Children.Add(new MenuGroupNode("New Group"));
                    };
                }
            }

            using (new EditorGUI.DisabledScope(locked || group.Children.Count != 0))
            {
                if (GUI.Button(deleteRect, "Delete", EditorStyles.miniButton))
                {
                    List<MenuNode> parent = row.ParentList;
                    MenuGroupNode captured = group;
                    _pending = () =>
                    {
                        _undo.Push();
                        parent.Remove(captured);
                    };
                }
            }
        }

        private void DrawEntryRow(Rect full, MenuRow row, Event current)
        {
            MenuEntry entry = row.Entry;
            bool locked = row.Locked;
            bool isSource = _drag.IsActive && _drag.Node == row.Node;

            if (current.type == EventType.Repaint && (row.Index & 1) == 1)
                EditorGUI.DrawRect(full, MenuManagerTheme.RowStripeColor());

            if (current.type == EventType.Repaint && isSource)
                EditorGUI.DrawRect(full, MenuManagerTheme.SelectionColor());

            if (current.type == EventType.Repaint && IsFocused(entry))
                EditorGUI.DrawRect(full, MenuManagerTheme.FocusColor());

            if (current.type == EventType.Repaint
                && !_drag.IsActive
                && full.Contains(current.mousePosition))
                _hoverPreview = row.FullPath;

            MenuRowColumns columns = ComputeColumns(full, row.Depth);
            DrawGrip(columns.Grip, current, onPress: () => _drag.BeginEntry(current, row.Node, row.ParentList),
                locked);

            bool enabled;

            using (new EditorGUI.DisabledScope(locked))
                enabled = EditorGUI.Toggle(columns.Toggle, entry.Enabled);

            if (enabled != entry.Enabled)
            {
                _undo.Push();
                entry.Enabled = enabled;
                Persist();
            }

            using (new EditorGUI.DisabledScope(entry.Missing || locked))
            {
                string path = EditorGUI.DelayedTextField(columns.Path, entry.Path);

                if (path != entry.Path)
                {
                    _undo.Push();
                    entry.Path = path;
                    Persist();
                }

                if (ShowFileName)
                {
                    string file = EditorGUI.DelayedTextField(columns.File, entry.CreateFileName);

                    if (file != entry.CreateFileName)
                    {
                        _undo.Push();
                        entry.CreateFileName = file;
                        Persist();
                    }
                }
            }

            DrawPriorityCell(columns.Priority, entry, locked);
            DrawStatusCell(columns.Status, row);
        }

        private MenuRowColumns ComputeColumns(Rect full, int depth)
        {
            float h = EditorGUIUtility.singleLineHeight;
            float y = full.y + (full.height - h) * 0.5f;
            float left = full.x + depth * MenuManagerTheme.Indent;

            Rect grip = new(left, y, GripWidth, h);
            float x = left + GripWidth + Pad;

            Rect toggle = new(x, y, ToggleWidth, h);
            x += ToggleWidth + Pad;

            float statusW = _registry.ColumnStatusWidth;
            float priorityW = _registry.ColumnPriorityWidth;
            float fileW = _registry.ColumnFileWidth;

            float statusX = full.xMax - statusW;
            float priorityX = statusX - Pad - priorityW;
            float fileX = ShowFileName
                ? priorityX - Pad - fileW
                : priorityX;

            float pathEnd = ShowFileName
                ? fileX - Pad
                : priorityX - Pad;

            float pathWidth = Mathf.Max(60f, pathEnd - x);

            Rect path = new(x, y, pathWidth, h);
            Rect file = new(fileX, y, fileW, h);
            Rect priority = new(priorityX, y, priorityW, h);
            Rect status = new(statusX, y, statusW, h);

            return new MenuRowColumns
            {
                Grip = grip,
                Toggle = toggle,
                Path = path,
                File = file,
                Priority = priority,
                Status = status
            };
        }

        private void DrawStatusCell(Rect cell, MenuRow row)
        {
            if (!row.Entry.Missing)
            {
                bool canOpen = _resolved.ContainsKey(row.Entry.Id);

                using (new EditorGUI.DisabledScope(!canOpen))
                {
                    if (GUI.Button(cell, OpenContent, EditorStyles.miniButton))
                        OpenScript(row.Entry);
                }

                return;
            }

            if (row.Locked)
            {
                EditorGUI.LabelField(cell, "missing", EditorStyles.miniLabel);
                return;
            }

            if (!GUI.Button(cell, RemoveContent, EditorStyles.miniButton))
                return;

            List<MenuNode> parent = row.ParentList;
            MenuNode node = row.Node;

            _pending = () =>
            {
                _undo.Push();
                parent.Remove(node);
            };
        }

        private void OpenScript(MenuEntry entry)
        {
            if (!_resolved.TryGetValue(entry.Id, out ResolvedMenu match))
                return;

            if (!MenuTree.OpenScript(match.DeclaringType))
                CustomLogger.LogWarning($"Menu Manager: could not locate the script for '{entry.Path}'.", null);
        }

        private void DrawPriorityCell(Rect cell, MenuEntry entry, bool locked)
        {
            const float buttonWidth = 18f;
            Rect valueRect = new(cell.x, cell.y, Mathf.Max(18f, cell.width - buttonWidth - 2f), cell.height);
            Rect buttonRect = new(valueRect.xMax + 2f, cell.y, buttonWidth, cell.height);

            using EditorGUI.DisabledScope disabled = new(locked);

            if (entry.OverridePriority)
            {
                if (Event.current.type == EventType.Repaint)
                    EditorGUI.DrawRect(valueRect, MenuManagerTheme.OverrideColor());

                int value = EditorGUI.DelayedIntField(valueRect, entry.OverrideValue);

                if (value != entry.OverrideValue)
                {
                    _undo.Push();
                    entry.OverrideValue = value;
                    Persist();
                }

                if (GUI.Button(buttonRect, AutoContent, EditorStyles.miniButton))
                {
                    _undo.Push();
                    entry.OverridePriority = false;
                    Persist();
                }

                return;
            }

            string label = entry.Priority == int.MinValue
                ? "-"
                : entry.Priority.ToString();

            EditorGUI.LabelField(valueRect, label, EditorStyles.miniLabel);

            if (GUI.Button(buttonRect, OverrideContent, EditorStyles.miniButton))
            {
                _undo.Push();
                entry.OverridePriority = true;
                entry.OverrideValue = entry.Priority == int.MinValue
                    ? 0
                    : entry.Priority;

                Persist();
            }
        }

        private void CleanMissing()
        {
            _undo.Push();

            bool removed = MenuTree.RemoveMissing(WritableRoot);
            bool pruned = MenuTree.PruneEmptyGroups(WritableRoot);

            if (!removed && !pruned)
                _undo.DropLast();
        }

        private void AutoGroup()
        {
            _undo.Push();

            if (!MenuTree.AutoGroup(WritableRoot, _resolved))
                _undo.DropLast();
        }

        private void SortNodes()
        {
            _undo.Push();

            if (!MenuTree.Sort(WritableRoot))
                _undo.DropLast();
        }

        private void DrawFooter()
        {
            if (GUILayout.Button("Add Group", GUILayout.Height(24f)))
                _pending = () =>
                {
                    _undo.Push();
                    WritableRoot.Add(new MenuGroupNode("New Group"));
                };
        }

        private void DrawStatusBar()
        {
            string text = string.IsNullOrEmpty(_hoverPreview)
                ? " "
                : "Resolves to:  " + _hoverPreview;

            EditorGUILayout.LabelField(text, EditorStyles.miniLabel);
        }

        private void DrawGrip(Rect rect, Event current, Action onPress, bool locked)
        {
            GUI.Label(rect, "\u2261", MenuManagerTheme.Grip);

            if (locked)
                return;

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Pan);

            if (current.type == EventType.MouseDown
                && current.button == 0
                && rect.Contains(current.mousePosition))
            {
                onPress.Invoke();
                current.Use();
            }
        }
    }
}