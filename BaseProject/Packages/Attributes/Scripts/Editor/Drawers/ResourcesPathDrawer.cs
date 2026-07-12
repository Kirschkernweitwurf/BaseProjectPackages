using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws a string field as an object picker that stores a Resources-relative path for
    /// <see cref="ResourcesPathAttribute"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(ResourcesPathAttribute))]
    public sealed class ResourcesPathDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use [ResourcesPath] with a string.");
                return;
            }

            ResourcesPathAttribute attribute = (ResourcesPathAttribute)this.attribute;
            Type type = attribute.Type ?? typeof(Object);

            Object current = string.IsNullOrEmpty(property.stringValue)
                ? null
                : Resources.Load(property.stringValue, type);

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            Object picked = EditorGUI.ObjectField(position, label, current, type, false);
            if (EditorGUI.EndChangeCheck())
            {
                if (picked == null)
                {
                    property.stringValue = string.Empty;
                }
                else
                {
                    string resourcesPath = PathUtility.ToResourcesPath(AssetDatabase.GetAssetPath(picked));
                    if (!string.IsNullOrEmpty(resourcesPath))
                        property.stringValue = resourcesPath;
                }
            }

            EditorGUI.EndProperty();
        }
    }
}