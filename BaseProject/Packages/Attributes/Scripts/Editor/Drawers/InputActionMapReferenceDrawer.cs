using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Property drawer for <see cref="InputActionMapReference"/>.
    /// Displays a field for the asset and a dropdown of all action maps in that asset.
    /// Stores the map's GUID, so it survives renames.
    /// </summary>
    [CustomPropertyDrawer(typeof(InputActionMapReference))]
    public sealed class InputActionMapReferenceDrawer : PropertyDrawer
    {
        private const string MissingAssetLabel = "<assign asset>";
        private const float Spacing = 4f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            SerializedProperty assetProp = property.FindPropertyRelative(InputActionMapReference.AssetFieldName);
            SerializedProperty mapIdProp = property.FindPropertyRelative(InputActionMapReference.MapIdFieldName);

            float half = (position.width - Spacing) * 0.5f;
            Rect assetRect = new(position.x, position.y, half, position.height);
            Rect mapRect = new(position.x + half + Spacing, position.y, half, position.height);

            EditorGUI.PropertyField(assetRect, assetProp, GUIContent.none);

            InputActionAsset asset = assetProp.objectReferenceValue as InputActionAsset;
            if (asset == null)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUI.Popup(mapRect, 0, new[] { MissingAssetLabel });
                }

                EditorGUI.EndProperty();
                return;
            }

            int count = asset.actionMaps.Count;
            string[] names = new string[count];
            string[] ids = new string[count];
            int currentIndex = -1;

            for (int i = 0; i < count; i++)
            {
                names[i] = asset.actionMaps[i].name;
                ids[i] = asset.actionMaps[i].id.ToString();
                if (ids[i] == mapIdProp.stringValue)
                    currentIndex = i;
            }

            int newIndex = EditorGUI.Popup(mapRect, Mathf.Max(0, currentIndex), names);
            if (count > 0 && newIndex != currentIndex)
                mapIdProp.stringValue = ids[newIndex];

            EditorGUI.EndProperty();
        }
    }
}