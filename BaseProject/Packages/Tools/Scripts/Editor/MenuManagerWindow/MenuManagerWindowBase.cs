#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindow
{
    /// <summary>Shared window logic. Shows the shipped package tree read only and the project overlay tree editable.</summary>
    public abstract class MenuManagerWindowBase : EditorWindow
    {
        private const float RowHeight = 22f;
        private const float HeaderHeight = 24f;
        private const float Indent = 14f;
        private const float FoldWidth = 14f;
        private const float GripWidth = 18f;
        private const float ToggleWidth = 18f;
        private const float Pad = 4f;
        private const float DragThreshold = 4f;
        private const float SplitterWidth = 6f;
        private const int MaxUndoSteps = 100;

        private static readonly GUIContent AutoContent = new("A", "Reset to automatic priority");
        private static readonly GUIContent OverrideContent = new("M", "Override priority manually");

        private MenuRegistry registry;
        private MenuOverlay overlay;
        private Dictionary<string, ResolvedMenu> resolved = new();
        private Vector2 scroll;
        private Action pending;
        private string hoverPreview = string.Empty;
        private int activeSplitter = -1;

        private bool stylesReady;
        private GUIStyle titleStyle;
        private GUIStyle gripStyle;
        private GUIStyle columnStyle;
        private GUIStyle ghostStyle;

        private bool dragArmed;
        private bool dragActive;
        private bool dragIsGroup;
        private MenuNode dragNode;
        private List<MenuNode> dragSourceList;
        private readonly HashSet<List<MenuNode>> dragForbidden = new();
        private readonly HashSet<List<MenuNode>> lockedLists = new();
        private Vector2 dragStart;

        private List<MenuNode> dropParent;
        private int dropIndex;
        private bool dropValid;
        private Rect dropLine;

        private readonly List<State> undoStates = new();
        private readonly List<State> redoStates = new();
        private readonly List<Row> rows = new();

        /// <summary>Kind of entries this window manages.</summary>
        protected abstract EMenuEntryKind Kind { get; }

        /// <summary>Whether to show the asset file name column.</summary>
        protected virtual bool ShowFileName => false;

        private bool PackageLocked => registry != null && registry.IsReadOnly;

        private List<MenuNode> WritableRoot => PackageLocked ? overlay.RootFor(Kind) : registry.RootFor(Kind);

#region Unity Callbacks
        private void OnEnable()
        {
            registry = MenuRegistry.Instance;
            overlay = MenuOverlay.instance;
            wantsMouseMove = true;
            RefreshScan();
        }

        private void OnGUI()
        {
            if (registry == null)
                return;

            EnsureStyles();
            Event current = Event.current;

            if (current.type == EventType.MouseMove)
                Repaint();

            HandleUndoCommands(current);
            hoverPreview = string.Empty;

            DrawToolbar();

            if (PackageLocked)
                EditorGUILayout.HelpBox("The shipped layout is read only. Add and arrange your own entries under Project.", MessageType.Info);

            MenuComposite.Recalculate();
            DrawColumnHeader(current);

            BuildAllRows();

            scroll = EditorGUILayout.BeginScrollView(scroll);

            foreach (Row row in rows)
                DrawRow(row, current);

            GUILayout.Space(6f);
            DrawFooter();

            ResolveDrag(current);

            EditorGUILayout.EndScrollView();

            DrawStatusBar();

            if (pending == null)
                return;

            pending.Invoke();
            pending = null;
            Persist();
            Repaint();
        }
#endregion

        private void EnsureStyles()
        {
            if (stylesReady)
                return;

            titleStyle = new GUIStyle(EditorStyles.textField) { fontStyle = FontStyle.Bold };
            gripStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter, fontSize = 14 };
            columnStyle = new GUIStyle(EditorStyles.miniBoldLabel);
            ghostStyle = new GUIStyle(EditorStyles.helpBox) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold };
            stylesReady = true;
        }

        private void RefreshScan()
        {
            resolved = MenuScanner.Scan();
            MenuComposite.Sync(resolved);
        }

        private void Persist()
        {
            registry.Persist();
            overlay.Persist();
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

            using (new EditorGUI.DisabledScope(undoStates.Count == 0))
            {
                if (GUILayout.Button("Undo", EditorStyles.toolbarButton, GUILayout.Width(56f)))
                    PerformUndo();
            }

            using (new EditorGUI.DisabledScope(redoStates.Count == 0))
            {
                if (GUILayout.Button("Redo", EditorStyles.toolbarButton, GUILayout.Width(56f)))
                    PerformRedo();
            }

            GUILayout.FlexibleSpace();

            EditorGUI.BeginChangeCheck();

            int newStart;
            int newGap;

            using (new EditorGUI.DisabledScope(PackageLocked))
            {
                EditorGUILayout.LabelField("Start", GUILayout.Width(34f));
                newStart = EditorGUILayout.IntField(registry.StartPriority, EditorStyles.toolbarTextField, GUILayout.Width(50f));

                EditorGUILayout.LabelField("Gap", GUILayout.Width(28f));
                newGap = EditorGUILayout.IntField(registry.SeparatorGap, EditorStyles.toolbarTextField, GUILayout.Width(50f));
            }

            if (EditorGUI.EndChangeCheck())
            {
                PushUndo();
                registry.StartPriority = newStart;
                registry.SeparatorGap = newGap;
                Persist();
            }

            EditorGUILayout.EndHorizontal();

            string hint = ShowFileName
                ? "Group names build the path. Assets/Create is added automatically. Type only the custom part."
                : "Group names build the path. Type only the custom part in each entry.";

            EditorGUILayout.LabelField(hint, EditorStyles.miniLabel);
        }

        private void DrawColumnHeader(Event current)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 18f, GUILayout.ExpandWidth(true));

            float statusW = registry.ColumnStatusWidth;
            float priorityW = registry.ColumnPriorityWidth;
            float fileW = registry.ColumnFileWidth;

            float statusX = rect.xMax - statusW;
            float priorityX = statusX - Pad - priorityW;
            float fileX = ShowFileName ? priorityX - Pad - fileW : priorityX;

            float h = EditorGUIUtility.singleLineHeight;
            float y = rect.y + (rect.height - h) * 0.5f;
            float pathStart = rect.x + GripWidth + Pad + ToggleWidth + Pad;
            float pathEnd = ShowFileName ? fileX - Pad : priorityX - Pad;

            EditorGUI.LabelField(new Rect(pathStart, y, Mathf.Max(30f, pathEnd - pathStart), h), "Path", columnStyle);

            if (ShowFileName)
                EditorGUI.LabelField(new Rect(fileX, y, fileW, h), "File Name", columnStyle);

            EditorGUI.LabelField(new Rect(priorityX, y, priorityW, h), "Prio", columnStyle);
            EditorGUI.LabelField(new Rect(statusX, y, statusW, h), "State", columnStyle);

            DrawSplitter(rect, statusX, 2, current);
            DrawSplitter(rect, priorityX, 1, current);

            if (ShowFileName)
                DrawSplitter(rect, fileX, 0, current);

            if (activeSplitter < 0)
                return;

            if (current.type == EventType.MouseDrag)
            {
                float mouseX = current.mousePosition.x;

                if (activeSplitter == 2)
                    registry.ColumnStatusWidth = rect.xMax - mouseX;
                else if (activeSplitter == 1)
                    registry.ColumnPriorityWidth = statusX - mouseX;
                else
                    registry.ColumnFileWidth = priorityX - mouseX;

                current.Use();
                Repaint();
            }
            else if (current.type == EventType.MouseUp)
            {
                Persist();
                activeSplitter = -1;
            }
        }

        private void DrawSplitter(Rect header, float x, int id, Event current)
        {
            Rect handle = new(x - SplitterWidth * 0.5f, header.y, SplitterWidth, header.height);
            EditorGUIUtility.AddCursorRect(handle, MouseCursor.ResizeHorizontal);

            if (current.type == EventType.Repaint)
                EditorGUI.DrawRect(new Rect(x - 0.5f, header.y, 1f, header.height), GuideColor());

            if (current.type == EventType.MouseDown && current.button == 0 && handle.Contains(current.mousePosition))
            {
                activeSplitter = id;
                current.Use();
            }
        }

        private void BuildAllRows()
        {
            rows.Clear();
            lockedLists.Clear();

            List<MenuNode> packageRoot = registry.RootFor(Kind);
            List<MenuNode> overlayRoot = overlay.RootFor(Kind);

            if (PackageLocked)
            {
                rows.Add(new Row { IsSectionHeader = true, Header = "Shipped layout (read only)", Collapsible = true, Locked = true });

                if (!overlay.ShippedCollapsed)
                    AddSectionRows(packageRoot, true);

                rows.Add(new Row { IsSectionHeader = true, Header = "Project", Collapsible = false, Locked = false });
                AddSectionRows(overlayRoot, false);
            }
            else
            {
                AddSectionRows(packageRoot, false);

                if (overlayRoot.Count > 0)
                {
                    rows.Add(new Row { IsSectionHeader = true, Header = "Project", Collapsible = false, Locked = false });
                    AddSectionRows(overlayRoot, false);
                }
            }
        }

        private void AddSectionRows(List<MenuNode> root, bool locked)
        {
            if (locked)
                MarkListsLocked(root);

            if (root.Count == 0)
            {
                rows.Add(new Row { ParentList = root, Index = 0, Depth = 0, IsPlaceholder = true, Locked = locked });
                return;
            }

            List<string> prefix = new();
            string prefixRoot = MenuPath.Prefix(Kind);

            if (!string.IsNullOrEmpty(prefixRoot))
                prefix.Add(prefixRoot);

            BuildNodes(root, 0, prefix, locked, rows);
        }

        private void MarkListsLocked(List<MenuNode> nodes)
        {
            lockedLists.Add(nodes);

            foreach (MenuNode node in nodes)
            {
                if (node is MenuGroupNode group)
                    MarkListsLocked(group.Children);
            }
        }

        private void BuildNodes(List<MenuNode> nodes, int depth, List<string> prefix, bool locked, List<Row> output)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                MenuNode node = nodes[i];

                if (node is MenuGroupNode group)
                {
                    output.Add(new Row { Node = node, ParentList = nodes, Index = i, Depth = depth, IsGroup = true, Group = group, Locked = locked });

                    if (!group.Expanded)
                        continue;

                    prefix.Add(group.Name);

                    if (group.Children.Count == 0)
                        output.Add(new Row { ParentList = group.Children, Index = 0, Depth = depth + 1, IsPlaceholder = true, Locked = locked });
                    else
                        BuildNodes(group.Children, depth + 1, prefix, locked, output);

                    prefix.RemoveAt(prefix.Count - 1);
                }
                else if (node is MenuEntryNode entryNode)
                {
                    prefix.Add(entryNode.Entry.Path);
                    string full = MenuPath.Combine(prefix);
                    prefix.RemoveAt(prefix.Count - 1);

                    output.Add(new Row { Node = node, ParentList = nodes, Index = i, Depth = depth, Entry = entryNode.Entry, FullPath = full, Locked = locked });
                }
            }
        }

        private void DrawRow(Row row, Event current)
        {
            if (row.IsSectionHeader)
            {
                DrawSectionHeader(row, current);
                return;
            }

            float height = row.IsGroup ? HeaderHeight : RowHeight;
            Rect full = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));
            row.Rect = full;

            if (current.type == EventType.Repaint)
            {
                DrawGuides(full, row.Depth);

                if (row.Locked)
                    EditorGUI.DrawRect(full, LockedColor());
            }

            Rect content = new(full.x + row.Depth * Indent, full.y, full.width - row.Depth * Indent, full.height);

            if (row.IsPlaceholder)
            {
                EditorGUI.LabelField(content, row.Locked ? "No shipped entries" : "Drop entries here", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            if (row.IsGroup)
                DrawGroupRow(content, row, current);
            else
                DrawEntryRow(full, row, current);
        }

        private void DrawSectionHeader(Row row, Event current)
        {
            Rect full = GUILayoutUtility.GetRect(0f, HeaderHeight, GUILayout.ExpandWidth(true));
            row.Rect = full;

            if (current.type == EventType.Repaint)
                EditorGUI.DrawRect(full, SectionColor());

            float h = EditorGUIUtility.singleLineHeight;
            float y = full.y + (full.height - h) * 0.5f;
            float x = full.x + 4f;

            if (row.Collapsible)
            {
                bool expanded = !overlay.ShippedCollapsed;
                bool now = EditorGUI.Foldout(new Rect(x, y, FoldWidth, h), expanded, GUIContent.none);

                if (now != expanded)
                {
                    overlay.ShippedCollapsed = !now;
                    overlay.Persist();
                }

                x += FoldWidth + 2f;
            }

            EditorGUI.LabelField(new Rect(x, y, full.xMax - x - 4f, h), row.Header, EditorStyles.boldLabel);
        }

        private void DrawGroupRow(Rect content, Row row, Event current)
        {
            MenuGroupNode group = row.Group;
            bool locked = row.Locked;
            bool isSource = dragActive && dragNode == group;

            if (current.type == EventType.Repaint)
                EditorGUI.DrawRect(content, HeaderColor(isSource));

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
            DrawGrip(grip, current, () => BeginGroupDrag(current, group, row.ParentList), locked);
            x += GripWidth + Pad;

            float addWidth = 54f;
            float deleteWidth = 54f;
            Rect deleteRect = new(content.xMax - deleteWidth - 2f, y, deleteWidth, h);
            Rect addRect = new(deleteRect.x - addWidth - Pad, y, addWidth, h);
            Rect nameRect = new(x, y, Mathf.Max(40f, addRect.x - x - Pad), h);

            string newName;

            using (new EditorGUI.DisabledScope(locked))
                newName = EditorGUI.DelayedTextField(nameRect, group.Name, titleStyle);

            if (newName != group.Name)
            {
                PushUndo();
                group.Name = newName;
                Persist();
            }

            using (new EditorGUI.DisabledScope(locked))
            {
                if (GUI.Button(addRect, "+ Group", EditorStyles.miniButton))
                {
                    MenuGroupNode captured = group;
                    pending = () =>
                    {
                        PushUndo();
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
                    pending = () =>
                    {
                        PushUndo();
                        parent.Remove(captured);
                    };
                }
            }
        }

        private void DrawEntryRow(Rect full, Row row, Event current)
        {
            MenuEntry entry = row.Entry;
            bool locked = row.Locked;
            bool isSource = dragActive && dragNode == row.Node;

            if (current.type == EventType.Repaint && (row.Index & 1) == 1)
                EditorGUI.DrawRect(full, RowStripeColor());

            if (current.type == EventType.Repaint && isSource)
                EditorGUI.DrawRect(full, SelectionColor());

            if (current.type == EventType.Repaint && !dragActive && full.Contains(current.mousePosition))
                hoverPreview = row.FullPath;

            Columns columns = Compute(full, row.Depth);
            DrawGrip(columns.Grip, current, () => BeginEntryDrag(current, row.Node, row.ParentList), locked);

            bool enabled;

            using (new EditorGUI.DisabledScope(locked))
                enabled = EditorGUI.Toggle(columns.Toggle, entry.Enabled);

            if (enabled != entry.Enabled)
            {
                PushUndo();
                entry.Enabled = enabled;
                Persist();
            }

            using (new EditorGUI.DisabledScope(entry.Missing || locked))
            {
                string path = EditorGUI.DelayedTextField(columns.Path, entry.Path);

                if (path != entry.Path)
                {
                    PushUndo();
                    entry.Path = path;
                    Persist();
                }

                if (ShowFileName)
                {
                    string file = EditorGUI.DelayedTextField(columns.File, entry.CreateFileName);

                    if (file != entry.CreateFileName)
                    {
                        PushUndo();
                        entry.CreateFileName = file;
                        Persist();
                    }
                }
            }

            DrawPriorityCell(columns.Priority, entry, locked);
            EditorGUI.LabelField(columns.Status, entry.Missing ? "missing" : "ok", EditorStyles.miniLabel);
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
                    EditorGUI.DrawRect(valueRect, OverrideColor());

                int value = EditorGUI.DelayedIntField(valueRect, entry.OverrideValue);

                if (value != entry.OverrideValue)
                {
                    PushUndo();
                    entry.OverrideValue = value;
                    Persist();
                }

                if (GUI.Button(buttonRect, AutoContent, EditorStyles.miniButton))
                {
                    PushUndo();
                    entry.OverridePriority = false;
                    Persist();
                }

                return;
            }

            string label = entry.Priority == int.MinValue ? "-" : entry.Priority.ToString();
            EditorGUI.LabelField(valueRect, label, EditorStyles.miniLabel);

            if (GUI.Button(buttonRect, OverrideContent, EditorStyles.miniButton))
            {
                PushUndo();
                entry.OverridePriority = true;
                entry.OverrideValue = entry.Priority == int.MinValue ? 0 : entry.Priority;
                Persist();
            }
        }

        private void DrawFooter()
        {
            if (GUILayout.Button("Add Group", GUILayout.Height(24f)))
                pending = () =>
                {
                    PushUndo();
                    WritableRoot.Add(new MenuGroupNode("New Group"));
                };
        }

        private void DrawStatusBar()
        {
            string text = string.IsNullOrEmpty(hoverPreview) ? " " : "Resolves to:  " + hoverPreview;
            EditorGUILayout.LabelField(text, EditorStyles.miniLabel);
        }

        private void DrawGrip(Rect rect, Event current, Action onPress, bool locked)
        {
            GUI.Label(rect, "\u2261", gripStyle);

            if (locked)
                return;

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Pan);

            if (current.type == EventType.MouseDown && current.button == 0 && rect.Contains(current.mousePosition))
            {
                onPress.Invoke();
                current.Use();
            }
        }

        private void BeginEntryDrag(Event current, MenuNode node, List<MenuNode> parent)
        {
            dragArmed = true;
            dragActive = false;
            dragIsGroup = false;
            dragNode = node;
            dragSourceList = parent;
            dragStart = current.mousePosition;
            dragForbidden.Clear();
        }

        private void BeginGroupDrag(Event current, MenuGroupNode group, List<MenuNode> parent)
        {
            dragArmed = true;
            dragActive = false;
            dragIsGroup = true;
            dragNode = group;
            dragSourceList = parent;
            dragStart = current.mousePosition;
            dragForbidden.Clear();
            CollectForbidden(group);
        }

        private void CollectForbidden(MenuGroupNode group)
        {
            dragForbidden.Add(group.Children);

            foreach (MenuNode child in group.Children)
            {
                if (child is MenuGroupNode sub)
                    CollectForbidden(sub);
            }
        }

        private void ResolveDrag(Event current)
        {
            if (!dragArmed)
                return;

            if (!dragActive && current.type == EventType.MouseDrag &&
                Vector2.Distance(current.mousePosition, dragStart) > DragThreshold)
            {
                dragActive = true;
            }

            if (dragActive)
            {
                ComputeDropTarget(current.mousePosition);

                if (current.type == EventType.Repaint)
                {
                    if (dropValid)
                        EditorGUI.DrawRect(dropLine, AccentColor());

                    DrawGhost(current.mousePosition);
                }

                if (current.type == EventType.MouseDrag)
                {
                    current.Use();
                    Repaint();
                }
            }

            if (current.type == EventType.MouseUp)
            {
                if (dragActive)
                {
                    FinishDrag();
                    current.Use();
                }

                dragArmed = false;
                dragActive = false;
                dragNode = null;
                dragSourceList = null;
                dragForbidden.Clear();
                Repaint();
            }
        }

        private void ComputeDropTarget(Vector2 mouse)
        {
            dropValid = false;
            dropParent = null;
            dropIndex = 0;

            foreach (Row row in rows)
            {
                if (row.IsSectionHeader)
                    continue;

                if (mouse.y < row.Rect.yMin || mouse.y > row.Rect.yMax)
                    continue;

                bool topHalf = mouse.y < row.Rect.center.y;

                if (row.IsPlaceholder)
                {
                    Set(row.ParentList, 0, new Rect(row.Rect.x + 6f, row.Rect.center.y - 1f, row.Rect.width - 12f, 2f));
                    return;
                }

                if (row.IsGroup)
                {
                    if (topHalf)
                        Set(row.ParentList, row.Index, LineAt(row.Rect.yMin, row.Rect, row.Depth));
                    else if (row.Group.Expanded)
                        Set(row.Group.Children, 0, LineAt(row.Rect.yMax, row.Rect, row.Depth + 1));
                    else
                        Set(row.ParentList, row.Index + 1, LineAt(row.Rect.yMax, row.Rect, row.Depth));

                    return;
                }

                int index = topHalf ? row.Index : row.Index + 1;
                float y = topHalf ? row.Rect.yMin : row.Rect.yMax;
                Set(row.ParentList, index, LineAt(y, row.Rect, row.Depth));
                return;
            }

            if (rows.Count > 0 && mouse.y > rows[rows.Count - 1].Rect.yMax)
                Set(WritableRoot, WritableRoot.Count, LineAt(rows[rows.Count - 1].Rect.yMax, rows[rows.Count - 1].Rect, 0));
        }

        private void Set(List<MenuNode> parent, int index, Rect line)
        {
            if (dragForbidden.Contains(parent) || lockedLists.Contains(parent))
                return;

            dropParent = parent;
            dropIndex = index;
            dropLine = line;
            dropValid = true;
        }

        private void FinishDrag()
        {
            if (!dropValid || dropParent == null || dragNode == null)
                return;

            int sourceIndex = dragSourceList.IndexOf(dragNode);

            if (sourceIndex < 0)
                return;

            PushUndo();
            dragSourceList.RemoveAt(sourceIndex);
            int target = dropIndex;

            if (dragSourceList == dropParent && sourceIndex < target)
                target--;

            target = Mathf.Clamp(target, 0, dropParent.Count);
            dropParent.Insert(target, dragNode);
            Persist();
        }

        private void DrawGhost(Vector2 mouse)
        {
            string label = dragIsGroup ? ((MenuGroupNode)dragNode).Name : dragNode is MenuEntryNode en ? en.Entry.Path : "Entry";

            if (string.IsNullOrEmpty(label))
                label = dragIsGroup ? "Group" : "Entry";

            GUI.Box(new Rect(mouse.x + 12f, mouse.y + 4f, 220f, 20f), label, ghostStyle);
        }

        private void DrawGuides(Rect full, int depth)
        {
            for (int d = 1; d <= depth; d++)
            {
                float x = full.x + d * Indent - Indent * 0.5f;
                EditorGUI.DrawRect(new Rect(x, full.y, 1f, full.height), GuideColor());
            }
        }

        private Rect LineAt(float y, Rect row, int depth) =>
            new(row.x + depth * Indent + 6f, y - 1f, row.width - depth * Indent - 12f, 2f);

        private Columns Compute(Rect full, int depth)
        {
            float h = EditorGUIUtility.singleLineHeight;
            float y = full.y + (full.height - h) * 0.5f;
            float left = full.x + depth * Indent;

            Rect grip = new(left, y, GripWidth, h);
            float x = left + GripWidth + Pad;

            Rect toggle = new(x, y, ToggleWidth, h);
            x += ToggleWidth + Pad;

            float statusW = registry.ColumnStatusWidth;
            float priorityW = registry.ColumnPriorityWidth;
            float fileW = registry.ColumnFileWidth;

            float statusX = full.xMax - statusW;
            float priorityX = statusX - Pad - priorityW;
            float fileX = ShowFileName ? priorityX - Pad - fileW : priorityX;
            float pathEnd = ShowFileName ? fileX - Pad : priorityX - Pad;
            float pathWidth = Mathf.Max(60f, pathEnd - x);

            Rect path = new(x, y, pathWidth, h);
            Rect file = new(fileX, y, fileW, h);
            Rect priority = new(priorityX, y, priorityW, h);
            Rect status = new(statusX, y, statusW, h);

            return new Columns { Grip = grip, Toggle = toggle, Path = path, File = file, Priority = priority, Status = status };
        }

        private void PushUndo()
        {
            undoStates.Add(CaptureState());

            if (undoStates.Count > MaxUndoSteps)
                undoStates.RemoveAt(0);

            redoStates.Clear();
        }

        private void PerformUndo()
        {
            if (undoStates.Count == 0)
                return;

            redoStates.Add(CaptureState());
            State state = undoStates[undoStates.Count - 1];
            undoStates.RemoveAt(undoStates.Count - 1);
            ApplyState(state);
        }

        private void PerformRedo()
        {
            if (redoStates.Count == 0)
                return;

            undoStates.Add(CaptureState());
            State state = redoStates[redoStates.Count - 1];
            redoStates.RemoveAt(redoStates.Count - 1);
            ApplyState(state);
        }

        private void HandleUndoCommands(Event current)
        {
            if (current.type == EventType.ValidateCommand && (current.commandName == "Undo" || current.commandName == "Redo"))
            {
                current.Use();
                return;
            }

            if (current.type != EventType.ExecuteCommand)
                return;

            if (current.commandName == "Undo")
            {
                PerformUndo();
                current.Use();
            }
            else if (current.commandName == "Redo")
            {
                PerformRedo();
                current.Use();
            }
        }

        private State CaptureState() => new()
        {
            Package = CloneNodes(registry.RootFor(Kind)),
            Overlay = CloneNodes(overlay.RootFor(Kind)),
            Start = registry.StartPriority,
            Gap = registry.SeparatorGap
        };

        private void ApplyState(State state)
        {
            GUIUtility.keyboardControl = 0;
            EditorGUIUtility.editingTextField = false;

            List<MenuNode> package = registry.RootFor(Kind);
            package.Clear();
            package.AddRange(CloneNodes(state.Package));

            List<MenuNode> overlayRoot = overlay.RootFor(Kind);
            overlayRoot.Clear();
            overlayRoot.AddRange(CloneNodes(state.Overlay));

            registry.StartPriority = state.Start;
            registry.SeparatorGap = state.Gap;

            MenuComposite.Recalculate();
            Persist();
            Repaint();
        }

        private static List<MenuNode> CloneNodes(List<MenuNode> nodes)
        {
            List<MenuNode> copy = new(nodes.Count);

            foreach (MenuNode node in nodes)
                copy.Add(CloneNode(node));

            return copy;
        }

        private static MenuNode CloneNode(MenuNode node)
        {
            if (node is MenuGroupNode group)
            {
                MenuGroupNode clone = new(group.Name) { Expanded = group.Expanded };

                foreach (MenuNode child in group.Children)
                    clone.Children.Add(CloneNode(child));

                return clone;
            }

            if (node is MenuEntryNode entryNode)
                return new MenuEntryNode(CloneEntry(entryNode.Entry));

            return null;
        }

        private static MenuEntry CloneEntry(MenuEntry entry) => new(entry.Id, entry.Path, entry.Kind)
        {
            Enabled = entry.Enabled,
            CreateFileName = entry.CreateFileName,
            OverridePriority = entry.OverridePriority,
            OverrideValue = entry.OverrideValue
        };

        private static Color HeaderColor(bool active) => active
            ? new Color(0.23f, 0.36f, 0.55f, 0.6f)
            : EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.07f) : new Color(0f, 0f, 0f, 0.07f);

        private static Color SectionColor() =>
            EditorGUIUtility.isProSkin ? new Color(0.35f, 0.45f, 0.6f, 0.25f) : new Color(0.35f, 0.45f, 0.6f, 0.18f);

        private static Color LockedColor() =>
            EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.02f) : new Color(0f, 0f, 0f, 0.03f);

        private static Color RowStripeColor() =>
            EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.03f) : new Color(0f, 0f, 0f, 0.03f);

        private static Color SelectionColor() => new(0.23f, 0.55f, 0.95f, 0.15f);

        private static Color AccentColor() => new(0.23f, 0.55f, 0.95f, 0.9f);

        private static Color OverrideColor() => new(0.95f, 0.75f, 0.2f, 0.18f);

        private static Color GuideColor() =>
            EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.1f) : new Color(0f, 0f, 0f, 0.12f);

        private sealed class Row
        {
            public MenuNode Node;
            public List<MenuNode> ParentList;
            public int Index;
            public int Depth;
            public bool IsGroup;
            public bool IsPlaceholder;
            public bool IsSectionHeader;
            public bool Locked;
            public bool Collapsible;
            public string Header;
            public MenuGroupNode Group;
            public MenuEntry Entry;
            public string FullPath;
            public Rect Rect;
        }

        private sealed class State
        {
            public List<MenuNode> Package;
            public List<MenuNode> Overlay;
            public int Start;
            public int Gap;
        }

        private struct Columns
        {
            public Rect Grip;
            public Rect Toggle;
            public Rect Path;
            public Rect File;
            public Rect Priority;
            public Rect Status;
        }
    }
}
#endif
