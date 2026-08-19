using System.IO;
using Base.EditorUiPackage;
using Base.UtilityPackage.Logging;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.PackageDependencies
{
    /// <summary>
    /// Generates the installer's package list from the assembly definitions in this repository.
    /// <para>
    /// The installer cannot work the graph out for itself: it has to know what a package needs
    /// before that package is on disk. Generating the list here and shipping the result keeps the
    /// asmdefs as the single source of truth without the installer having to guess.
    /// </para>
    /// </summary>
    internal sealed class PackageDefaultsWindow : EditorWindow
    {
        private const string CopyLabel = "Copy to Clipboard";
        private const string Description = "Reads every asmdef under the packages root, resolves the references "
            + "between packages and drops the edges another edge already implies. Optional assemblies behind a "
            + "define constraint and test assemblies are ignored, so they never become hard dependencies.";
        private const string GenerateLabel = "Scan";
        private const string GraphHeader = "Dependency Graph";
        private const string NoDependencies = "none";
        private const string OutputFileName = "BasePackageDefaults.cs";
        private const string PathPrefsKey = "Base.ToolPackage.PackageDefaults.OutputPath";
        private const string PreviewHeader = "Generated File";
        private const string RootLabel = "Packages Root";
        private const string SaveDialogTitle = "Save generated defaults";
        private const string ScriptFilter = "cs";
        private const string WindowTitle = "Package Defaults";
        private const string WriteLabel = "Write File";

        private const float ButtonHeight = 22f;
        private const float MinimumHeight = 420f;
        private const float MinimumWidth = 560f;
        private const float PreviewMinHeight = 180f;

        private PackageDependencyInfo[] _packages;
        private string _preview;
        private string _root;
        private Vector2 _scroll;

#region Unity Callbacks
        private void OnEnable()
        {
            _root = DefaultRoot();
            Scan();
        }

        private void OnGUI()
        {
            DrawHeader();

            EditorGUILayout.Space(EditorMetrics.SectionGap);

            using (EditorGUILayout.ScrollViewScope scope = new(_scroll))
            {
                _scroll = scope.scrollPosition;

                DrawGraph();
                DrawPreview();
            }
        }
#endregion

        /// <summary>Opens or focuses the window.</summary>
        [DynamicMenuItem("Tools/Base Packages/Assets/Package Defaults")]
        private static void Open()
        {
            PackageDefaultsWindow window = GetWindow<PackageDefaultsWindow>(WindowTitle);

            window.minSize = new Vector2(MinimumWidth, MinimumHeight);
            window.Show();
        }

        // In this repository the packages root is the project's own package folder.
        private static string DefaultRoot()
            => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Packages"));

        private void DrawHeader()
        {
            GUILayout.Label(WindowTitle, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(Description, EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.Space(EditorMetrics.TightGap);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();

                _root = EditorGUILayout.TextField(RootLabel, _root);

                if (EditorGUI.EndChangeCheck())
                    _packages = null;

                if (GUILayout.Button(GenerateLabel, GUILayout.Height(ButtonHeight)))
                    Scan();
            }
        }

        private void DrawGraph()
        {
            if (_packages == null || _packages.Length == 0)
                return;

            GUILayout.Label(GraphHeader, EditorStyles.boldLabel);

            for (int i = 0; i < _packages.Length; i++)
            {
                PackageDependencyInfo package = _packages[i];
                Rect row = GUILayoutUtility.GetRect(0f, EditorMetrics.RowHeight, GUILayout.ExpandWidth(true));

                EditorRows.DrawRowBackground(row, i);

                Rect name = new(row.x + EditorMetrics.RowInset, row.y, row.width * 0.35f, row.height);
                Rect list = new(name.xMax, row.y, row.width - name.width - EditorMetrics.RowInset, row.height);

                GUI.Label(name, package.DisplayName);
                GUI.Label(list, Describe(package), EditorStyles.miniLabel);
            }

            EditorGUILayout.Space(EditorMetrics.SectionGap);
        }

        private void DrawPreview()
        {
            if (string.IsNullOrEmpty(_preview))
                return;

            GUILayout.Label(PreviewHeader, EditorStyles.boldLabel);

            EditorGUILayout.TextArea(_preview, GUILayout.MinHeight(PreviewMinHeight));

            EditorGUILayout.Space(EditorMetrics.TightGap);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(CopyLabel, GUILayout.Height(ButtonHeight)))
                    EditorGUIUtility.systemCopyBuffer = _preview;

                if (GUILayout.Button(WriteLabel, GUILayout.Height(ButtonHeight)))
                    Write();
            }
        }

        private static string Describe(PackageDependencyInfo package)
        {
            if (package.DirectDependencies.Count == 0)
                return NoDependencies;

            return string.Join(", ", package.DirectDependencies);
        }

        private void Scan()
        {
            _packages = PackageDependencyScanner.Scan(_root);

            _preview = _packages.Length == 0
                ? string.Empty
                : PackageDefaultsWriter.Render(_packages);

            Repaint();
        }

        private void Write()
        {
            string previous = EditorPrefs.GetString(PathPrefsKey, string.Empty);

            string directory = string.IsNullOrEmpty(previous)
                ? string.Empty
                : Path.GetDirectoryName(previous);

            string path = EditorUtility.SaveFilePanel(SaveDialogTitle, directory, OutputFileName, ScriptFilter);

            if (string.IsNullOrEmpty(path))
                return;

            File.WriteAllText(path, _preview);
            EditorPrefs.SetString(PathPrefsKey, path);

            CustomLogger.Log($"Wrote {Path.GetFileName(path)} with {_packages.Length} packages.", null);
        }
    }
}