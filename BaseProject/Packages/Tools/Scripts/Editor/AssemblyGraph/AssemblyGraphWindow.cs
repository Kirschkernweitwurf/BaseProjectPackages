using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.AssemblyGraph
{
    /// <summary>Editor window that visualizes project assemblies and their references.</summary>
    public sealed class AssemblyGraphWindow : EditorWindow
    {
        private bool HasFocus => !string.IsNullOrEmpty(_focusedName);

        private AssemblyGraphView _graphView;
        private ToolbarSearchField _searchField;
        private ToolbarButton _clearFocusButton;
        private Label _statusLabel;

        private List<AssemblyNodeInfo> _allNodes = new();

        private bool _showPackages = true;
        private bool _showUnityPackages;
        private bool _showLibrary;
        private bool _onlyIssues;
        private string _search = string.Empty;
        private string _focusedName;

#region Unity Callbacks
        private void CreateGUI()
        {
            LoadStyleSheet();
            rootVisualElement.Add(BuildToolbar());

            _graphView = new AssemblyGraphView(OnFocusRequested, OnNodeCleanupRequested);
            rootVisualElement.Add(_graphView);

            Reload();
        }
#endregion

        [MenuItem("Tools/Assembly Graph")]
        public static void Open()
        {
            AssemblyGraphWindow window = GetWindow<AssemblyGraphWindow>();
            window.titleContent = new GUIContent("Assembly Graph");
            window.minSize = new Vector2(720f, 420f);
        }

        private static ToolbarToggle BuildToggle(string label, bool initialValue, Action<bool> onChanged)
        {
            ToolbarToggle toggle = new()
            {
                text = label,
                value = initialValue
            };

            toggle.RegisterValueChangedCallback(evt => onChanged(evt.newValue));
            return toggle;
        }

        private static HashSet<string> CollectUnused(AssemblyNodeInfo node)
        {
            HashSet<string> set = new();
            foreach (AssemblyReferenceInfo reference in node.References)
                if (reference.IsUnused)
                    set.Add(reference.TargetName);

            return set;
        }

        private static string BuildConfirmMessage(int assemblyCount, int referenceCount)
            => $"This removes {referenceCount} reference(s) from {assemblyCount} assembly file(s).\n\n"
                + "Only your own assemblies are touched. Unity packages and libraries are never modified.\n\n"
                + "Detection reads compiled metadata. A reference that only supplies a constant value can look unused "
                + "but still be needed. Commit your work first, then let Unity recompile and check the console for errors.";

        private Toolbar BuildToolbar()
        {
            Toolbar toolbar = new();

            toolbar.Add(new ToolbarButton(Reload)
            {
                text = "Refresh"
            });

            toolbar.Add(new ToolbarButton(CleanUpAll)
            {
                text = "Clean Up All"
            });

            _clearFocusButton = new ToolbarButton(ClearFocus)
            {
                text = "Clear Focus"
            };

            toolbar.Add(_clearFocusButton);

            toolbar.Add(BuildToggle("Packages", _showPackages, onChanged: value =>
            {
                _showPackages = value;
                ApplyFilter();
            }));

            toolbar.Add(BuildToggle("Unity Packages", _showUnityPackages, onChanged: value =>
            {
                _showUnityPackages = value;
                ApplyFilter();
            }));

            toolbar.Add(BuildToggle("Library", _showLibrary, onChanged: value =>
            {
                _showLibrary = value;
                ApplyFilter();
            }));

            toolbar.Add(BuildToggle("Only issues", _onlyIssues, onChanged: value =>
            {
                _onlyIssues = value;
                ApplyFilter();
            }));

            _searchField = new ToolbarSearchField();
            _searchField.RegisterValueChangedCallback(OnSearchChanged);
            toolbar.Add(_searchField);

            VisualElement spacer = new();
            spacer.style.flexGrow = 1f;
            toolbar.Add(spacer);

            _statusLabel = new Label();
            _statusLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            _statusLabel.style.marginRight = 8f;
            toolbar.Add(_statusLabel);

            return toolbar;
        }

        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            _search = string.IsNullOrEmpty(evt.newValue)
                ? string.Empty
                : evt.newValue.ToLowerInvariant();

            ApplyFilter();
        }

        private void Reload()
        {
            _allNodes = AssemblyGraphModel.Build();
            ApplyFilter();
        }

        /// <summary>Toggles focus on the clicked assembly.</summary>
        private void OnFocusRequested(AssemblyNodeInfo node)
        {
            _focusedName = _focusedName == node.Name
                ? null
                : node.Name;

            ApplyFilter();
        }

        private void ClearFocus()
        {
            if (!HasFocus)
                return;

            _focusedName = null;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (_graphView == null)
                return;

            List<AssemblyNodeInfo> visible = HasFocus
                ? CollectFocusSet()
                : CollectFilteredSet();

            _graphView.Rebuild(visible, _focusedName);
            UpdateToolbarState(visible.Count);
        }

        private List<AssemblyNodeInfo> CollectFilteredSet()
        {
            List<AssemblyNodeInfo> visible = new();

            foreach (AssemblyNodeInfo node in _allNodes)
            {
                if (!IsKindVisible(node))
                    continue;

                if (_onlyIssues && !node.HasUnusedReferences)
                    continue;

                if (!MatchesSearch(node))
                    continue;

                visible.Add(node);
            }

            return visible;
        }

        /// <summary>
        /// Returns the focused assembly plus every direct neighbor in both directions.
        /// Kind and issue filters are ignored so the dependency picture stays complete.
        /// </summary>
        private List<AssemblyNodeInfo> CollectFocusSet()
        {
            AssemblyNodeInfo focused = FindNode(_focusedName);
            if (focused == null)
            {
                _focusedName = null;
                return CollectFilteredSet();
            }

            HashSet<string> names = new()
            {
                focused.Name
            };

            foreach (AssemblyReferenceInfo reference in focused.References)
                names.Add(reference.TargetName);

            foreach (AssemblyNodeInfo node in _allNodes)
            {
                foreach (AssemblyReferenceInfo reference in node.References)
                {
                    if (reference.TargetName != focused.Name)
                        continue;

                    names.Add(node.Name);
                    break;
                }
            }

            List<AssemblyNodeInfo> visible = new();
            foreach (AssemblyNodeInfo node in _allNodes)
                if (names.Contains(node.Name))
                    visible.Add(node);

            return visible;
        }

        private AssemblyNodeInfo FindNode(string name)
        {
            foreach (AssemblyNodeInfo node in _allNodes)
                if (node.Name == name)
                    return node;

            return null;
        }

        private bool MatchesSearch(AssemblyNodeInfo node)
        {
            if (string.IsNullOrEmpty(_search))
                return true;

            return node.Name.ToLowerInvariant().Contains(_search);
        }

        private bool IsKindVisible(AssemblyNodeInfo node)
        {
            switch (node.Kind)
            {
                case EAssemblyKind.Project:
                    return true;
                case EAssemblyKind.Package:
                    return _showPackages;
                case EAssemblyKind.UnityPackage:
                    return _showUnityPackages;
                default:
                    return _showLibrary;
            }
        }

        private void UpdateToolbarState(int visibleCount)
        {
            if (_clearFocusButton != null)
                _clearFocusButton.SetEnabled(HasFocus);

            if (_statusLabel == null)
                return;

            _statusLabel.text = HasFocus
                ? $"Focus: {_focusedName}  |  {visibleCount} shown"
                : $"{visibleCount} shown / {_allNodes.Count} total";
        }

        private void OnNodeCleanupRequested(AssemblyNodeInfo node)
        {
            if (!node.IsCleanable)
                return;

            HashSet<string> unused = CollectUnused(node);
            if (unused.Count == 0)
                return;

            bool confirmed = EditorUtility.DisplayDialog("Remove unused references",
                BuildConfirmMessage(1, unused.Count),
                "Remove",
                "Cancel");

            if (!confirmed)
                return;

            FinishCleanup(AsmdefReferenceCleaner.RemoveReferences(node.AsmdefPath, unused));
        }

        private void CleanUpAll()
        {
            Dictionary<string, HashSet<string>> plan = new();
            int totalReferences = 0;

            foreach (AssemblyNodeInfo node in _allNodes)
            {
                if (!node.IsCleanable || !node.HasUnusedReferences)
                    continue;

                HashSet<string> unused = CollectUnused(node);
                if (unused.Count == 0)
                    continue;

                plan[node.AsmdefPath] = unused;
                totalReferences += unused.Count;
            }

            if (plan.Count == 0)
            {
                EditorUtility.DisplayDialog("Assembly Graph", "No unused references found.", "OK");
                return;
            }

            bool confirmed = EditorUtility.DisplayDialog("Remove unused references",
                BuildConfirmMessage(plan.Count, totalReferences),
                "Remove",
                "Cancel");

            if (!confirmed)
                return;

            int removed = 0;
            foreach (KeyValuePair<string, HashSet<string>> entry in plan)
                removed += AsmdefReferenceCleaner.RemoveReferences(entry.Key, entry.Value);

            FinishCleanup(removed);
        }

        private void FinishCleanup(int removed)
        {
            AssetDatabase.Refresh();

            if (_statusLabel != null)
                _statusLabel.text = $"Removed {removed} reference(s). Recompiling, press Refresh when done.";
        }

        private void LoadStyleSheet()
        {
            foreach (string guid in AssetDatabase.FindAssets("AssemblyGraph t:StyleSheet"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                StyleSheet sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                if (sheet == null)
                    continue;

                rootVisualElement.styleSheets.Add(sheet);
                return;
            }
        }
    }
}