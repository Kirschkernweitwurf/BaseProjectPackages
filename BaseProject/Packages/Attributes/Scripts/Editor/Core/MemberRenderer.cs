using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Runs the per-member pipeline: visibility, enable state, before-field decorations, the field
    /// itself, then after-field handlers. Descends into nested serializable types so the same pipeline
    /// applies at any depth, instead of handing the whole subtree to Unity's default drawing.
    /// </summary>
    public static class MemberRenderer
    {
        private const string CoreLibraryAssembly = "mscorlib";
        private const string SystemAssemblyPrefix = "System";
        private const string UnityAssemblyPrefix = "Unity";

        private const float WidgetGap = 2f;

        private static float[] _widgetWidths;

        /// <summary>Draws a top-level member through all handlers.</summary>
        public static void Draw(SerializedProperty property, FieldInfo field, AttributePackageEditor editor)
            => Draw(property, field, editor.target.GetType(), editor.target, editor);

        private static void Draw(SerializedProperty property, FieldInfo field, Type declaringType,
            object declaringObject, AttributePackageEditor editor)
        {
            Object before = property.propertyType == SerializedPropertyType.ObjectReference
                ? property.objectReferenceValue
                : null;

            MemberContext context =
                new(property, field, editor.target, declaringType, declaringObject, editor, before);

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

            foreach (IBeforeFieldHandler handler in HandlerRegistry.BeforeField)
                handler.BeforeField(context);

            IndentAttribute indent = context.GetAttribute<IndentAttribute>();
            int amount = indent?.Amount ?? 0;

            EditorGUI.indentLevel += amount;
            using (new EditorGUI.DisabledScope(!enabled))
                DrawBody(context, field, editor);

            EditorGUI.indentLevel -= amount;

            foreach (IAfterFieldHandler handler in HandlerRegistry.AfterField)
                handler.AfterField(context);
        }

        private static void DrawBody(in MemberContext context, FieldInfo field, AttributePackageEditor editor)
        {
            SerializedProperty property = context.Property;

            if (!CanDescend(property, field, out Type nestedType))
            {
                DrawLeafField(context);
                return;
            }

            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, context.DisplayName, true);
            if (!property.isExpanded)
                return;

            object instance = SerializedPropertyReflection.GetValue(property);

            EditorGUI.indentLevel++;
            DrawChildren(property, nestedType, instance, editor);
            EditorGUI.indentLevel--;
        }

        private static void DrawLeafField(in MemberContext context)
        {
            SerializedProperty property = context.Property;
            IInlineFieldWidget[] widgets = HandlerRegistry.InlineWidgets;
            _widgetWidths ??= new float[widgets.Length];

            float trailing = 0f;
            for (int i = 0; i < widgets.Length; i++)
            {
                float width = widgets[i].GetWidth(context);
                _widgetWidths[i] = width;
                if (width > 0f)
                    trailing += width + WidgetGap;
            }

            if (trailing <= 0f)
            {
                EditorGUILayout.PropertyField(property, true);
                return;
            }

            float height = EditorGUI.GetPropertyHeight(property, true);
            Rect line = EditorGUILayout.GetControlRect(true, height);
            Rect fieldRect = new(line.x, line.y, line.width - trailing, line.height);
            EditorGUI.PropertyField(fieldRect, property, true);

            float x = fieldRect.xMax + WidgetGap;
            for (int i = 0; i < widgets.Length; i++)
            {
                float width = _widgetWidths[i];
                if (width <= 0f)
                    continue;

                Rect widgetRect = new(x, line.y, width, EditorGUIUtility.singleLineHeight);
                widgets[i].Draw(widgetRect, context);
                x += width + WidgetGap;
            }
        }

        private static void DrawChildren(SerializedProperty parent, Type declaringType, object declaringObject,
            AttributePackageEditor editor)
        {
            SerializedProperty iterator = parent.Copy();
            SerializedProperty end = parent.GetEndProperty();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                enterChildren = false;
                FieldInfo childField = ReflectionCache.GetField(declaringType, iterator.name);
                Draw(iterator.Copy(), childField, declaringType, declaringObject, editor);
            }
        }

        private static bool CanDescend(SerializedProperty property, FieldInfo field, out Type nestedType)
        {
            nestedType = null;

            if (property.propertyType != SerializedPropertyType.Generic || property.isArray)
                return false;

            nestedType = field?.FieldType;
            if (nestedType == null || nestedType == typeof(string))
                return false;

            if (IsFrameworkType(nestedType))
                return false;

            return !PropertyDrawerCache.HasDrawer(nestedType);
        }

        private static bool IsFrameworkType(Type type)
        {
            string assembly = type.Assembly.GetName().Name;
            return assembly.StartsWith(UnityAssemblyPrefix)
                || assembly.StartsWith(SystemAssemblyPrefix)
                || assembly == CoreLibraryAssembly;
        }
    }
}