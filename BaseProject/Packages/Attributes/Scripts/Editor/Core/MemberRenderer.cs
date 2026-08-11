using System;
using System.Collections.Generic;
using System.Reflection;
using Base.AttributePackage.Editor.Collections;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Runs the per-member pipeline: visibility, enable state, before-field decorations, the field
    /// itself, then after-field handlers. Descends into nested serializable types so the same pipeline
    /// applies at any depth, instead of handing the whole subtree to Unity's default drawing. Arrays
    /// carrying a collection attribute are handed to their own renderer instead of to Unity's list.
    /// </summary>
    public static class MemberRenderer
    {
        private const float WidgetGap = 2f;

        private static float[] _widgetWidths;

        /// <summary>Draws a top-level member through all handlers.</summary>
        /// <param name="property">The property to draw.</param>
        /// <param name="field">The reflected field behind it.</param>
        /// <param name="editor">The editor drawing it.</param>
        /// <param name="showLabel">False to give the value the whole row, for a horizontal cell.</param>
        public static void Draw(SerializedProperty property, FieldInfo field, AttributePackageEditor editor,
            bool showLabel = true)
            => Draw(property, field, editor.target.GetType(), editor.target, editor, showLabel);

        private static void Draw(SerializedProperty property, FieldInfo field, Type declaringType,
            object declaringObject, AttributePackageEditor editor, bool showLabel = true)
        {
            Object before = property.propertyType == SerializedPropertyType.ObjectReference
                ? property.objectReferenceValue
                : null;

            MemberContext context =
                new(property, field, editor.target, declaringType, declaringObject, editor, before, showLabel);

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

            // A member with a control in front of its label is indented one step so that control has a
            // gutter to sit in, the same room Unity gives any other foldout arrow.
            if (LeadingGutter.IsNeeded(context))
                amount++;

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

            if (TryDrawCollection(context, field))
                return;

            if (!CanDescend(property, field, out Type nestedType))
            {
                DrawLeafField(context);
                return;
            }

            // An inline property draws its children on the field's own row, so it never opens a foldout.
            if (InlinePropertyRenderer.TryDraw(context, nestedType))
                return;

            property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, context.Label, true);
            if (!property.isExpanded)
                return;

            object instance = SerializedPropertyReflection.GetValue(property);

            EditorGUI.indentLevel++;
            DrawChildren(property, nestedType, instance, editor);
            EditorGUI.indentLevel--;
        }

        // An array is normally handed to Unity's own list drawing. [Table] and [ListDrawerSettings]
        // replace that wholesale, so they are checked before anything else decides how to draw.
        private static bool TryDrawCollection(in MemberContext context, FieldInfo field)
        {
            SerializedProperty property = context.Property;

            if (!property.isArray || property.propertyType == SerializedPropertyType.String)
                return false;

            TableAttribute table = context.GetAttribute<TableAttribute>();
            if (table != null)
            {
                TableRenderer.Draw(property, context.Label, ElementType(field), table,
                    ArraySizeLimits.CanResize(context));
                return true;
            }

            ListDrawerSettingsAttribute settings = context.GetAttribute<ListDrawerSettingsAttribute>();
            if (settings == null)
                return false;

            ListDrawerRenderer.Draw(property, context.Label, settings, ArraySizeLimits.CanResize(context));
            return true;
        }

        private static Type ElementType(FieldInfo field)
        {
            Type type = field?.FieldType;
            if (type == null)
                return null;

            if (type.IsArray)
                return type.GetElementType();

            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)
                ? type.GetGenericArguments()[0]
                : type;
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

            GUIContent label = context.EffectiveLabel;

            if (trailing <= 0f)
            {
                EditorGUILayout.PropertyField(property, label, true);
                return;
            }

            float height = EditorGUI.GetPropertyHeight(property, true);
            Rect line = EditorGUILayout.GetControlRect(true, height);
            Rect fieldRect = new(line.x, line.y, line.width - trailing, line.height);
            EditorGUI.PropertyField(fieldRect, property, label, true);

            float x = fieldRect.xMax + WidgetGap;

            // Every widget is handed a rect worked out here, so none of them may have the indent applied
            // to it a second time. A widget sized to its own text loses exactly that much width and ends
            // up with the last characters cut off.
            using (new NoIndentScope())
            {
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

            if (FrameworkAssemblies.Contains(nestedType))
                return false;

            return !PropertyDrawerCache.HasDrawer(nestedType);
        }
    }
}