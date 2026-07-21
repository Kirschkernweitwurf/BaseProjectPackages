#if UNITY_EDITOR
using Base.ControllerSupport.Controller.Navigation;
using UnityEditor;
using UnityEngine;

namespace Base.ControllerSupport.Editor
{
    /// <summary>
    /// Adds "Rebuild" and "Rebuild Scene" buttons to the <see cref="NavigableGroup"/> inspector so
    /// designers can rewire navigation without hunting through the context menu. The full overview
    /// lives in the <see cref="NavigationGroupsWindow"/>.
    /// </summary>
    [CustomEditor(typeof(NavigableGroup))]
    public sealed class NavigableGroupEditor : UnityEditor.Editor
    {
        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Rebuild"))
                    ((NavigableGroup)target).Rebuild();

                if (GUILayout.Button("Rebuild Scene"))
                    NavigationRebuildService.RebuildLoadedScenes();
            }
        }
    }
}
#endif