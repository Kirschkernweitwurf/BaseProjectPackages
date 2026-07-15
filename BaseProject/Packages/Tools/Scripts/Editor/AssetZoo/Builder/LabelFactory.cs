using Base.UIPackage.Utility;
using UnityEngine;
using UnityEngine.Rendering;

namespace Base.ToolPackage.Editor.AssetZoo.Builder
{
    /// <summary>
    /// Creates 3D TextMesh labels that always face the camera.
    /// </summary>
    public static class LabelFactory
    {
        /// <summary>
        /// Creates a 3D TextMesh label with the specified properties and adds
        /// an <see cref="EditorBillboard"/> component to make it always face the camera.
        /// </summary>
        public static GameObject CreateLabel(string text, Transform parent, Vector3 localPosition, int fontSize,
            Color color, float worldScale)
        {
            GameObject go = new($"Label_{text}");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = Vector3.one * worldScale;

            TextMesh mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.fontSize = fontSize;
            mesh.color = color;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.characterSize = 1f;

            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            go.AddComponent<EditorBillboard>();

            return go;
        }
    }
}