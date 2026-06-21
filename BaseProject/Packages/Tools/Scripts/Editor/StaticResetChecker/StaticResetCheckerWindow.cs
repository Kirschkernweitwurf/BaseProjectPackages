#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Base.ToolPackage.Editor.Generated;
using Base.UtilityPackage.Logging;
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
    public class StaticResetCheckerWindow : EditorWindow
    {
        private const int PageSize = 50;
        private const string PrefPrefix = "StaticResetChecker.";

        private readonly Dictionary<string, bool> _foldouts = new();

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
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Finds static fields that are not reset on Enter Play Mode.",
                EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.Space(4);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
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

            EditorGUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Scan", GUILayout.Height(28)))
                {
                    SavePrefs();
                    RunScan();
                }

                using (new EditorGUI.DisabledScope(!_hasScanned || _findings.Count == 0))
                {
                    if (GUILayout.Button("Copy report", GUILayout.Height(28), GUILayout.Width(110)))
                        EditorGUIUtility.systemCopyBuffer = BuildReport();
                }
            }

            if (_hasScanned)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.HelpBox(_status, _findings.Count == 0
                    ? MessageType.Info
                    : MessageType.Warning);
            }

            DrawResults();
        }
#endregion

        [MenuItem("Tools/Base Packages/Code Health/Static Reset Checker", priority = MenuOrders.Code)]
        private static void Open()
        {
            StaticResetCheckerWindow w = GetWindow<StaticResetCheckerWindow>("Static Reset");
            w.minSize = new Vector2(420, 360);
            w.Show();
        }

        private static void OpenAt(Finding f)
        {
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(f.AssetPath);
            if (obj != null)
            {
                AssetDatabase.OpenAsset(obj, f.Line);
                return;
            }

            if (!string.IsNullOrEmpty(f.AbsolutePath) && File.Exists(f.AbsolutePath))
            {
                InternalEditorUtility.OpenFileAtLineExternal(f.AbsolutePath, f.Line, 0);
                return;
            }

            CustomLogger.LogWarning("Could not open " + f.AssetPath, null);
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

        private void DrawResults()
        {
            if (_groups.Count == 0)
                return;

            int totalPages = Mathf.Max(1, Mathf.CeilToInt(_groups.Count / (float)PageSize));
            _page = Mathf.Clamp(_page, 0, totalPages - 1);

            if (totalPages > 1)
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(_page <= 0))
                    {
                        if (GUILayout.Button("Prev", GUILayout.Width(70)))
                            _page--;
                    }

                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField($"Page {_page + 1} / {totalPages}   ({_groups.Count} files)",
                        EditorStyles.miniLabel, GUILayout.Width(180));

                    GUILayout.FlexibleSpace();

                    using (new EditorGUI.DisabledScope(_page >= totalPages - 1))
                    {
                        if (GUILayout.Button("Next", GUILayout.Width(70)))
                            _page++;
                    }
                }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            int start = _page * PageSize;
            int end = Mathf.Min(start + PageSize, _groups.Count);
            for (int gi = start; gi < end; gi++)
            {
                IGrouping<string, Finding> group = _groups[gi];
                string file = group.Key;
                bool open = _foldouts.GetValueOrDefault(file, true);

                string fileTitle = $"{Path.GetFileName(file)}  ({group.Count()})";
                open = EditorGUILayout.Foldout(open, fileTitle, true);
                _foldouts[file] = open;
                if (!open)
                    continue;

                EditorGUI.indentLevel++;
                foreach (Finding f in group.OrderBy(x => x.Line))
                {
                    GUIContent content = new($"L{f.Line}   {f.Name}   ({f.Kind})", f.Snippet);
                    Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(18));
                    if (GUI.Button(rect, content, EditorStyles.linkLabel))
                        OpenAt(f);
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();
        }

        private void RunScan()
        {
            try
            {
                ScanOptions opt = new()
                {
                    RootFolder = string.IsNullOrWhiteSpace(_rootFolder)
                        ? "Assets"
                        : _rootFolder.Trim(),
                    ResetAttributes = _resetAttributes.Split(',')
                        .Select(s => s.Trim())
                        .Where(s => s.Length > 0)
                        .ToArray(),
                    IgnoreMarker = _ignoreMarker,
                    IncludeEvents = _includeEvents,
                    IncludeAutoProperties = _includeAutoProperties,
                    SkipEditorFolders = _skipEditorFolders,
                    ExpandHelpers = _expandHelpers,
                    IgnoreReadonly = _ignoreReadonly
                };

                _findings = StaticResetScanner.Scan(opt, out _filesScanned);
                _groups = _findings.GroupBy(f => f.AssetPath).OrderBy(g => g.Key).ToList();
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
            catch (Exception e)
            {
                _hasScanned = true;
                _findings = new List<Finding>();
                _groups = new List<IGrouping<string, Finding>>();
                _page = 0;
                _status = "Scan failed: " + e.Message;
                CustomLogger.LogError($"Scan failed: {e}", null);
            }
        }

        private string BuildReport()
        {
            StringBuilder sb = new();
            foreach (IGrouping<string, Finding> group in _groups)
            {
                sb.AppendLine(group.Key);
                foreach (Finding f in group.OrderBy(x => x.Line))
                    sb.AppendLine($"  L{f.Line}  {f.Name}  ({f.Kind})");
            }

            return sb.ToString();
        }
    }
}
#endif