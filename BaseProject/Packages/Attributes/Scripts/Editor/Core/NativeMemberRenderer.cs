using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Draws read-only values for members marked with <see cref="ShowNonSerializedAttribute"/>
    /// and <see cref="ShowNativePropertyAttribute"/>. The annotated members are collected once per
    /// type and cached, so repaints do not scan any members.
    /// </summary>
    /// <remarks>
    /// These members are drawn after every serialized field, outside the pipeline the handlers run in,
    /// so the decorations that pipeline provides have to be repeated here. Only the two that make sense
    /// on a read-only value are: <see cref="TitleAttribute"/> to open a section and
    /// <see cref="InfoBoxAttribute"/> to explain one.
    /// </remarks>
    internal static class NativeMemberRenderer
    {
        private const string NullText = "null";

        // Static is included so a const can carry a section header, which is the only way to put a title
        // in front of the native members: they are drawn after the serialized fields, so no serialized
        // field is in the right place to hold one.
        private const BindingFlags MemberFlags = BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.Public
            | BindingFlags.NonPublic;

        private static readonly Dictionary<Type, FieldInfo[]> Fields = new();

        private static readonly Dictionary<Type, PropertyInfo[]> Properties = new();

        /// <summary>Draws all native members for the edited object.</summary>
        /// <param name="editor">The editor whose target is drawn.</param>
        public static void Draw(UnityEditor.Editor editor)
        {
            Object target = editor.target;
            Type type = target.GetType();

            bool visible = true;

            foreach (FieldInfo field in GetFields(type))
            {
                DrawDecorations(type, target, field, ref visible);

                if (!visible)
                    continue;

                DrawValue(ObjectNames.NicifyVariableName(field.Name), Read(field, target));
            }

            foreach (PropertyInfo property in GetProperties(type))
            {
                DrawDecorations(type, target, property, ref visible);

                if (!visible)
                    continue;

                DrawValue(ObjectNames.NicifyVariableName(property.Name), Read(property, target));
            }
        }

        // A collapsible title closes whatever section came before it, so the members between two titles
        // belong to the first one. That mirrors how the serialized fields above behave.
        private static void DrawDecorations(Type type, Object target, MemberInfo member, ref bool visible)
        {
            TitleAttribute title = member.GetCustomAttribute<TitleAttribute>();

            if (title != null)
            {
                string text = Resolve(type, target, title.Title);
                visible = !title.Foldout || TitleRenderer.DrawCollapsible(type, title, text);

                if (!title.Foldout)
                    TitleRenderer.DrawPlain(title, text);
            }

            if (!visible)
                return;

            InfoBoxAttribute box = member.GetCustomAttribute<InfoBoxAttribute>();
            InfoBoxRenderer.Draw(box, Resolve(type, target, box?.Message));
        }

        // The native members are drawn outside the member pipeline, so there is no MemberContext to
        // resolve against and the target stands in for it.
        private static string Resolve(Type type, Object target, string value)
            => ValueResolver.IsMemberReference(value)
                && ValueResolver.TryRead(type, target, ValueResolver.MemberName(value), out object read)
                    ? read?.ToString() ?? string.Empty
                    : value;

        private static object Read(FieldInfo field, Object target)
        {
            try
            {
                // A const or static field ignores the target, which is what lets a const carry a header.
                return field.IsStatic
                    ? field.GetValue(null)
                    : field.GetValue(target);
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        private static object Read(PropertyInfo property, Object target)
        {
            try
            {
                return property.GetValue(target, null);
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }

        private static FieldInfo[] GetFields(Type type)
        {
            if (Fields.TryGetValue(type, out FieldInfo[] cached))
                return cached;

            List<FieldInfo> fields = new();

            foreach (FieldInfo field in type.GetFields(MemberFlags))
            {
                if (field.GetCustomAttribute<ShowNonSerializedAttribute>() != null)
                    fields.Add(field);
            }

            FieldInfo[] result = fields.ToArray();
            Fields[type] = result;
            return result;
        }

        private static PropertyInfo[] GetProperties(Type type)
        {
            if (Properties.TryGetValue(type, out PropertyInfo[] cached))
                return cached;

            List<PropertyInfo> properties = new();

            foreach (PropertyInfo property in type.GetProperties(MemberFlags))
            {
                if (property.CanRead && property.GetCustomAttribute<ShowNativePropertyAttribute>() != null)
                    properties.Add(property);
            }

            PropertyInfo[] result = properties.ToArray();
            Properties[type] = result;
            return result;
        }

        private static void DrawValue(string label, object value)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                switch (value)
                {
                    case null:
                        EditorGUILayout.TextField(label, NullText);
                        break;
                    case int intValue:
                        EditorGUILayout.IntField(label, intValue);
                        break;
                    case float floatValue:
                        EditorGUILayout.FloatField(label, floatValue);
                        break;
                    case bool boolValue:
                        EditorGUILayout.Toggle(label, boolValue);
                        break;
                    case string stringValue:
                        EditorGUILayout.TextField(label, stringValue);
                        break;
                    case Vector2 vector2Value:
                        EditorGUILayout.Vector2Field(label, vector2Value);
                        break;
                    case Vector3 vector3Value:
                        EditorGUILayout.Vector3Field(label, vector3Value);
                        break;
                    case Color colorValue:
                        EditorGUILayout.ColorField(label, colorValue);
                        break;
                    case Enum enumValue:
                        EditorGUILayout.TextField(label, enumValue.ToString());
                        break;
                    case Object objectValue:
                        EditorGUILayout.ObjectField(label, objectValue, objectValue.GetType(), true);
                        break;
                    default:
                        EditorGUILayout.LabelField(label, value.ToString());
                        break;
                }
            }
        }
    }
}