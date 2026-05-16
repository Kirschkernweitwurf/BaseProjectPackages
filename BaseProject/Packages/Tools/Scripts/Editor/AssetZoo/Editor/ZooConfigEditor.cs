#if UNITY_EDITOR
using Base.ToolPackage.Editor.AssetZoo.Runtime.Builder;
using Base.ToolPackage.Editor.AssetZoo.Runtime.Config;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Editor
{
    /// <summary>
    /// Custom inspector for <see cref="ZooConfig"/> that adds buttons for building, clearing,
    /// and selecting the zoo root, as well as opening the zoo editor window.
    /// </summary>
    [CustomEditor(typeof(ZooConfig))]
    public class ZooConfigEditor : UnityEditor.Editor
    {
        private const float MainButtonHeight = 28f;
        private const float AuxButtonHeight = 22f;

        private readonly ZooBuilder _builder = new();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            bool hasZoo = _builder.HasZoo;

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Build Zoo", GUILayout.Height(MainButtonHeight)))
                    _builder.Build((ZooConfig)target);

                using (new EditorGUI.DisabledScope(!hasZoo))
                {
                    if (GUILayout.Button("Clear Zoo", GUILayout.Height(MainButtonHeight)))
                        _builder.Clear();
                }
            }

            using (new EditorGUI.DisabledScope(!hasZoo))
            {
                if (GUILayout.Button("Select Zoo Root", GUILayout.Height(AuxButtonHeight)))
                    SelectZooRoot();
            }

            if (GUILayout.Button("Open Zoo Window", GUILayout.Height(AuxButtonHeight)))
                ZooEditorWindow.Open((ZooConfig)target);
        }

        private void SelectZooRoot()
        {
            GameObject root = _builder.GetZooRoot();
            if (root == null)
                return;

            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);
        }
    }
}
#endif