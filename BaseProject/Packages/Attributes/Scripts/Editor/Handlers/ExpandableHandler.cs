using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws the inspector of an <see cref="ExpandableAttribute"/> reference inline, inside a boxed and
    /// indented block below the field, so a ScriptableObject can be edited without leaving the current
    /// selection. The nested editor is cached, since recreating it every repaint leaks native objects.
    /// </summary>
    public sealed class ExpandableHandler : IAfterFieldHandler
    {
        private const float BoxPadding = 4f;
        private const int HandlerOrder = 80;

        public int Order => HandlerOrder;

        public void AfterField(in MemberContext context)
        {
            ExpandableAttribute attribute = context.GetAttribute<ExpandableAttribute>();
            if (attribute == null)
                return;

            if (context.Property.propertyType != SerializedPropertyType.ObjectReference)
                return;

            Object target = context.Property.objectReferenceValue;
            if (target == null)
                return;

            if (!ExpandableToggleWidget.IsExpanded(context, attribute))
                return;

            UnityEditor.Editor editor = EmbeddedEditorCache.Get(target);
            if (editor == null)
                return;

            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(BoxPadding);

            editor.OnInspectorGUI();

            GUILayout.Space(BoxPadding);
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }
    }
}
