using Base.EditorUIPackage.Editor;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.OrderManagement
{
    /// <summary>Editor window to manage constants and regenerate the generated file.</summary>
    internal sealed class OrderManagerWindow : EditorWindow
    {
        private const float AddButtonWidth = 110f;
        private const string AddLabel = "Add Constant";
        private const string CommentField = "comment";
        private const string ConstantsField = "constants";
        private const string ConstantsHeader = "Constants";
        private const string Description = "Names the execution order constants the generated file is built "
            + "from, then writes that file. Every constant becomes a named value your scripts can order "
            + "themselves by instead of spelling a number out.";
        private const string EmptyHint = "Add one and it appears in the generated file the next time you "
            + "press Generate.";
        private const string EmptyMessage = "No constants yet";
        private const float GenerateButtonHeight = 32f;
        private const string GenerateLabel = "Generate";
        private const string MenuPath = "Tools/Base Packages/Code/Generation/Order Manager";
        private const float MinWindowHeight = 320f;
        private const float MinWindowWidth = 420f;
        private const string NameField = "name";
        private const string NamespaceField = "generatedNamespace";
        private const int NoRemoval = -1;
        private const float NumberWidth = 80f;
        private const string OutputDirectoryField = "outputDirectory";
        private const string OutputHeader = "Output";
        private const string RemoveLabel = "X";
        private const float RemoveWidth = 24f;
        private const string RootClassField = "rootClassName";
        private const string ValueField = "value";
        private const string WindowTitle = "Order Manager";

        private readonly EditorWindowStyles _styles = new();

        private OrderRegistry _registry;
        private SerializedObject _serialized;
        private Vector2 _scroll;

#region Unity Callbacks
        private void OnEnable()
        {
            _registry = OrderRegistry.instance;
            _serialized = new SerializedObject(_registry);
        }

        private void OnGUI()
        {
            if (_serialized == null)
                return;

            _styles.EnsureBuilt();
            _serialized.Update();

            EditorWindowChrome.DrawHeader(_styles, WindowTitle, Description);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawSettings();
            EditorGUILayout.Space(EditorMetrics.SectionGap);
            DrawConstants();

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(EditorMetrics.ItemGap);
            DrawGenerateButton();

            if (_serialized.ApplyModifiedProperties())
                _registry.Persist();
        }

        private void OnDisable() => _styles.Dispose();
#endregion

        [DynamicMenuItem(MenuPath)]
        private static void Open()
        {
            OrderManagerWindow window = GetWindow<OrderManagerWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
            window.Show();
        }

        private void DrawSettings()
        {
            EditorWindowChrome.DrawSectionHeader(_styles, OutputHeader);
            EditorWindowChrome.BeginCard(_styles);

            EditorGUILayout.PropertyField(_serialized.FindProperty(OutputDirectoryField));
            EditorGUILayout.PropertyField(_serialized.FindProperty(NamespaceField));
            EditorGUILayout.PropertyField(_serialized.FindProperty(RootClassField));

            EditorWindowChrome.EndCard();
        }

        private void DrawConstants()
        {
            SerializedProperty constants = _serialized.FindProperty(ConstantsField);

            DrawConstantsHeader(constants);

            if (constants.arraySize == 0)
            {
                EditorWindowChrome.DrawEmptyState(_styles, EditorIcons.Script, EmptyMessage, EmptyHint);
                return;
            }

            // The index is collected and applied after the loop. Deleting an element while the rows are
            // being drawn changes how many controls the pass emits, which is what IMGUI reports as a
            // control count mismatch on the next repaint.
            int removalIndex = NoRemoval;

            for (int i = 0; i < constants.arraySize; i++)
            {
                if (DrawConstant(constants, i))
                    removalIndex = i;
            }

            if (removalIndex != NoRemoval)
                constants.DeleteArrayElementAtIndex(removalIndex);
        }

        private void DrawConstantsHeader(SerializedProperty constants)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(ConstantsHeader, _styles.SectionHeader);
            GUILayout.FlexibleSpace();

            if (EditorWindowChrome.SecondaryButton(_styles, AddLabel, GUILayout.Width(AddButtonWidth)))
                constants.arraySize++;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(EditorMetrics.TightGap);
        }

        // Reports whether the row asked to be removed, so the caller can do it once the pass is over.
        private bool DrawConstant(SerializedProperty constants, int index)
        {
            SerializedProperty element = constants.GetArrayElementAtIndex(index);

            EditorWindowChrome.BeginCard(_styles);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField(element.FindPropertyRelative(NameField), GUIContent.none);
            EditorGUILayout.PropertyField(element.FindPropertyRelative(ValueField), GUIContent.none,
                GUILayout.Width(NumberWidth));

            bool isRemoved = EditorWindowChrome.SecondaryButton(_styles, RemoveLabel,
                GUILayout.Width(RemoveWidth));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(element.FindPropertyRelative(CommentField));

            EditorWindowChrome.EndCard();

            return isRemoved;
        }

        private void DrawGenerateButton()
        {
            if (!EditorWindowChrome.PrimaryButton(_styles, GenerateLabel,
                    GUILayout.Height(GenerateButtonHeight)))
                return;

            _serialized.ApplyModifiedProperties();
            _registry.Persist();
            OrderCodeGenerator.Generate(_registry);
        }
    }
}