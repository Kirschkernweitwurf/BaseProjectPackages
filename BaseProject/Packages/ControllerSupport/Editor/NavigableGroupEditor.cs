using Base.AttributesPackage.Editor.Inspectors;
using Base.ControllerSupportPackage.Controller.Navigation;
using UnityEditor;
using UnityEngine;

namespace Base.ControllerSupportPackage.Editor
{
    /// <summary>
    /// Adds "Rebuild" and "Rebuild Scene" buttons to the <see cref="NavigableGroup"/> inspector so
    /// designers can rewire navigation without hunting through the context menu. Derives from
    /// <see cref="AttributesPackageEditor"/> so the Attributes package renders the group's fields. The
    /// full overview lives in the <see cref="NavigationGroupsWindow"/>.
    /// </summary>
    [CustomEditor(typeof(NavigableGroup))]
    internal sealed class NavigableGroupEditor : AttributesPackageEditor
    {
        private const string RebuildLabel = "Rebuild";
        private const string RebuildSceneLabel = "Rebuild Scene";

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(RebuildLabel))
                    NavigationRebuildService.RebuildGroup((NavigableGroup)target);

                if (GUILayout.Button(RebuildSceneLabel))
                    NavigationRebuildService.RebuildLoadedScenes();
            }
        }
    }
}