#if UNITY_EDITOR
using Base.UtilityPackage.Generated;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.OrderManagement
{
    /// <summary>Editor window to manage constants and regenerate the generated file.</summary>
    public sealed class OrderManagerWindow : EditorWindow
    {
        private const float NumberWidth = 80f;
        private const float RemoveWidth = 24f;
        private const string WindowTitle = "Order Manager";

        private OrderRegistry registry;
        private SerializedObject serialized;
        private Vector2 scroll;

#region Unity Callbacks
        private void OnEnable()
        {
            registry = OrderRegistry.instance;
            serialized = new SerializedObject(registry);
        }

        private void OnGUI()
        {
            if (serialized == null)
                return;

            serialized.Update();
            scroll = EditorGUILayout.BeginScrollView(scroll);

            DrawSettings();
            EditorGUILayout.Space();
            DrawConstants();
            EditorGUILayout.Space();
            DrawGenerateButton();

            EditorGUILayout.EndScrollView();

            if (serialized.ApplyModifiedProperties())
                registry.Persist();
        }
#endregion

        [MenuItem("Tools/Base Packages/Code Generation/Order Manager", priority = MenuOrders.UnityEditor)]
        private static void Open()
        {
            OrderManagerWindow window = GetWindow<OrderManagerWindow>();
            window.titleContent = new GUIContent(WindowTitle);
            window.minSize = new Vector2(420f, 320f);
            window.Show();
        }

        private static void DrawConstant(SerializedProperty constants, int index)
        {
            SerializedProperty element = constants.GetArrayElementAtIndex(index);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(element.FindPropertyRelative("name"), GUIContent.none);
            EditorGUILayout.PropertyField(element.FindPropertyRelative("value"), GUIContent.none,
                GUILayout.Width(NumberWidth));

            if (GUILayout.Button("X", GUILayout.Width(RemoveWidth)))
            {
                constants.DeleteArrayElementAtIndex(index);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(element.FindPropertyRelative("comment"));
            EditorGUILayout.EndVertical();
        }

        private void DrawSettings()
        {
            EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serialized.FindProperty("outputDirectory"));
            EditorGUILayout.PropertyField(serialized.FindProperty("generatedNamespace"));
            EditorGUILayout.PropertyField(serialized.FindProperty("rootClassName"));
        }

        private void DrawConstants()
        {
            SerializedProperty constants = serialized.FindProperty("constants");
            EditorGUILayout.LabelField("Constants", EditorStyles.boldLabel);

            for (int i = 0; i < constants.arraySize; i++)
            {
                DrawConstant(constants, i);
                EditorGUILayout.Space();
            }

            if (GUILayout.Button("Add Constant"))
                constants.arraySize++;
        }

        private void DrawGenerateButton()
        {
            if (!GUILayout.Button("Generate", GUILayout.Height(32f)))
                return;

            serialized.ApplyModifiedProperties();
            registry.Persist();
            OrderCodeGenerator.Generate(registry);
        }
    }
}
#endif