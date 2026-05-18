#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Base.AttributePackage.Editor
{
    [CustomPropertyDrawer(typeof(InputActionMapReference))]
    public class InputActionMapReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            SerializedProperty assetProp = property.FindPropertyRelative("asset");
            SerializedProperty mapIdProp = property.FindPropertyRelative("mapId");

            float half = position.width * 0.5f - 2f;
            Rect assetRect = new(position.x,           position.y, half, position.height);
            Rect mapRect   = new(position.x + half + 4, position.y, half, position.height);

            EditorGUI.PropertyField(assetRect, assetProp, GUIContent.none);

            InputActionAsset asset = assetProp.objectReferenceValue as InputActionAsset;
            if (asset == null)
            {
                using (new EditorGUI.DisabledScope(true))
                    EditorGUI.Popup(mapRect, 0, new[] { "<assign asset>" });
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
                if (ids[i] == mapIdProp.stringValue) currentIndex = i;
            }

            int newIndex = EditorGUI.Popup(mapRect, Mathf.Max(0, currentIndex), names);
            if (count > 0 && newIndex != currentIndex)
                mapIdProp.stringValue = ids[newIndex];

            EditorGUI.EndProperty();
        }
    }
}
#endif