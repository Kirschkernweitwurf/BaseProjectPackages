using Base.EditorUiPackage;
using Base.UtilityPackage.Menus;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.GetComponentAssigner
{
    /// <summary>
    /// One-click tool that assigns every empty <see cref="GetComponentAttribute"/> and
    /// <see cref="GetComponentInParentAttribute"/> field on prefab assets and the open scenes, so
    /// references resolve without opening each inspector once.
    /// </summary>
    internal sealed class GetComponentAssignerWindow : EditorWindow
    {
        private const string AssignLabel = "Assign References";
        private const float AssignButtonHeight = 28f;
        private const string Description = "Fills in every empty [GetComponent] and "
            + "[GetComponentInParent] field across the project, so references resolve without "
            + "opening each inspector once.";
        private const string NothingFoundResult = "No empty references found. Everything is already "
            + "assigned.";
        private const string PrefabsLabel = "Include prefab assets";
        private const string ScenesLabel = "Include open scenes";
        private const string ScopeHeader = "Scope";
        private const string MenuPath = "Tools/Base Packages/Unity Editor/References/Assign GetComponents";
        private const float MinimumHeight = 140f;
        private const float MinimumWidth = 300f;
        private const string WindowTitle = "Assign GetComponents";

        [SerializeField] private bool includePrefabs = true;
        [SerializeField] private bool includeScenes = true;

        private readonly EditorWindowStyles _styles = new();

        private string _result = string.Empty;

#region Unity Callbacks
        private void OnEnable() => titleContent = new GUIContent(WindowTitle);

        private void OnGUI()
        {
            _styles.EnsureBuilt();

            EditorWindowChrome.DrawHeader(_styles, WindowTitle, Description);

            EditorWindowChrome.DrawSectionHeader(_styles, ScopeHeader);
            EditorWindowChrome.BeginCard(_styles);

            includePrefabs = EditorGUILayout.ToggleLeft(PrefabsLabel, includePrefabs);
            includeScenes = EditorGUILayout.ToggleLeft(ScenesLabel, includeScenes);

            EditorWindowChrome.EndCard();

            EditorGUILayout.Space(EditorMetrics.ItemGap);

            using (new EditorGUI.DisabledScope(!includePrefabs && !includeScenes))
            {
                if (EditorWindowChrome.PrimaryButton(_styles, AssignLabel,
                        GUILayout.Height(AssignButtonHeight)))
                    Assign();
            }

            EditorWindowChrome.DrawFooter(_styles, _result);
        }

        private void OnDisable() => _styles.Dispose();
#endregion

        [DynamicMenuItem(MenuPath)]
        private static void Open()
        {
            GetComponentAssignerWindow window = GetWindow<GetComponentAssignerWindow>();

            window.minSize = new Vector2(MinimumWidth, MinimumHeight);
            window.Show();
        }

        private void Assign()
        {
            int assigned = GetComponentBatchAssigner.Run(includePrefabs, includeScenes);

            _result = assigned == 0
                ? NothingFoundResult
                : $"Assigned {assigned} reference(s).";
        }
    }
}