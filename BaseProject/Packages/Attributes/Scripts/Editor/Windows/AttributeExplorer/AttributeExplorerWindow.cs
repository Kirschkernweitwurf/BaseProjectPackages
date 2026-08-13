using System;
using System.Collections.Generic;
using Base.AttributePackage.Editor.Windows.AttributeExplorer.Reference;
using Base.AttributePackage.Editor.Windows.AttributeExplorer.Showcase;
using Base.AttributePackage.Editor.Windows.AttributeExplorer.Troubleshoot;
using Base.ToolPackage.MenuManagerWindow;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeExplorer
{
    /// <summary>
    /// One window for everything about the attribute package: a reference page per attribute, a showcase
    /// of all of them at once, and a scan for usages that cannot work as written.
    /// </summary>
    /// <remarks>
    /// One window rather than two, because the four views answer the same question from different ends.
    /// Somebody who does not know an attribute exists starts at the reference; somebody whose attribute
    /// is not doing anything starts at the scan; and having those in separate windows meant the second
    /// one was never found by the person who needed it.
    /// </remarks>
    internal sealed class AttributeExplorerWindow : EditorWindow
    {
        private const float BarSpacing = 4f;
        private const float ButtonHeight = 24f;
        private const float ButtonWidth = 120f;
        private const string CopiedNotice = "Report copied";
        private const string CopyLabel = "Copy report";
        private const string ErrorsOnlyLabel = "Errors only";
        private const float ErrorsOnlyWidth = 88f;
        private const float ListSpacing = 6f;
        private const float MinimumHeight = 420f;
        private const float MinimumWidth = 760f;
        private const float NotificationFade = 0.8f;
        private const string NotScannedMessage = "Not scanned yet.";
        private const string ScanLabel = "Scan";
        private const float ShowcasePadding = 10f;
        private const float SearchHeight = 20f;
        private const string SearchHint = "Nothing matches the current filter.";
        private const float SearchWidth = 200f;
        private const string SuccessMessage = "No problems found.";
        private const float TabBarHeight = 26f;
        private const string DemoLabel = "Demo types";
        private const string DemoNotice = "Showing types that are broken on purpose, so the scan can be "
            + "seen working on a project that has nothing wrong. These are never part of a project scan.";
        private const float DemoWidth = 88f;

        private static readonly GUIContent CopiedContent = new(CopiedNotice);
        private static readonly GUIContent TitleContent = new(AttributeExplorerInfo.WindowTitle);

        [SerializeField] private EAttributeExplorerTab tab = EAttributeExplorerTab.Reference;
        [SerializeField] private AttributeReferencePane reference = new();
        [SerializeField] private bool errorsOnly;
        [SerializeField] private bool demoTypes;
        [SerializeField] private string search = string.Empty;
        [SerializeField] private Vector2 findingsScroll;
        [SerializeField] private Vector2 showcaseScroll;

        private readonly AttributeExplorerStyles _styles = new();
        private readonly AttributeTroubleshootStyles _findingStyles = new();

        private List<AttributeIssueGroup> _groups = new();
        private AttributeShowcase _showcase;
        private bool _scanned;
        private int _errors;
        private int _warnings;

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

#region Unity Callbacks
        private void OnEnable()
        {
            titleContent = TitleContent;

            // The reference list highlights the row under the pointer, which only moves between events.
            // Without this the window would either miss the move or repaint on every single frame.
            wantsMouseMove = true;

            AssemblyReloadEvents.afterAssemblyReload += Invalidate;
        }

        private void OnGUI()
        {
            _styles.EnsureBuilt();
            _findingStyles.EnsureBuilt();

            if (Event.current.type == EventType.MouseMove)
                Repaint();

            // The whole window is two rectangles taken from its own size, so nothing above the body can
            // reserve a width the body then has to guess at.
            Rect bar = new(0f, 0f, position.width, TabBarHeight);
            Rect body = new(0f, TabBarHeight, position.width, position.height - TabBarHeight);

            DrawTabBar(bar);

            if (tab == EAttributeExplorerTab.Reference)
            {
                reference.Draw(body, _styles, this);
                return;
            }

            GUILayout.BeginArea(body);

            if (tab == EAttributeExplorerTab.Showcase)
                DrawShowcase();
            else
                DrawFindings();

            GUILayout.EndArea();
        }

        private void OnDisable()
        {
            AssemblyReloadEvents.afterAssemblyReload -= Invalidate;

            reference.Release();
            _styles.Dispose();

            if (_showcase != null)
                DestroyImmediate(_showcase);
        }
#endregion

        [DynamicMenuItem(AttributeExplorerInfo.MenuPath)]
        private static void OpenWindow()
        {
            AttributeExplorerWindow window = GetWindow<AttributeExplorerWindow>();

            window.titleContent = TitleContent;
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

        private void DrawTabBar(Rect bar)
        {
            EAttributeExplorerTab picked = AttributeExplorerTabBar.Draw(bar, tab, _styles);

            if (picked == tab)
                return;

            tab = picked;
            Invalidate();

            // The tab switch changes the layout mid-event, so this frame is abandoned rather than left to
            // mismatch what the layout pass measured.
            GUIUtility.ExitGUI();
        }

        private void DrawShowcase()
        {
            EditorGUILayout.Space(BarSpacing);

            showcaseScroll = EditorGUILayout.BeginScrollView(showcaseScroll);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(ShowcasePadding, false);
            EditorGUILayout.BeginVertical();

            AttributeShowcaseView.Draw(ShowcaseAsset);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(ShowcasePadding, false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
        }

        private void DrawFindings()
        {
            DrawActionBar();
            DrawSummary();

            if (!_scanned)
                return;

            if (_errors == 0 && _warnings == 0)
            {
                AttributeTroubleshootView.DrawSuccess(_findingStyles);
                return;
            }

            DrawGroups();
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

            EditorGUILayout.Space(BarSpacing, false);

            // Copying respects the errors-only toggle, because the report you want to send is almost
            // always the one you are currently looking at.
            using (new EditorGUI.DisabledScope(_groups.Count == 0))
            {
                if (GUILayout.Button(CopyLabel, GUILayout.Height(ButtonHeight), GUILayout.Width(ButtonWidth)))
                    CopyReport();
            }

            GUILayout.FlexibleSpace();

            // The toggle and the search box are toolbar controls and shorter than the buttons beside them,
            // so they are centered against the bar rather than sitting on its top edge.
            EditorGUILayout.BeginVertical(GUILayout.Height(ButtonHeight));
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();

            errorsOnly = GUILayout.Toggle(errorsOnly, ErrorsOnlyLabel, EditorStyles.toolbarButton,
                GUILayout.Width(ErrorsOnlyWidth), GUILayout.Height(SearchHeight));

            EditorGUILayout.Space(BarSpacing, false);

            // A filter rather than a tab of its own. It is the same report over a different set of
            // types, and no name for a place that shows deliberately broken code read well.
            EditorGUI.BeginChangeCheck();

            demoTypes = GUILayout.Toggle(demoTypes, DemoLabel, EditorStyles.toolbarButton,
                GUILayout.Width(DemoWidth), GUILayout.Height(SearchHeight));

            if (EditorGUI.EndChangeCheck())
            {
                Rescan();
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.Space(BarSpacing, false);

            search = EditorGUILayout.TextField(search, EditorStyles.toolbarSearchField,
                GUILayout.Width(SearchWidth), GUILayout.Height(SearchHeight));

            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(BarSpacing, false);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(BarSpacing);
        }

        private void DrawSummary()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(BarSpacing, false);
            EditorGUILayout.BeginVertical();

            if (demoTypes)
                EditorGUILayout.HelpBox(DemoNotice, MessageType.Info);

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label(SummaryText(), _findingStyles.Summary);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(BarSpacing, false);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawGroups()
        {
            findingsScroll = EditorGUILayout.BeginScrollView(findingsScroll);
            GUILayout.Space(ListSpacing);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(BarSpacing, false);
            EditorGUILayout.BeginVertical();

            Type clicked = AttributeTroubleshootView.DrawGroups(_groups, search, errorsOnly, _findingStyles,
                out bool anyShown);

            if (!anyShown)
                EditorGUILayout.LabelField(SearchHint, EditorStyles.centeredGreyMiniLabel);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(BarSpacing, false);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(ListSpacing);
            EditorGUILayout.EndScrollView();

            if (clicked != null)
                Open(clicked);
        }

        private void CopyReport()
        {
            string report = AttributeReportFormatter.Build(_groups, errorsOnly);

            if (string.IsNullOrEmpty(report))
                return;

            EditorGUIUtility.systemCopyBuffer = report;
            ShowNotification(CopiedContent, NotificationFade);
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
            _groups = demoTypes
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