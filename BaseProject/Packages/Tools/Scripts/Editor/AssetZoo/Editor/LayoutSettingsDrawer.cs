#if UNITY_EDITOR
using System.Collections.Generic;
using Base.ToolPackage.Editor.AssetZoo.Runtime.Config;
using Base.UtilityPackage.Editor;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.AssetZoo.Editor
{
    /// <summary>
    /// Custom drawer for <see cref="LayoutSettings"/>. Keeps the foldout for
    /// collapsibility and hides settings that don't apply to the chosen layout
    /// (e.g. circleRadius is only visible when Circle is selected).
    /// </summary>
    [CustomPropertyDrawer(typeof(LayoutSettings))]
    public class LayoutSettingsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            Rect headerRect = new(position.x, position.y, position.width, lineHeight);
            property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                float y = position.y + lineHeight + spacing;

                foreach (string name in GetVisiblePropertyNames(property))
                {
                    SerializedProperty prop = CustomEditorUtility.FindProp(property, name);
                    float h = EditorGUI.GetPropertyHeight(prop, true);
                    Rect rect = new(position.x, y, position.width, h);
                    EditorGUI.PropertyField(rect, prop, true);
                    y += h + spacing;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            if (!property.isExpanded)
                return lineHeight;

            float height = lineHeight + spacing;
            foreach (string name in GetVisiblePropertyNames(property))
            {
                SerializedProperty prop = CustomEditorUtility.FindProp(property, name);
                height += EditorGUI.GetPropertyHeight(prop, true) + spacing;
            }

            return height;
        }

        private static IEnumerable<string> GetVisiblePropertyNames(SerializedProperty property)
        {
            yield return nameof(LayoutSettings.Type);
            yield return nameof(LayoutSettings.Alignment);
            yield return nameof(LayoutSettings.CategoryDirection);
            yield return nameof(LayoutSettings.Spacing);
            yield return nameof(LayoutSettings.CategorySpacing);

            SerializedProperty typeProp = CustomEditorUtility.FindProp(property, nameof(LayoutSettings.Type));
            ELayoutType layoutType = (ELayoutType)typeProp.enumValueIndex;

            switch (layoutType)
            {
                case ELayoutType.Grid:
                    yield return nameof(LayoutSettings.GridColumns);
                    break;
                case ELayoutType.Circle:
                    yield return nameof(LayoutSettings.CircleRadius);
                    break;
                case ELayoutType.Line:
                default:
                    break;
            }
        }
    }
}
#endif