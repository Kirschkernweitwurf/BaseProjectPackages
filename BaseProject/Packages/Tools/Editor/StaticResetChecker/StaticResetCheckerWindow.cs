using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Base.EditorUiPackage;
using Base.UtilityPackage.Logging;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.ToolPackage.Editor.StaticResetChecker
{
    /// <summary>
    /// Editor window for scanning static fields that are not reset on Enter Play Mode.
    /// <br/><br/>
    /// The scanner is a pure text analysis tool that looks for "static" declarations
    /// and checks if their names appear in reset methods in the same file.
    /// <br/><br/>
    /// Fields that are not touched in any reset method are reported as findings,
    /// with a link to the source line. You can configure the reset attributes, ignore marker and other options.
    /// This is useful for projects that have "Reset Domain" disabled in the Editor settings,
    /// to find static state that might persist across play sessions and cause bugs.
    /// <br/><br/>
    /// </summary>
    /// <remarks>
    /// To suppress a false positive, add the ignore marker as a comment on the field line (e.g. "reset-ignore").
    /// </remarks>
    internal sealed class StaticResetCheckerWindow : EditorWindow
    {
        private const float CopyButtonWidth = 110f;
        private const string CopyLabel = "Copy report";
        private const string Description = "Finds static fields, events and auto-properties that no reset "
            + "method touches. With Domain Reload off they keep their value between play sessions, which "
            + "is where the state that survives a stop comes from.";
        private const string EmptyHint = "Press Scan to read the folder above.";
        private const string EmptyMessage = "Nothing scanned yet";
        private const string EmptyOkMessage = "No unreset statics";
        private const string MenuPath = "Tools/Base Packages/Code/Health/Static Reset Checker";
        private const float MinWindowHeight = 360f;
        private const float MinWindowWidth = 420f;
        private const string NextLabel = "Next";
        private const string OptionsHeader = "Options";
        private const float PageButtonWidth = 70f;
        private const string PageFormat = "Page {0} / {1}   ({2} files)";
        private const float PageLabelWidth = 180f;
        private const int PageSize = 50;
        private const string PrefPrefix = "StaticResetChecker.";
        private const string PrevLabel = "Prev";
        private const string ResultsHeader = "Findings";
        private const float RowHeight = 18f;
        private const float ScanButtonHeight = 28f;
        private const string ScanLabel = "Scan";
        private const string WindowTitle = "Static Reset Checker";

        private readonly Dictionary<string, bool> _foldouts = new();
        private readonly EditorWindowStyles _styles = new();

        private string _rootFolder = "Assets";
        private string _ignoreMarker = "reset-ignore";
        private string _resetAttributes = "InitializeOnEnterPlayMode,RuntimeInitializeOnLoadMethod";

        private bool _includeEvents = true;
        private bool _includeAutoProperties = true;
        private bool _skipEditorFolders = true;
        private bool _expandHelpers = true;
        private bool _ignoreReadonly = true;
        private bool _logToConsole;

        private int _filesScanned;
        private bool _hasScanned;
        private string _status = string.Empty;
        private Vector2 _scroll;
        private int _page;
        private List<Finding> _findings = new();
        private List<IGrouping<string, Finding>> _groups = new();

#region Unity Callbacks
        private void OnEnable()
        {
            _rootFolder = EditorPrefs.GetString(PrefPrefix + "root", _rootFolder);
            _resetAttributes = EditorPrefs.GetString(PrefPrefix + "attrs", _resetAttributes);
            _ignoreMarker = EditorPrefs.GetString(PrefPrefix + "ignore", _ignoreMarker);
            _includeEvents = EditorPrefs.GetBool(PrefPrefix + "events", _includeEvents);
            _includeAutoProperties = EditorPrefs.GetBool(PrefPrefix + "props", _includeAutoProperties);
            _skipEditorFolders = EditorPrefs.GetBool(PrefPrefix + "skipEditor", _skipEditorFolders);
            _expandHelpers = EditorPrefs.GetBool(PrefPrefix + "helpers", _expandHelpers);
            _ignoreReadonly = EditorPrefs.GetBool(PrefPrefix + "ignoreReadonly", _ignoreReadonly);
            _logToConsole = EditorPrefs.GetBool(PrefPrefix + "log", _logToConsole);
        }

        private void OnGUI()
        {
            _styles.EnsureBuilt();

            EditorWindowChrome.DrawHeader(_styles, WindowTitle, Description);

            EditorWindowChrome.DrawSectionHeader(_styles, OptionsHeader);
            EditorWindowChrome.BeginCard(_styles);

            {
                _rootFolder = EditorGUILayout.TextField("Scan folder", _rootFolder);
                _resetAttributes = EditorGUILayout.TextField(new GUIContent("Reset attributes",
                        "Comma separated. A method with any of these attributes counts as a reset method."),
                    _resetAttributes);

                _ignoreMarker = EditorGUILayout.TextField(new GUIContent("Ignore marker",
                    "Add this as a comment on a field line to skip it."), _ignoreMarker);

                _includeEvents = EditorGUILayout.Toggle("Include static events", _includeEvents);
                _includeAutoProperties = EditorGUILayout.Toggle("Include static auto-properties",
                    _includeAutoProperties);

                _skipEditorFolders = EditorGUILayout.Toggle(new GUIContent("Skip /Editor/ folders",
                    "Editor-only statics usually don't need play-mode resets."), _skipEditorFolders);

                _expandHelpers = EditorGUILayout.Toggle(new GUIContent("Follow static helper calls",
                    "Also look inside static methods called from a reset method."), _expandHelpers);

                _ignoreReadonly = EditorGUILayout.Toggle(new GUIContent("Ignore readonly statics",
                    "Readonly static fields keep their value and don't need a play-mode reset."), _ignoreReadonly);

                _logToConsole = EditorGUILayout.Toggle("Also log to Console", _logToConsole);
            }

            EditorWindowChrome.EndCard();

            EditorGUILayout.Space(EditorMetrics.ItemGap);

            DrawActions();

            if (!_hasScanned)
            {
                EditorWindowChrome.DrawEmptyState(_styles, EditorIcons.Script, EmptyMessage, EmptyHint);
                return;
            }

            DrawResults();

            EditorWindowChrome.DrawFooter(_styles, _status);
        }

        private void OnDisable() => _styles.Dispose();
#endregion

        [DynamicMenuItem(MenuPath)]
        private static void Open()
        {
            StaticResetCheckerWindow window = GetWindow<StaticResetCheckerWindow>(WindowTitle);

            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
            window.Show();
        }

        private static void OpenAt(Finding finding)
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(finding.AssetPath);

            if (asset != null)
            {
                AssetDatabase.OpenAsset(asset, finding.Line);
                return;
            }

            if (!string.IsNullOrEmpty(finding.AbsolutePath) && File.Exists(finding.AbsolutePath))
            {
                InternalEditorUtility.OpenFileAtLineExternal(finding.AbsolutePath, finding.Line, 0);
                return;
            }

            CustomLogger.LogWarning($"Could not open {finding.AssetPath}", null);
        }

        private void SavePrefs()
        {
            EditorPrefs.SetString(PrefPrefix + "root", _rootFolder);
            EditorPrefs.SetString(PrefPrefix + "attrs", _resetAttributes);
            EditorPrefs.SetString(PrefPrefix + "ignore", _ignoreMarker);
            EditorPrefs.SetBool(PrefPrefix + "events", _includeEvents);
            EditorPrefs.SetBool(PrefPrefix + "props", _includeAutoProperties);
            EditorPrefs.SetBool(PrefPrefix + "skipEditor", _skipEditorFolders);
            EditorPrefs.SetBool(PrefPrefix + "helpers", _expandHelpers);
            EditorPrefs.SetBool(PrefPrefix + "ignoreReadonly", _ignoreReadonly);
            EditorPrefs.SetBool(PrefPrefix + "log", _logToConsole);
        }

        private void DrawActions()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (EditorWindowChrome.PrimaryButton(_styles, ScanLabel,
                        GUILayout.Height(ScanButtonHeight)))
                {
                    SavePrefs();
                    RunScan();
                }

                GUILayout.Space(EditorMetrics.TightGap);

                using (new EditorGUI.DisabledScope(!_hasScanned || _findings.Count == 0))
                {
                    if (EditorWindowChrome.SecondaryButton(_styles, CopyLabel,
                            GUILayout.Height(ScanButtonHeight), GUILayout.Width(CopyButtonWidth)))
                        EditorGUIUtility.systemCopyBuffer = BuildReport();
                }
            }

            EditorGUILayout.Space(EditorMetrics.ItemGap);
        }

        private void DrawResults()
        {
            if (_groups.Count == 0)
            {
                EditorWindowChrome.DrawEmptyState(_styles, EditorIcons.Success, EmptyOkMessage, _status);
                return;
            }

            EditorWindowChrome.DrawSectionHeader(_styles, ResultsHeader);

            int totalPages = Mathf.Max(1, Mathf.CeilToInt(_groups.Count / (float)PageSize));
            _page = Mathf.Clamp(_page, 0, totalPages - 1);

            if (totalPages > 1)
                DrawPager(totalPages);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorWindowChrome.BeginCard(_styles);

            int start = _page * PageSize;
            int end = Mathf.Min(start + PageSize, _groups.Count);
            int rowIndex = 0;

            for (int groupIndex = start; groupIndex < end; groupIndex++)
                DrawGroup(_groups[groupIndex], ref rowIndex);

            EditorWindowChrome.EndCard();

            EditorGUILayout.EndScrollView();
        }

        private void DrawPager(int totalPages)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(_page <= 0))
                {
                    if (EditorWindowChrome.SecondaryButton(_styles, PrevLabel,
                            GUILayout.Width(PageButtonWidth)))
                        _page--;
                }

                GUILayout.FlexibleSpace();

                GUILayout.Label(string.Format(PageFormat, _page + 1, totalPages, _groups.Count),
                    _styles.Footer, GUILayout.Width(PageLabelWidth));

                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledScope(_page >= totalPages - 1))
                {
                    if (EditorWindowChrome.SecondaryButton(_styles, NextLabel,
                            GUILayout.Width(PageButtonWidth)))
                        _page++;
                }
            }

            EditorGUILayout.Space(EditorMetrics.TightGap);
        }

        // The row index runs across groups rather than restarting in each, so the striping stays
        // continuous down the list instead of resetting at every file header.
        private void DrawGroup(IGrouping<string, Finding> group, ref int rowIndex)
        {
            string file = group.Key;
            bool isOpen = _foldouts.GetValueOrDefault(file, true);

            string title = $"{Path.GetFileName(file)}  ({group.Count()})";

            isOpen = EditorGUILayout.Foldout(isOpen, title, true);
            _foldouts[file] = isOpen;

            if (!isOpen)
                return;

            foreach (Finding finding in group.OrderBy(entry => entry.Line))
            {
                DrawFinding(finding, rowIndex);
                rowIndex++;
            }

            EditorGUILayout.Space(EditorMetrics.TightGap);
        }

        private void DrawFinding(Finding finding, int rowIndex)
        {
            Rect row = EditorGUILayout.GetControlRect(GUILayout.Height(RowHeight));

            EditorRows.DrawRowBackground(row, rowIndex);

            Rect cell = new(row.x + EditorMetrics.Indent, row.y,
                Mathf.Max(0f, row.width - EditorMetrics.Indent), row.height);

            GUIContent content = new($"L{finding.Line}   {finding.Name}   ({finding.Kind})", finding.Snippet);

            if (GUI.Button(cell, content, EditorStyles.linkLabel))
                OpenAt(finding);
        }

        private void RunScan()
        {
            try
            {
                ScanOptions options = new()
                {
                    RootFolder = string.IsNullOrWhiteSpace(_rootFolder)
                        ? "Assets"
                        : _rootFolder.Trim(),
                    ResetAttributes = _resetAttributes.Split(',')
                        .Select(attribute => attribute.Trim())
                        .Where(attribute => attribute.Length > 0)
                        .ToArray(),
                    IgnoreMarker = _ignoreMarker,
                    IncludeEvents = _includeEvents,
                    IncludeAutoProperties = _includeAutoProperties,
                    SkipEditorFolders = _skipEditorFolders,
                    ExpandHelpers = _expandHelpers,
                    IgnoreReadonly = _ignoreReadonly
                };

                _findings = StaticResetScanner.Scan(options, out _filesScanned);
                _groups = _findings.GroupBy(finding => finding.AssetPath).OrderBy(group => group.Key).ToList();
                _page = 0;
                _hasScanned = true;

                _status = _findings.Count == 0
                    ? $"No unreset static members found. Scanned {_filesScanned} file(s)."
                    : $"Found {_findings.Count} possibly-unreset static member(s) in "
                    + $"{_groups.Count} file(s)."
                    + $" Scanned {_filesScanned} file(s).";

                if (_logToConsole)
                    CustomLogger.Log(_status
                        + (_findings.Count > 0
                            ? "\n" + BuildReport()
                            : string.Empty), null);
            }
            catch (Exception exception)
            {
                // Deliberately broad: the scan reads arbitrary files off disk, and every way that can
                // fail has the same answer, which is to show the message and leave the list empty.
                _hasScanned = true;
                _findings = new List<Finding>();
                _groups = new List<IGrouping<string, Finding>>();
                _page = 0;
                _status = "Scan failed: " + exception.Message;
                CustomLogger.LogError($"Scan failed: {exception}", null);
            }
        }

        private string BuildReport()
        {
            StringBuilder builder = new();
            foreach (IGrouping<string, Finding> group in _groups)
            {
                builder.AppendLine(group.Key);
                foreach (Finding finding in group.OrderBy(entry => entry.Line))
                    builder.AppendLine($"  L{finding.Line}  {finding.Name}  ({finding.Kind})");
            }

            return builder.ToString();
        }
    }
}