#if UNITY_EDITOR
using Base.ToolPackage.Editor.AssetZoo.Config;
using Base.ToolPackage.Editor.AssetZoo.Generation;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.UI
{
    /// <summary>
    /// Custom inspector for <see cref="ZooConfig"/> that adds buttons for building, clearing,
    /// and selecting the zoo root, as well as opening the zoo editor window.
    /// </summary>
    [CustomEditor(typeof(ZooConfig))]
    public class ZooConfigEditor : UnityEditor.Editor
    {
        private const float AuxButtonHeight = 22f;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            if (GUILayout.Button("Auto Generate Categories", GUILayout.Height(AuxButtonHeight)))
                ZooAutoGenerator.Generate((ZooConfig)target);

            if (GUILayout.Button("Open Zoo Window", GUILayout.Height(AuxButtonHeight)))
                ZooEditorWindow.Open((ZooConfig)target);
        }
    }
}
#endif