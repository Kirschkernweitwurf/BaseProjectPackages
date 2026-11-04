using Base.EditorUIPackage.Editor;
using Base.ToolsPackage.Editor.AssetZoo.Config;
using UnityEditor;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.UI
{
    /// <summary>
    /// A saved zoo is edited in the builder window, where its settings update the zoo as they change, so the
    /// Inspector points there instead of offering a second, slower way to do the same.
    /// </summary>
    [CustomEditor(typeof(ZooConfig))]
    internal sealed class ZooConfigEditor : UnityEditor.Editor
    {
        private const float ButtonHeight = 30f;
        private const string Hint = "This saved zoo is edited in the Asset Zoo Builder. Double-clicking it opens "
            + "it there too.";
        private const string OpenWindowLabel = "Open In Builder";

        private readonly EditorWindowStyles _styles = new();

#region Unity Callbacks
        private void OnDisable() => _styles.Dispose();
#endregion

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            _styles.EnsureBuilt();

            GUILayout.Label(Hint, _styles.Description);

            EditorGUILayout.Space(EditorMetrics.ItemGap);

            if (EditorWindowChrome.PrimaryButton(_styles, OpenWindowLabel, GUILayout.Height(ButtonHeight)))
                ZooEditorWindow.Open((ZooConfig)target);
        }
    }
}