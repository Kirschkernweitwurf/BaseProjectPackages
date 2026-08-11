using System;
using System.Collections.Generic;
using Base.AttributePackage.Editor.Windows.AttributeTroubleshoot.Showcase;
using Base.ToolPackage.MenuManagerWindow;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot
{
    /// <summary>
    /// Editor window that lists every attribute usage that cannot work as written. The drawers and
    /// handlers fail quietly by design, falling back to the plain field so a broken attribute never
    /// breaks the inspector, which means these mistakes are otherwise only visible while the affected
    /// object happens to be selected.
    /// </summary>
    /// <remarks>
    /// Three tabs: the project scan, a scan of deliberately broken sample types so the report can be
    /// read on a healthy project, and a live inspector of a demo asset so the attributes can be seen
    /// working rather than only described.
    /// </remarks>
    internal sealed class AttributeTroubleshootWindow : EditorWindow
    {
        private const float BarSpacing = 4f;
        private const float ButtonHeight = 26f;
        private const float ButtonWidth = 140f;
        private const string ErrorsOnlyLabel = "Errors only";
        private const float ErrorsOnlyWidth = 90f;
        private const float ListSpacing = 4f;
        private const float MinimumHeight = 320f;
        private const float MinimumWidth = 460f;
        private const string NotScannedMessage = "Not scanned yet.";
        private const string SampleNotice =
            "These findings come from types that are broken on purpose, so the report can be read on a "
            + "project that has nothing wrong. They are excluded from the project scan.";
        private const string ScanLabel = "Scan";
        private const float SearchHeight = 20f;
        private const string SearchHint = "Nothing matches the current filter.";
        private const float SearchWidth = 220f;
        private const string SuccessMessage = "No problems found.";

        private static readonly string[] TabLabels =
        {
            "Project",
            "Samples",
            "Showcase"
        };

        [SerializeField] private bool errorsOnly;
        [SerializeField] private ETroubleshootTab tab = ETroubleshootTab.Project;
        [SerializeField] private string search = string.Empty;
        [SerializeField] private Vector2 scrollPosition;
        [SerializeField] private Vector2 showcaseScrollPosition;

        // Created on first use and never saved, so the showcase can be edited freely without leaving
        // anything behind in the project.
        private AttributeShowcase ShowcaseAsset
        {
            get
            {
                if (_showcase == null)
                {
                    _showcase = CreateInstance<AttributeShowcase>();
                    _showcase.hideFlags = HideFlags.DontSave;
                }

                return _showcase;
            }
        }

        private readonly AttributeTroubleshootStyles _styles = new();

        private List<AttributeIssueGroup> _groups = new();
        private AttributeShowcase _showcase;
        private bool _scanned;
        private int _errors;
        private int _warnings;

#region Unity Callbacks
        private void OnEnable()
        {
            titleContent = new GUIContent(TroubleshootWindowInfo.WindowTitle);
            AssemblyReloadEvents.afterAssemblyReload += Invalidate;
        }

        private void OnGUI()
        {
            _styles.EnsureBuilt();

            DrawTabs();

            if (tab == ETroubleshootTab.Showcase)
            {
                DrawShowcase();
                return;
            }

            DrawActionBar();
            DrawSummary();

            if (!_scanned)
                return;

            if (_errors == 0 && _warnings == 0)
            {
                AttributeTroubleshootView.DrawSuccess(_styles);
                return;
            }

            DrawIssues();
        }

        private void OnDisable()
        {
            AssemblyReloadEvents.afterAssemblyReload -= Invalidate;

            if (_showcase != null)
                DestroyImmediate(_showcase);
        }
#endregion

        [DynamicMenuItem(TroubleshootWindowInfo.MenuPath)]
        private static void OpenWindow()
        {
            AttributeTroubleshootWindow window = GetWindow<AttributeTroubleshootWindow>();

            window.minSize = new Vector2(MinimumWidth, MinimumHeight);
            window.Show();
        }

        // Opening the script is the point of a finding, so a failed lookup pings the asset instead of
        // doing nothing, which would read as a dead click.
        private static void Open(Type type)
        {
            if (!ScriptLocator.Open(type))
                ScriptLocator.Ping(type);
        }

        private static string Plural(int count, string word) => count == 1
            ? word
            : word + "s";

        private void DrawTabs()
        {
            EditorGUILayout.Space(BarSpacing);

            int selected = GUILayout.Toolbar((int)tab, TabLabels);
            if (selected == (int)tab)
                return;

            tab = (ETroubleshootTab)selected;
            Invalidate();

            // Scanning a handful of sample types is instant, so that tab shows its report right away
            // instead of asking for a click that could only ever produce one answer.
            if (tab == ETroubleshootTab.Samples)
                Rescan();

            // The tab switch changes the layout mid-event, so this frame is abandoned rather than left
            // to mismatch what the layout pass measured.
            GUIUtility.ExitGUI();
        }

        private void DrawActionBar()
        {
            EditorGUILayout.Space(BarSpacing);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(BarSpacing, false);

            if (GUILayout.Button(ScanLabel, GUILayout.Height(ButtonHeight), GUILayout.Width(ButtonWidth)))
            {
                Rescan();
                GUIUtility.ExitGUI();
            }

            GUILayout.FlexibleSpace();

            errorsOnly = GUILayout.Toggle(errorsOnly, ErrorsOnlyLabel, EditorStyles.toolbarButton,
                GUILayout.Width(ErrorsOnlyWidth), GUILayout.Height(SearchHeight));

            EditorGUILayout.Space(BarSpacing, false);

            search = EditorGUILayout.TextField(search, EditorStyles.toolbarSearchField,
                GUILayout.Width(SearchWidth), GUILayout.Height(SearchHeight));

            EditorGUILayout.Space(BarSpacing, false);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(BarSpacing);
        }

        private void DrawSummary()
        {
            if (tab == ETroubleshootTab.Samples)
                EditorGUILayout.HelpBox(SampleNotice, MessageType.Info);

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label(SummaryText(), _styles.Summary);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawIssues()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            GUILayout.Space(ListSpacing);

            Type clicked = AttributeTroubleshootView.DrawGroups(_groups, search, errorsOnly, _styles,
                out bool anyShown);

            if (!anyShown)
                EditorGUILayout.LabelField(SearchHint, EditorStyles.centeredGreyMiniLabel);

            GUILayout.Space(ListSpacing);
            EditorGUILayout.EndScrollView();

            if (clicked != null)
                Open(clicked);
        }

        private void DrawShowcase()
        {
            EditorGUILayout.Space(BarSpacing);

            showcaseScrollPosition = EditorGUILayout.BeginScrollView(showcaseScrollPosition);
            AttributeShowcaseView.Draw(ShowcaseAsset);
            EditorGUILayout.EndScrollView();
        }

        private string SummaryText()
        {
            if (!_scanned)
                return NotScannedMessage;

            if (_errors == 0 && _warnings == 0)
                return SuccessMessage;

            return $"{_errors} {Plural(_errors, "error")}, {_warnings} {Plural(_warnings, "warning")} "
                + $"across {_groups.Count} {Plural(_groups.Count, "type")}.";
        }

        private void Rescan()
        {
            _groups = tab == ETroubleshootTab.Samples
                ? AttributeTroubleshootCollector.CollectSamples(out _errors, out _warnings)
                : AttributeTroubleshootCollector.CollectProject(out _errors, out _warnings);

            _scanned = true;
            Repaint();
        }

        // A domain reload or a tab switch can change every answer, so the previous result is dropped
        // rather than shown as if it were still current.
        private void Invalidate()
        {
            _groups = new List<AttributeIssueGroup>();
            _errors = 0;
            _warnings = 0;
            _scanned = false;
            Repaint();
        }
    }
}