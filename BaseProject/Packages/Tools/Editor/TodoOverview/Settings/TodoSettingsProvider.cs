using System.Collections.Generic;
using Base.EditorUiPackage;
using Base.ToolPackage.Editor.BaseToolsOverview;
using UnityEditor;

namespace Base.ToolPackage.Editor.TodoOverview.Settings
{
    /// <summary>
    /// Exposes <see cref="TodoSettings"/> in the project settings, so the keywords, the notation for
    /// owner and date, and the paths that are out of scope are declared once per project.
    /// </summary>
    internal static class TodoSettingsProvider
    {
        private const string DatesHelp = "Date formats are tried in this order. Two digit years resolve "
            + "into the current century.";

        private const string MetadataHelp = "Patterns that read the responsible person and the date out of "
            + "an item. Each one is a regular expression that may carry an owner group, a date group or "
            + "both, and whatever it matches is cut out of the message. Example for "
            + "\"TODO: text (Jonny, 20.08.26)\": \\((?<owner>[^,()]+),\\s*(?<date>[0-9.]+)\\)";

        private const string DatesHeader = "Dates";
        private const string KeywordsHeader = "Keywords";
        private const string MetadataHeader = "Owner And Date";
        private const string PageLabel = "Todo Overview";
        private const string ScopeHelp = "Files whose path contains one of these are never read.";
        private const string SettingsPath = "Project/Base Tools/Todo Overview";
        private const string ScopeHeader = "Scope";
        private const string Summary = "The keywords the scan looks for, how owner and date are read out of "
            + "an item, and the paths that are never read.";
        private const string TagsHelp = "The keywords the scan looks for, with the color each one is drawn in.";

        private static readonly EditorWindowStyles Styles = new();

        private static SerializedObject _serializedObject;

        /// <summary>The settings path used to open this page programmatically.</summary>
        internal static string Path => SettingsPath;

        [SettingsProvider]
        [BaseToolsPage(Summary)]
        private static SettingsProvider Create() => new(SettingsPath, SettingsScope.Project)
        {
            label = PageLabel,
            keywords = new HashSet<string>
            {
                "todo",
                "fixme",
                "bug",
                "hack",
                "comment",
                "task"
            },

            // Created lazily so the singleton is not loaded and seeded on every domain reload, only once
            // this page is actually opened.
            activateHandler = (_, _) => _serializedObject = new SerializedObject(TodoSettings.instance),
            deactivateHandler = () =>
            {
                _serializedObject?.Dispose();
                _serializedObject = null;

                Styles.Dispose();
            },
            guiHandler = _ => DrawGui()
        };

        private static void DrawGui()
        {
            if (_serializedObject == null)
                return;

            Styles.EnsureBuilt();

            _serializedObject.Update();

            EditorWindowChrome.DrawIntro(Styles, Summary);

            EditorGUI.BeginChangeCheck();

            DrawSection(KeywordsHeader, TagsHelp, TodoSettings.TagsPropertyName,
                TodoSettings.CaseSensitivePropertyName, TodoSettings.ContinuationPropertyName);

            DrawSection(MetadataHeader, MetadataHelp, TodoSettings.MetadataPropertyName);
            DrawSection(DatesHeader, DatesHelp, TodoSettings.DateFormatsPropertyName);

            DrawSection(ScopeHeader, ScopeHelp, TodoSettings.ExtensionsPropertyName,
                TodoSettings.IgnoredPropertyName);

            if (!EditorGUI.EndChangeCheck())
                return;

            _serializedObject.ApplyModifiedProperties();
            TodoSettings.instance.Persist();
        }

        // One block per thing being configured: a header, the sentence that explains it, then the
        // fields in a card. Before this the page was one column of help boxes and lists with nothing
        // marking where one setting ended and the next began.
        private static void DrawSection(string header, string help, params string[] propertyNames)
        {
            EditorWindowChrome.DrawSectionHeader(Styles, header);

            EditorGUILayout.HelpBox(help, MessageType.Info);
            EditorGUILayout.Space(EditorMetrics.TightGap);

            EditorWindowChrome.BeginCard(Styles);

            foreach (string propertyName in propertyNames)
                DrawProperty(propertyName);

            EditorWindowChrome.EndCard();

            EditorGUILayout.Space(EditorMetrics.ItemGap);
        }

        private static void DrawProperty(string propertyName)
        {
            SerializedProperty property = _serializedObject.FindProperty(propertyName);

            if (property == null)
                return;

            EditorGUILayout.PropertyField(property, true);
        }
    }
}