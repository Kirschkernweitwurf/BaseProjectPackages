using Base.ToolPackage.MenuManagerWindow;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.GetComponentAssigner
{
    /// <summary>
    /// One-click tool that assigns every empty <see cref="GetComponentAttribute"/> and
    /// <see cref="GetComponentInParentAttribute"/> field on prefab assets and the open scenes, so
    /// references resolve without opening each inspector once.
    /// </summary>
    public sealed class GetComponentAssignerWindow : EditorWindow
    {
        private const string MenuPath = "Tools/Base Packages/Unity Editor/References/Assign GetComponents";
        private const string WindowTitle = "Assign GetComponents";

        [SerializeField] private bool includePrefabs = true;
        [SerializeField] private bool includeScenes = true;

        private string _result = string.Empty;

#region Unity Callbacks
        private void OnEnable() => titleContent = new GUIContent(WindowTitle);

        private void OnGUI()
        {
            EditorGUILayout.Space();

            includePrefabs = EditorGUILayout.ToggleLeft("Include prefab assets", includePrefabs);
            includeScenes = EditorGUILayout.ToggleLeft("Include open scenes", includeScenes);

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(!includePrefabs && !includeScenes))
            {
                if (GUILayout.Button("Assign References", GUILayout.Height(28f)))
                    Assign();
            }

            if (!string.IsNullOrEmpty(_result))
                EditorGUILayout.HelpBox(_result, MessageType.Info);
        }
#endregion

        [DynamicMenuItem(MenuPath)]
        private static void Open()
        {
            GetComponentAssignerWindow window = GetWindow<GetComponentAssignerWindow>();

            window.minSize = new Vector2(300f, 140f);
            window.Show();
        }

        private void Assign()
        {
            int assigned = GetComponentBatchAssigner.Run(includePrefabs, includeScenes);

            _result = assigned == 0
                ? "No empty references found. Everything is already assigned."
                : $"Assigned {assigned} reference(s).";
        }
    }
}