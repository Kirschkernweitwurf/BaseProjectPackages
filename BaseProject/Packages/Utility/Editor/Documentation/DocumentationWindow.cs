using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Base.UtilityPackage.Editor.Documentation
{
    /// <summary>
    /// Lists every installed package that ships a <c>Documentation~/index.md</c>, plus the project's
    /// own <c>Docs/index.md</c>, and opens them in whatever Markdown app the machine uses.
    /// </summary>
    /// <remarks>
    /// A plain <see cref="MenuItem"/> rather than a dynamic one on purpose: the Menu Manager that
    /// resolves dynamic entries lives in the Tools package, and the docs have to be reachable in a
    /// project that only installed a single package.
    /// </remarks>
    internal sealed class DocumentationWindow : EditorWindow
    {
        private const float ButtonWidth = 64f;
        private const string EmptyMessage =
            "No documentation found. Packages ship it in a Documentation~ folder, the project in a Docs "
            + "folder next to Assets. Each needs an index.md.";
        private const string FolderLabel = "Folder";
        private const string Hint =
            "Opens in your default Markdown app. If nothing happens, set one for .md files in your "
            + "operating system, for example Obsidian or VS Code.";
        private const string MenuPath = "Help/Base Packages Documentation";
        private const string OnlineLabel = "Online";
        private const string OpenLabel = "Open";
        private const string ProjectName = "This project";
        private const string RefreshLabel = "Refresh";
        private const string WindowTitle = "Documentation";

        private static readonly Vector2 MinWindowSize = new(420f, 240f);

        private readonly List<DocumentationSource> _sources = new();

        private Vector2 _scroll;

#region Unity Callbacks
        private void OnEnable() => Refresh();

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(RefreshLabel, EditorStyles.toolbarButton))
                    Refresh();
            }

            EditorGUILayout.HelpBox(_sources.Count == 0 ? EmptyMessage : Hint, MessageType.None);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            foreach (DocumentationSource source in _sources)
                DrawRow(source);

            EditorGUILayout.EndScrollView();
        }
#endregion

        [MenuItem(MenuPath)]
        private static void Open()
        {
            DocumentationWindow window = GetWindow<DocumentationWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = MinWindowSize;
            window.Show();
        }

        private static void DrawRow(DocumentationSource source)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                string label = source.Version.Length == 0
                    ? source.DisplayName
                    : $"{source.DisplayName}  {source.Version}";

                GUILayout.Label(label);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(OpenLabel, GUILayout.Width(ButtonWidth)))
                    EditorUtility.OpenWithDefaultApp(source.StartPagePath);

                if (GUILayout.Button(FolderLabel, GUILayout.Width(ButtonWidth)))
                    EditorUtility.RevealInFinder(source.StartPagePath);

                using (new EditorGUI.DisabledScope(source.OnlineUrl.Length == 0))
                {
                    if (GUILayout.Button(OnlineLabel, GUILayout.Width(ButtonWidth)))
                        Application.OpenURL(source.OnlineUrl);
                }
            }
        }

        private void Refresh()
        {
            _sources.Clear();

            string projectRoot = Path.GetDirectoryName(Application.dataPath);

            if (DocumentationSource.TryFindFolder(projectRoot, DocumentationSource.ProjectDocsFolder,
                    out string projectDocs))
                _sources.Add(new DocumentationSource(ProjectName, string.Empty, projectDocs, string.Empty));

            foreach (PackageInfo package in PackageInfo.GetAllRegisteredPackages())
            {
                if (!DocumentationSource.TryFindFolder(package.resolvedPath,
                        DocumentationSource.PackageDocsFolder, out string folder))
                    continue;

                _sources.Add(new DocumentationSource(package.displayName, package.version, folder,
                    package.documentationUrl));
            }

            DocumentationSource.Sort(_sources);
        }
    }
}