#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.MenuManagerWindow
{
    /// <summary>Shared window logic to arrange entries of a single kind into a nested tree with drag and drop.</summary>
    public abstract class MenuManagerWindowBase : EditorWindow
    {
        private const float DragThreshold = 4f;
        private const float FoldWidth = 14f;
        private const float GripWidth = 18f;
        private const float HeaderHeight = 24f;
        private const float Indent = 14f;

        private const int MaxUndoSteps = 100;
        private const float Pad = 4f;
        private const float RowHeight = 22f;
        private const float SplitterWidth = 6f;
        private const float ToggleWidth = 18f;

        private static readonly GUIContent AutoContent = new("A", "Reset to automatic priority");
        private static readonly GUIContent OverrideContent = new("M", "Override priority manually");

        /// <summary>Kind of entries this window manages.</summary>
        protected abstract EMenuEntryKind Kind { get; }

        /// <summary>Whether to show the asset file name column.</summary>
        protected virtual bool ShowFileName => false;

        private List<MenuNode> Root => registry.RootFor(Kind);

        private readonly HashSet<List<MenuNode>> dragForbidden = new();
        private readonly List<State> undoStates = new();
        private readonly List<State> redoStates = new();

        private readonly List<Row> rows = new();

        private MenuRegistry registry;
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
        private Vector2 dragStart;

        private List<MenuNode> dropParent;
        private int dropIndex;
        private bool dropValid;
        private Rect dropLine;

#region Unity Callbacks
        private void OnEnable()
        {
            registry = MenuRegistry.instance;
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
            registry.RecalculatePriorities();
            DrawColumnHeader(current);

            rows.Clear();
            List<string> prefix = new();
            string rootPrefix = MenuPath.Prefix(Kind);

            if (!string.IsNullOrEmpty(rootPrefix))
                prefix.Add(rootPrefix);

            BuildRows(Root, 0, prefix, rows);

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
            registry.Persist();
            Repaint();
        }
#endregion

        private static Color HeaderColor(bool active) => active
            ? new Color(0.23f, 0.36f, 0.55f, 0.6f)
            : EditorGUIUtility.isProSkin
                ? new Color(1f, 1f, 1f, 0.07f)
                : new Color(0f, 0f, 0f, 0.07f);

        private static Color RowStripeColor() => EditorGUIUtility.isProSkin
            ? new Color(1f, 1f, 1f, 0.03f)
            : new Color(0f, 0f, 0f, 0.03f);

        private static Color SelectionColor() => new(0.23f, 0.55f, 0.95f, 0.15f);

        private static Color AccentColor() => new(0.23f, 0.55f, 0.95f, 0.9f);

        private static Color OverrideColor() => new(0.95f, 0.75f, 0.2f, 0.18f);

        private static Color GuideColor() => EditorGUIUtility.isProSkin
            ? new Color(1f, 1f, 1f, 0.1f)
            : new Color(0f, 0f, 0f, 0.12f);

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
                MenuGroupNode clone = new(group.Name)
                {
                    Expanded = group.Expanded
                };

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

        private void EnsureStyles()
        {
            if (stylesReady)
                return;

            titleStyle = new GUIStyle(EditorStyles.textField)
            {
                fontStyle = FontStyle.Bold
            };

            gripStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14
            };

            columnStyle = new GUIStyle(EditorStyles.miniBoldLabel);
            ghostStyle = new GUIStyle(EditorStyles.helpBox)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold
            };

            stylesReady = true;
        }

        private void RefreshScan()
        {
            resolved = MenuScanner.Scan();
            registry.Migrate();
            registry.Sync(resolved);
            registry.RecalculatePriorities();
            registry.Persist();
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

            EditorGUILayout.LabelField("Start", GUILayout.Width(34f));
            int newStart = EditorGUILayout.IntField(registry.StartPriority, EditorStyles.toolbarTextField,
                GUILayout.Width(50f));

            EditorGUILayout.LabelField("Gap", GUILayout.Width(28f));
            int newGap = EditorGUILayout.IntField(registry.SeparatorGap, EditorStyles.toolbarTextField,
                GUILayout.Width(50f));

            if (EditorGUI.EndChangeCheck())
            {
                PushUndo();
                registry.StartPriority = newStart;
                registry.SeparatorGap = newGap;
                registry.Persist();
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
            float fileX = ShowFileName
                ? priorityX - Pad - fileW
                : priorityX;

            float h = EditorGUIUtility.singleLineHeight;
            float y = rect.y + (rect.height - h) * 0.5f;
            float pathStart = rect.x + GripWidth + Pad + ToggleWidth + Pad;
            float pathEnd = ShowFileName
                ? fileX - Pad
                : priorityX - Pad;

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
                registry.Persist();
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

        private void BuildRows(List<MenuNode> nodes, int depth, List<string> prefix, List<Row> output)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                MenuNode node = nodes[i];

                if (node is MenuGroupNode group)
                {
                    output.Add(new Row
                    {
                        Node = node,
                        ParentList = nodes,
                        Index = i,
                        Depth = depth,
                        IsGroup = true,
                        Group = group
                    });

                    if (!group.Expanded)
                        continue;

                    prefix.Add(group.Name);

                    if (group.Children.Count == 0)
                        output.Add(new Row
                        {
                            ParentList = group.Children,
                            Index = 0,
                            Depth = depth + 1,
                            IsPlaceholder = true
                        });
                    else
                        BuildRows(group.Children, depth + 1, prefix, output);

                    prefix.RemoveAt(prefix.Count - 1);
                }
                else if (node is MenuEntryNode entryNode)
                {
                    prefix.Add(entryNode.Entry.Path);
                    string full = MenuPath.Combine(prefix);
                    prefix.RemoveAt(prefix.Count - 1);

                    output.Add(new Row
                    {
                        Node = node,
                        ParentList = nodes,
                        Index = i,
                        Depth = depth,
                        Entry = entryNode.Entry,
                        FullPath = full
                    });
                }
            }
        }

        private void DrawRow(Row row, Event current)
        {
            float height = row.IsGroup
                ? HeaderHeight
                : RowHeight;

            Rect full = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));
            row.Rect = full;

            if (current.type == EventType.Repaint)
                DrawGuides(full, row.Depth);

            Rect content = new(full.x + row.Depth * Indent, full.y, full.width - row.Depth * Indent, full.height);

            if (row.IsPlaceholder)
            {
                EditorGUI.LabelField(content, "Drop entries here", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            if (row.IsGroup)
                DrawGroupRow(content, row, current);
            else
                DrawEntryRow(full, row, current);
        }

        private void DrawGroupRow(Rect content, Row row, Event current)
        {
            MenuGroupNode group = row.Group;
            bool isSource = dragActive && dragNode == group;

            if (current.type == EventType.Repaint)
                EditorGUI.DrawRect(content, HeaderColor(isSource));

            float h = EditorGUIUtility.singleLineHeight;
            float y = content.y + (content.height - h) * 0.5f;
            float x = content.x + 2f;

            Rect foldRect = new(x, y, FoldWidth, h);
            bool expanded = EditorGUI.Foldout(foldRect, group.Expanded, GUIContent.none);

            if (expanded != group.Expanded)
            {
                group.Expanded = expanded;
                registry.Persist();
            }

            x += FoldWidth;
            Rect grip = new(x, content.y, GripWidth, content.height);
            DrawGrip(grip, current, onPress: () => BeginGroupDrag(current, group, row.ParentList));
            x += GripWidth + Pad;

            float addWidth = 54f;
            float deleteWidth = 54f;
            Rect deleteRect = new(content.xMax - deleteWidth - 2f, y, deleteWidth, h);
            Rect addRect = new(deleteRect.x - addWidth - Pad, y, addWidth, h);
            Rect nameRect = new(x, y, Mathf.Max(40f, addRect.x - x - Pad), h);

            string newName = EditorGUI.DelayedTextField(nameRect, group.Name, titleStyle);

            if (newName != group.Name)
            {
                PushUndo();
                group.Name = newName;
                registry.Persist();
            }

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

            using (new EditorGUI.DisabledScope(group.Children.Count != 0))
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
            bool isSource = dragActive && dragNode == row.Node;

            if (current.type == EventType.Repaint && (row.Index & 1) == 1)
                EditorGUI.DrawRect(full, RowStripeColor());

            if (current.type == EventType.Repaint && isSource)
                EditorGUI.DrawRect(full, SelectionColor());

            if (current.type == EventType.Repaint && !dragActive && full.Contains(current.mousePosition))
                hoverPreview = row.FullPath;

            Columns columns = Compute(full, row.Depth);
            DrawGrip(columns.Grip, current, onPress: () => BeginEntryDrag(current, row.Node, row.ParentList));

            bool enabled = EditorGUI.Toggle(columns.Toggle, entry.Enabled);

            if (enabled != entry.Enabled)
            {
                PushUndo();
                entry.Enabled = enabled;
                registry.Persist();
            }

            using (new EditorGUI.DisabledScope(entry.Missing))
            {
                string path = EditorGUI.DelayedTextField(columns.Path, entry.Path);

                if (path != entry.Path)
                {
                    PushUndo();
                    entry.Path = path;
                    registry.Persist();
                }

                if (ShowFileName)
                {
                    string file = EditorGUI.DelayedTextField(columns.File, entry.CreateFileName);

                    if (file != entry.CreateFileName)
                    {
                        PushUndo();
                        entry.CreateFileName = file;
                        registry.Persist();
                    }
                }
            }

            DrawPriorityCell(columns.Priority, entry, current);
            EditorGUI.LabelField(columns.Status, entry.Missing
                ? "missing"
                : "ok", EditorStyles.miniLabel);
        }

        private void DrawFooter()
        {
            if (GUILayout.Button("Add Group", GUILayout.Height(24f)))
                pending = () =>
                {
                    PushUndo();
                    Root.Add(new MenuGroupNode("New Group"));
                };
        }

        private void DrawStatusBar()
        {
            string text = string.IsNullOrEmpty(hoverPreview)
                ? " "
                : "Resolves to:  " + hoverPreview;

            EditorGUILayout.LabelField(text, EditorStyles.miniLabel);
        }

        private void DrawGrip(Rect rect, Event current, Action onPress)
        {
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Pan);
            GUI.Label(rect, "\u2261", gripStyle);

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

            if (!dragActive
                && current.type == EventType.MouseDrag
                && Vector2.Distance(current.mousePosition, dragStart) > DragThreshold)
                dragActive = true;

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

                int index = topHalf
                    ? row.Index
                    : row.Index + 1;

                float y = topHalf
                    ? row.Rect.yMin
                    : row.Rect.yMax;

                Set(row.ParentList, index, LineAt(y, row.Rect, row.Depth));
                return;
            }

            if (rows.Count > 0 && mouse.y > rows[rows.Count - 1].Rect.yMax)
                Set(Root, Root.Count, LineAt(rows[rows.Count - 1].Rect.yMax, rows[rows.Count - 1].Rect, 0));
        }

        private void Set(List<MenuNode> parent, int index, Rect line)
        {
            if (dragForbidden.Contains(parent))
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
            registry.Persist();
        }

        private void DrawGhost(Vector2 mouse)
        {
            string label = dragIsGroup
                ? ((MenuGroupNode)dragNode).Name
                : dragNode is MenuEntryNode en
                    ? en.Entry.Path
                    : "Entry";

            if (string.IsNullOrEmpty(label))
                label = dragIsGroup
                    ? "Group"
                    : "Entry";

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

        private Rect LineAt(float y, Rect row, int depth)
            => new(row.x + depth * Indent + 6f, y - 1f, row.width - depth * Indent - 12f, 2f);

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

            return new Columns
            {
                Grip = grip,
                Toggle = toggle,
                Path = path,
                File = file,
                Priority = priority,
                Status = status
            };
        }

        private void DrawPriorityCell(Rect cell, MenuEntry entry, Event current)
        {
            const float buttonWidth = 18f;
            Rect valueRect = new(cell.x, cell.y, Mathf.Max(18f, cell.width - buttonWidth - 2f), cell.height);
            Rect buttonRect = new(valueRect.xMax + 2f, cell.y, buttonWidth, cell.height);

            if (entry.OverridePriority)
            {
                if (current.type == EventType.Repaint)
                    EditorGUI.DrawRect(valueRect, OverrideColor());

                int value = EditorGUI.DelayedIntField(valueRect, entry.OverrideValue);

                if (value != entry.OverrideValue)
                {
                    PushUndo();
                    entry.OverrideValue = value;
                    registry.Persist();
                }

                if (GUI.Button(buttonRect, AutoContent, EditorStyles.miniButton))
                {
                    PushUndo();
                    entry.OverridePriority = false;
                    registry.Persist();
                }

                return;
            }

            string label = entry.Priority == int.MinValue
                ? "-"
                : entry.Priority.ToString();

            EditorGUI.LabelField(valueRect, label, EditorStyles.miniLabel);

            if (GUI.Button(buttonRect, OverrideContent, EditorStyles.miniButton))
            {
                PushUndo();
                entry.OverridePriority = true;
                entry.OverrideValue = entry.Priority == int.MinValue
                    ? 0
                    : entry.Priority;

                registry.Persist();
            }
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
            if (current.type == EventType.ValidateCommand
                && current.commandName is "Undo" or "Redo")
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
            Root = CloneNodes(Root),
            Start = registry.StartPriority,
            Gap = registry.SeparatorGap
        };

        private void ApplyState(State state)
        {
            GUIUtility.keyboardControl = 0;
            EditorGUIUtility.editingTextField = false;
            List<MenuNode> live = Root;
            live.Clear();
            live.AddRange(CloneNodes(state.Root));
            registry.StartPriority = state.Start;
            registry.SeparatorGap = state.Gap;
            registry.RecalculatePriorities();
            registry.Persist();
            Repaint();
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

        private sealed class Row
        {
            public MenuNode Node;
            public List<MenuNode> ParentList;
            public int Index;
            public int Depth;
            public bool IsGroup;
            public bool IsPlaceholder;
            public MenuGroupNode Group;
            public MenuEntry Entry;
            public string FullPath;
            public Rect Rect;
        }

        private sealed class State
        {
            public List<MenuNode> Root;
            public int Start;
            public int Gap;
        }
    }
}
#endif