using System;
using System.Collections.Generic;
using Base.EditorUIPackage.Editor;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Base.ToolsPackage.Editor.AssemblyGraph
{
    /// <summary>Editor window that visualizes project assemblies and their references.</summary>
    internal sealed class AssemblyGraphWindow : EditorWindow
    {
        private const string ConfirmCancel = "Cancel";
        private const string ConfirmOk = "Remove";
        private const string ConfirmTitle = "Remove references";
        private const string MenuPath = "Tools/Base Packages/Unity Editor/Project Health/Assembly Graph";
        private const float MinWindowHeight = 420f;
        private const float MinWindowWidth = 720f;
        private const string MissingSheetMessage = "The assembly graph style sheet is missing, so the "
            + "nodes are drawn unstyled.";
        private const string RestoreLabel = "Restore Last";
        private const string RestoreTooltip = "Put back the asmdef this window last rewrote, in case "
            + "the removal broke the compile.";

        /// <summary>The GUID of this window's own sheet, from its meta file.</summary>
        private const string SheetGuid = "eaf1ce2966367bf458751be0e860234d";

        private const string WindowTitle = "Assembly Graph";

        private bool HasFocus => !string.IsNullOrEmpty(_focusedName);

        private AssemblyGraphView _graphView;
        private ToolbarSearchField _searchField;
        private ToolbarButton _clearFocusButton;
        private ToolbarButton _restoreButton;
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
            // The shared look first, then this window's own sheet on top of it, which only carries
            // the GraphView pieces the shared classes cannot reach. Reload is handed over because the
            // nodes paint their own containers, and those are only written while a node is built.
            EditorUssTheme.Apply(rootVisualElement, Reload);

            // The toolbar goes up before the sheet is attached, because that is where the status
            // label lives and a missing sheet is reported through it.
            rootVisualElement.Add(BuildToolbar());

            LoadStyleSheet();

            _graphView = new AssemblyGraphView(OnFocusRequested, OnNodeCleanupRequested);
            rootVisualElement.Add(_graphView);

            Reload();
        }
#endregion

        /// <summary>Opens the window, or focuses it when it is already open.</summary>
        [DynamicMenuItem(MenuPath)]
        public static void Open()
        {
            AssemblyGraphWindow window = GetWindow<AssemblyGraphWindow>();

            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
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

        private static HashSet<string> CollectCandidates(AssemblyNodeInfo node)
        {
            HashSet<string> set = new();
            foreach (AssemblyReferenceInfo reference in node.References)
            {
                if (reference.IsCandidate)
                    set.Add(reference.TargetName);
            }

            return set;
        }

        /// <summary>
        /// Says what the check saw and what it cannot see, because the listing is evidence rather
        /// than a verdict and acting on it without a recompile is how a build gets broken.
        /// </summary>
        private static string BuildConfirmMessage(AssemblyNodeInfo node, int referenceCount)
            => $"This removes {referenceCount} reference(s) from {node.Name}.\n\n"
                + "A reference is kept when the compiled metadata names it, when it declares an "
                + "ancestor of something the metadata names, or when a using directive names one of "
                + "its namespaces. Anything needed through a path none of those three see is listed "
                + "here anyway.\n\n"
                + "Let Unity recompile and read the console. Restore Last puts the file back.";

        private Toolbar BuildToolbar()
        {
            Toolbar toolbar = new();

            toolbar.Add(new ToolbarButton(Reload)
            {
                text = "Refresh"
            });

            _restoreButton = new ToolbarButton(RestoreLastCleanup)
            {
                text = RestoreLabel,
                tooltip = RestoreTooltip
            };

            toolbar.Add(_restoreButton);

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

            VisualElement spacer = new()
            {
                style =
                {
                    flexGrow = 1f
                }
            };

            toolbar.Add(spacer);

            _statusLabel = new Label
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleRight,
                    marginRight = 8f
                }
            };

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

                if (_onlyIssues && !node.HasCandidateReferences)
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
            {
                if (names.Contains(node.Name))
                    visible.Add(node);
            }

            return visible;
        }

        private AssemblyNodeInfo FindNode(string nodeName)
        {
            foreach (AssemblyNodeInfo node in _allNodes)
            {
                if (node.Name == nodeName)
                    return node;
            }

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

            if (_restoreButton != null)
                _restoreButton.SetEnabled(AsmdefBackupStore.HasBackup);

            if (_statusLabel == null)
                return;

            _statusLabel.text = HasFocus
                ? $"Focus: {_focusedName}  |  {visibleCount} shown"
                : $"{visibleCount} shown / {_allNodes.Count} total";
        }

        /// <summary>
        /// Removals go one assembly at a time on purpose. Every listing here can be wrong, and a
        /// single wrong one stops its dependents from compiling, which hides the rest of the damage
        /// until the first is fixed. One file per recompile keeps the cause and the error together.
        /// </summary>
        private void OnNodeCleanupRequested(AssemblyNodeInfo node)
        {
            if (!node.IsCleanable)
                return;

            HashSet<string> candidates = CollectCandidates(node);
            if (candidates.Count == 0)
                return;

            bool confirmed = EditorUtility.DisplayDialog(ConfirmTitle,
                BuildConfirmMessage(node, candidates.Count),
                ConfirmOk,
                ConfirmCancel);

            if (!confirmed)
                return;

            int removed = AsmdefReferenceCleaner.RemoveReferences(node.AsmdefPath, candidates);
            AssetDatabase.Refresh();

            SetStatus($"Removed {removed} reference(s) from {node.Name}. Recompiling, then press Refresh.");
        }

        private void RestoreLastCleanup()
        {
            string restored = AsmdefBackupStore.Restore();
            if (string.IsNullOrEmpty(restored))
                return;

            AssetDatabase.Refresh();

            SetStatus($"Restored {restored}. Recompiling, then press Refresh.");
        }

        private void SetStatus(string message)
        {
            if (_restoreButton != null)
                _restoreButton.SetEnabled(AsmdefBackupStore.HasBackup);

            if (_statusLabel == null)
                return;

            _statusLabel.text = message;
        }

        // By GUID rather than by name search, which answered with whatever file in the project
        // happened to be called this and would style the window from a stranger's sheet.
        private void LoadStyleSheet()
        {
            if (EditorStyleSheets.Apply(rootVisualElement, SheetGuid))
                return;

            if (_statusLabel != null)
                _statusLabel.text = MissingSheetMessage;
        }
    }
}