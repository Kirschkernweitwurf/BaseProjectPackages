using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Runs the per-member pipeline: visibility, enable state, the field itself, then after-field handlers.
    /// </summary>
    public static class MemberRenderer
    {
        /// <summary>Draws a single member through all handlers.</summary>
        public static void Draw(SerializedProperty property, FieldInfo field, AttributePackageEditor editor)
        {
            Object before = property.propertyType == SerializedPropertyType.ObjectReference
                ? property.objectReferenceValue
                : null;

            MemberContext context = new(property, field, editor.target, editor, before);

            foreach (IVisibilityHandler handler in HandlerRegistry.Visibility)
            {
                if (!handler.ShouldShow(context))
                    return;
            }

            bool enabled = true;
            foreach (IEnableHandler handler in HandlerRegistry.Enable)
            {
                if (!handler.ShouldEnable(context))
                {
                    enabled = false;
                    break;
                }
            }

            IndentAttribute indent = context.GetAttribute<IndentAttribute>();
            int amount = indent?.Amount ?? 0;

            EditorGUI.indentLevel += amount;
            using (new EditorGUI.DisabledScope(!enabled))
                EditorGUILayout.PropertyField(property, true);

            EditorGUI.indentLevel -= amount;

            foreach (IAfterFieldHandler handler in HandlerRegistry.AfterField)
                handler.AfterField(context);
        }
    }
}