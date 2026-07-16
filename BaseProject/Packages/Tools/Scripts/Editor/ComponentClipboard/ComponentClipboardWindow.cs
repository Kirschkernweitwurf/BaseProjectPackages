using System.Collections.Generic;
using Base.ToolPackage.MenuManagerWindow;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.ComponentClipboard
{
    /// <summary>
    /// Lists the components of the active GameObject with checkbox multi selection and offers copy,
    /// paste, delete and reorder through a context menu. Fills the gap left by Unity's single entry
    /// component clipboard.
    /// </summary>
    public class ComponentClipboardWindow : EditorWindow
    {
        private const string AddBadge = "Add";
        private const float BadgeWidth = 80f;
        private const int LeftMouseButton = 0;
        private const string MenuPath = "Tools/Base Packages/Unity Editor/Component Clipboard";
        private const float MinWindowHeight = 280f;
        private const float MinWindowWidth = 300f;
        private const string MissingBadge = "Missing";
        private const int NoIndex = -1;
        private const string OverwriteBadge = "Overwrite";
        private const float RowHeight = 20f;
        private const int SingleTarget = 1;
        private const float ToggleWidth = 20f;
        private const string WindowTitle = "Component Clipboard";

        private readonly List<Component> _components = new();
        private readonly List<Component> _selected = new();

        private GameObject _target;
        private Vector2 _scroll;
        private int _lastClickedIndex = NoIndex;

#region Unity Callbacks
        private void OnEnable()
        {
            ComponentClipboard.Changed += Repaint;
            Rebuild();
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.Layout)
                Rebuild();

            if (_target == null)
            {
                EditorGUILayout.HelpBox("Select a GameObject to list its components.", MessageType.Info);
                return;
            }

            DrawHeader();
            DrawComponentList();
            DrawClipboard();
            DrawButtons();
        }

        private void OnDisable() => ComponentClipboard.Changed -= Repaint;

        private void OnSelectionChange()
        {
            Rebuild();
            Repaint();
        }
#endregion

        [DynamicMenuItem(MenuPath)]
        private static void Open()
        {
            ComponentClipboardWindow window = GetWindow<ComponentClipboardWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
        }

        private static Color GetSelectionColor() => EditorGUIUtility.isProSkin
            ? new Color(0.24f, 0.37f, 0.59f, 1f)
            : new Color(0.6f, 0.75f, 0.95f, 1f);

        private static Color GetOverwriteColor() => EditorGUIUtility.isProSkin
            ? new Color(1f, 0.72f, 0.35f, 1f)
            : new Color(0.75f, 0.42f, 0f, 1f);

        private static Color GetMissingColor() => EditorGUIUtility.isProSkin
            ? new Color(1f, 0.45f, 0.4f, 1f)
            : new Color(0.7f, 0.1f, 0.05f, 1f);

        private static GUIContent BuildBadge(ComponentPasteStep step)
        {
            if (!step.IsValid)
                return new GUIContent(MissingBadge, "The type of this entry no longer exists.");

            if (!step.IsOverwrite)
                return new GUIContent(AddBadge, "No selected component of this type, a new one gets added.");

            if (step.Targets.Count == SingleTarget)
                return new GUIContent(OverwriteBadge, $"Overwrites the selected {step.Type.Name}.");

            return new GUIContent($"{OverwriteBadge} {step.Targets.Count}",
                $"Overwrites all {step.Targets.Count} selected {step.Type.Name} components.");
        }

        private static Color GetBadgeColor(ComponentPasteStep step, Color fallback)
        {
            if (!step.IsValid)
                return GetMissingColor();

            if (!step.IsOverwrite)
                return fallback;

            return GetOverwriteColor();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField(_target.name, EditorStyles.boldLabel);
            EditorGUILayout.Space();
        }

        private void DrawComponentList()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));

            for (int index = 0; index < _components.Count; index++)
                DrawComponentRow(_components[index], index);

            EditorGUILayout.EndScrollView();
        }

        private void DrawComponentRow(Component component, int index)
        {
            if (component == null)
                return;

            Rect rowRect = EditorGUILayout.GetControlRect(false, RowHeight);
            Rect toggleRect = new(rowRect.x, rowRect.y, ToggleWidth, rowRect.height);
            Rect labelRect = new(rowRect.x + ToggleWidth, rowRect.y, rowRect.width - ToggleWidth,
                rowRect.height);

            bool isSelected = _selected.Contains(component);

            if (isSelected && Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(rowRect, GetSelectionColor());

            bool isToggled = EditorGUI.Toggle(toggleRect, isSelected);

            if (isToggled != isSelected)
                SetSelected(component, isToggled, index);

            GUIContent content = EditorGUIUtility.ObjectContent(component, component.GetType());
            content.text = ObjectNames.NicifyVariableName(component.GetType().Name);
            EditorGUI.LabelField(labelRect, content);

            HandleRowInput(labelRect, rowRect, component, index);
        }

        private void HandleRowInput(Rect labelRect, Rect rowRect, Component component, int index)
        {
            Event current = Event.current;

            if (current.type == EventType.MouseDown
                && current.button == LeftMouseButton
                && labelRect.Contains(current.mousePosition))
            {
                if (current.shift && _lastClickedIndex != NoIndex)
                    SelectRange(_lastClickedIndex, index);
                else if (EditorGUI.actionKey)
                    SetSelected(component, !_selected.Contains(component), index);
                else
                    SelectSingle(component, index);

                current.Use();
                Repaint();
                return;
            }

            if (current.type != EventType.ContextClick || !rowRect.Contains(current.mousePosition))
                return;

            if (!_selected.Contains(component))
                SelectSingle(component, index);

            ShowContextMenu();
            current.Use();
        }

        private void DrawClipboard()
        {
            ComponentClipboard clipboard = ComponentClipboard.instance;
            List<ComponentPasteStep> plan = ComponentOperations.BuildPastePlan(_target, clipboard.Entries,
                _selected);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Clipboard ({clipboard.Entries.Count})", EditorStyles.boldLabel);

            for (int index = 0; index < plan.Count; index++)
            {
                if (DrawClipboardRow(clipboard, plan[index], index))
                    return;
            }
        }

        private bool DrawClipboardRow(ComponentClipboard clipboard, ComponentPasteStep step, int index)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(step.Entry.DisplayName);

            Color previous = GUI.color;
            GUI.color = GetBadgeColor(step, previous);
            EditorGUILayout.LabelField(BuildBadge(step), EditorStyles.miniLabel, GUILayout.Width(BadgeWidth));
            GUI.color = previous;

            bool isRemoved = GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(ToggleWidth));
            EditorGUILayout.EndHorizontal();

            if (!isRemoved)
                return false;

            clipboard.RemoveAt(index);
            return true;
        }

        private void DrawButtons()
        {
            ComponentClipboard clipboard = ComponentClipboard.instance;

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            using (new EditorGUI.DisabledScope(_selected.Count == 0))
            {
                if (GUILayout.Button("Copy"))
                    clipboard.Copy(_selected);
            }

            using (new EditorGUI.DisabledScope(!clipboard.HasEntries))
            {
                if (GUILayout.Button("Paste"))
                    Paste();

                if (GUILayout.Button("Clear"))
                    clipboard.Clear();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void ShowContextMenu()
        {
            ComponentClipboard clipboard = ComponentClipboard.instance;
            GenericMenu menu = new();

            menu.AddItem(new GUIContent("Copy"), false, func: () => clipboard.Copy(_selected));
            menu.AddItem(new GUIContent("Copy and Add to Clipboard"), false, func: () => clipboard.Append(_selected));

            if (clipboard.HasEntries)
            {
                menu.AddItem(new GUIContent("Paste into Selection"), false, Paste);
                menu.AddItem(new GUIContent("Paste as New (Force Duplicate)"), false, PasteAsNew);
                menu.AddItem(new GUIContent("Paste Values Only"), false, PasteValues);
                menu.AddItem(new GUIContent("Paste to All Selected GameObjects"), false, PasteToSelection);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Paste into Selection"));
                menu.AddDisabledItem(new GUIContent("Paste as New (Force Duplicate)"));
                menu.AddDisabledItem(new GUIContent("Paste Values Only"));
                menu.AddDisabledItem(new GUIContent("Paste to All Selected GameObjects"));
            }

            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Move Up"), false, MoveUp);
            menu.AddItem(new GUIContent("Move Down"), false, MoveDown);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Delete"), false, Delete);
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Select All"), false, SelectAll);
            menu.AddItem(new GUIContent("Deselect All"), false, DeselectAll);
            menu.ShowAsContext();
        }

        private void Paste()
        {
            ComponentOperations.Paste(_target, ComponentClipboard.instance.Entries, _selected);
            Rebuild();
        }

        private void PasteAsNew()
        {
            ComponentOperations.PasteAsNew(_target, ComponentClipboard.instance.Entries);
            Rebuild();
        }

        private void PasteValues()
        {
            ComponentOperations.PasteValues(_target, ComponentClipboard.instance.Entries, _selected);
            Rebuild();
        }

        private void PasteToSelection()
        {
            GameObject[] targets = Selection.gameObjects;

            foreach (GameObject target in targets)
                ComponentOperations.PasteMatchingByType(target, ComponentClipboard.instance.Entries);

            Rebuild();
        }

        private void MoveUp()
        {
            ComponentOperations.MoveUp(_selected);
            Rebuild();
        }

        private void MoveDown()
        {
            ComponentOperations.MoveDown(_selected);
            Rebuild();
        }

        private void Delete()
        {
            ComponentOperations.Delete(_selected);
            _selected.Clear();
            _lastClickedIndex = NoIndex;
            Rebuild();
        }

        private void SelectAll()
        {
            _selected.Clear();

            foreach (Component component in _components)
            {
                if (ComponentOperations.CanCopy(component))
                    _selected.Add(component);
            }
        }

        private void DeselectAll()
        {
            _selected.Clear();
            _lastClickedIndex = NoIndex;
        }

        private void SelectSingle(Component component, int index)
        {
            _selected.Clear();
            _selected.Add(component);
            _lastClickedIndex = index;
        }

        private void SelectRange(int fromIndex, int toIndex)
        {
            int start = Mathf.Min(fromIndex, toIndex);
            int end = Mathf.Max(fromIndex, toIndex);
            _selected.Clear();

            for (int index = start; index <= end; index++)
            {
                if (index >= 0 && index < _components.Count)
                    _selected.Add(_components[index]);
            }
        }

        private void SetSelected(Component component, bool isSelected, int index)
        {
            if (isSelected && !_selected.Contains(component))
                _selected.Add(component);
            else if (!isSelected)
                _selected.Remove(component);

            _lastClickedIndex = index;
            SortSelection();
        }

        private void SortSelection() => _selected.Sort((first, second)
            => _components.IndexOf(first).CompareTo(_components.IndexOf(second)));

        private void Rebuild()
        {
            _target = Selection.activeGameObject;
            _components.Clear();

            if (_target == null)
            {
                _selected.Clear();
                return;
            }

            _components.AddRange(_target.GetComponents<Component>());
            _selected.RemoveAll(component => component == null || !_components.Contains(component));
            SortSelection();
        }
    }
}