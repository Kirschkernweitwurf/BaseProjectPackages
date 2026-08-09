using System;
using System.Collections.Generic;
using Base.ToolPackage.Editor.CodebaseGraph.Analysis;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>
    /// Lists everything that was set aside during triage. Dismissing is easy to do in passing, which is
    /// exactly why it needs somewhere to be looked at afterwards: without this the only record is a JSON
    /// file, and a decision you cannot review is a decision you stop trusting.
    /// </summary>
    public sealed class CodebaseGraphDismissalsWindow : EditorWindow
    {
        private const string CopyLabel = "Copy as instructions";
        private const string CopyMessage = "The instruction block is on the clipboard. Paste it back "
            + "through Update dismissals in the graph window, or hand it to an agent to edit.";

        private const string CountFormat = "{0} dismissed";
        private const string EmptyText = "Nothing is dismissed. Anything you set aside in the graph window "
            + "shows up here, and can be brought back one entry at a time.";

        private const string HeadingClass = "pane-heading";
        private const string KindTitleClass = "section-title";
        private const int MinimumWindowHeight = 320;
        private const int MinimumWindowWidth = 620;
        private const string PaneClass = "pane";
        private const string PlaceholderClass = "pane-placeholder";
        private const string RestoreAllLabel = "Restore all";
        private const string RestoreAllTitle = "Restore every dismissal";
        private const string RestoreLabel = "Restore";
        private const string RestoreTreeLabel = "Restore with contents";
        private const string RestoreTreeTooltip = "Also brings back everything dismissed inside this one, "
            + "which is the exact reverse of dismissing with contents.";

        private const string RootClass = "codebase-graph-root";
        private const string RowClass = "dismissal-row";
        private const string RowNameClass = "dismissal-name";
        private const string RowScopeClass = "dismissal-scope";
        private const string ScopeAlone = "this entry only";
        private const string ScopeWithContents = "with everything inside";
        private const string SearchPlaceholder = "Search";
        private const string StyleSheetFilter = "CodebaseGraph t:StyleSheet";
        private const string ToolbarRowClass = "top-bar";
        private const string WindowTitle = "Dismissed findings";

        private readonly List<DismissalEntry> _entries = new();

        private ScrollView _list;
        private Label _countLabel;
        private Button _restoreAllButton;
        private string _search = string.Empty;

#region Unity Callbacks
        private void CreateGUI()
        {
            rootVisualElement.AddToClassList(RootClass);
            LoadStyleSheet();

            rootVisualElement.Add(BuildToolbar());

            _countLabel = new Label(string.Empty);
            _countLabel.AddToClassList(HeadingClass);
            rootVisualElement.Add(_countLabel);

            _list = new ScrollView();
            _list.AddToClassList(PaneClass);
            rootVisualElement.Add(_list);

            DismissalStore.Changed += Rebuild;
            Rebuild();
        }

        private void OnDisable() => DismissalStore.Changed -= Rebuild;
#endregion

        /// <summary>Opens the window.</summary>
        public static void Open()
        {
            CodebaseGraphDismissalsWindow window = GetWindow<CodebaseGraphDismissalsWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(MinimumWindowWidth, MinimumWindowHeight);
        }

        private static Label BuildLabel(string text, string styleClass)
        {
            Label label = new(text);
            label.AddToClassList(styleClass);
            return label;
        }

        private VisualElement BuildToolbar()
        {
            Toolbar toolbar = new();
            toolbar.AddToClassList(ToolbarRowClass);

            _restoreAllButton = new ToolbarButton(RestoreAll) { text = RestoreAllLabel };
            toolbar.Add(_restoreAllButton);

            toolbar.Add(new ToolbarButton(CopyInstructions) { text = CopyLabel });

            ToolbarSearchField search = new();
            search.tooltip = SearchPlaceholder;
            search.RegisterValueChangedCallback(OnSearchChanged);
            toolbar.Add(search);

            return toolbar;
        }

        private void Rebuild()
        {
            _entries.Clear();
            _entries.AddRange(DismissalStore.Collect());

            _countLabel.text = string.Format(CountFormat, _entries.Count);
            _restoreAllButton.SetEnabled(_entries.Count > 0);

            _list.Clear();

            if (_entries.Count == 0)
            {
                _list.Add(BuildLabel(EmptyText, PlaceholderClass));
                return;
            }

            EDismissalKind? lastKind = null;

            foreach (DismissalEntry entry in _entries)
            {
                if (!IsMatch(entry))
                    continue;

                if (lastKind != entry.Kind)
                {
                    _list.Add(BuildLabel(entry.Kind.ToString(), KindTitleClass));
                    lastKind = entry.Kind;
                }

                _list.Add(BuildRow(entry));
            }
        }

        private bool IsMatch(DismissalEntry entry)
            => string.IsNullOrEmpty(_search)
                || entry.DisplayName.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0;

        private VisualElement BuildRow(DismissalEntry entry)
        {
            VisualElement row = new();
            row.AddToClassList(RowClass);

            VisualElement text = new();
            text.style.flexGrow = 1f;
            text.Add(BuildLabel(entry.DisplayName, RowNameClass));

            text.Add(BuildLabel(entry.IncludesContents
                    ? ScopeWithContents
                    : ScopeAlone,
                RowScopeClass));

            row.Add(text);
            row.Add(new Button(() => Restore(entry)) { text = RestoreLabel });

            if (entry.CanHoldContents)
            {
                row.Add(new Button(() => RestoreTree(entry))
                {
                    text = RestoreTreeLabel,
                    tooltip = RestoreTreeTooltip
                });
            }

            return row;
        }

        private void Restore(DismissalEntry entry)
        {
            DismissalStore.Restore(entry.Id);
            Rebuild();
        }

        private void RestoreTree(DismissalEntry entry)
        {
            DismissalStore.RestoreWithContents(entry.Id);
            Rebuild();
        }

        private void RestoreAll()
        {
            bool confirmed = EditorUtility.DisplayDialog(RestoreAllTitle,
                $"Bring back all {_entries.Count} dismissed entries?",
                "Restore",
                "Cancel");

            if (!confirmed)
                return;

            DismissalStore.RestoreAll();
            Rebuild();
        }

        private void CopyInstructions()
        {
            EditorGUIUtility.systemCopyBuffer = DismissalTextFormat.Write();
            EditorUtility.DisplayDialog(WindowTitle, CopyMessage, "OK");
        }

        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            _search = evt.newValue ?? string.Empty;
            Rebuild();
        }

        private void LoadStyleSheet()
        {
            foreach (string guid in AssetDatabase.FindAssets(StyleSheetFilter))
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
