using System;
using System.Collections.Generic;
using System.Reflection;
using Base.UtilityPackage.Editor.Dropdown;
using Base.UtilityPackage.Serialization;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Base.UtilityPackage.Editor.Serialization
{
    /// <summary>
    /// Draws a <see cref="TypeReference"/> as a searchable type picker. A field typed as
    /// <see cref="TypeReferenceOfBase{TBase}"/> only offers types assignable to that base, which is why
    /// no filter attribute exists.
    /// </summary>
    // Registered for the generic subclass as well. Unity resolves a drawer for children by walking the
    // base chain, and it does not reliably match an open generic type that way, so a list of
    // TypeReferenceOfBase would otherwise fall back to Unity's own drawing and show the stored assembly
    // qualified name in full.
    [CustomPropertyDrawer(typeof(TypeReference), true)]
    [CustomPropertyDrawer(typeof(TypeReferenceOfBase<>), true)]
    public sealed class TypeReferenceDrawer : PropertyDrawer
    {
        private const string BrokenSuffix = " (missing)";
        private const string ClearLabel = "None";
        private const string NoCandidatesMessage = "No type is assignable to the declared base type.";
        private const string NoProjectTypesMessage = "No project type was found. Add [TypeScope] to offer "
            + "Unity and framework types as well.";
        private const float WarningHeightLines = 2f;
        private const float WarningSpacing = 2f;

        // Kept alive between openings so the dropdown remembers its scroll position and search text.
        private readonly AdvancedDropdownState _state = new();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float line = EditorGUIUtility.singleLineHeight;

            return HasCandidates()
                ? line
                : line + WarningSpacing + line * WarningHeightLines;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty stored = property.FindPropertyRelative(TypeReference.TypeNameField);
            if (stored == null)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            Rect fieldRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginProperty(fieldRect, label, stored);
            Draw(fieldRect, stored, label);
            EditorGUI.EndProperty();

            if (HasCandidates())
                return;

            Rect warningRect = new(position.x, fieldRect.yMax + WarningSpacing, position.width,
                EditorGUIUtility.singleLineHeight * WarningHeightLines);

            EditorGUI.HelpBox(warningRect, ResolveBaseType(fieldInfo) == typeof(object)
                ? NoProjectTypesMessage
                : NoCandidatesMessage, MessageType.Warning);
        }

        private static string LabelFor(Type type) => string.IsNullOrEmpty(type.Namespace)
            ? type.Name
            : type.Namespace.Replace('.', '/') + "/" + type.Name;

        private static string CaptionFor(SerializedProperty stored)
        {
            if (string.IsNullOrEmpty(stored.stringValue))
                return ClearLabel;

            Type resolved = Type.GetType(stored.stringValue);

            return resolved == null
                ? ShortName(stored.stringValue) + BrokenSuffix
                : resolved.Name;
        }

        // The stored value is an assembly qualified name; everything past the first comma is assembly
        // information the user does not need to read while the type is missing.
        private static string ShortName(string assemblyQualifiedName)
        {
            int comma = assemblyQualifiedName.IndexOf(',');
            string full = comma < 0
                ? assemblyQualifiedName
                : assemblyQualifiedName[..comma];

            int dot = full.LastIndexOf('.');

            return dot < 0
                ? full
                : full[(dot + 1)..];
        }

        // The declared base comes from the generic argument of TypeReferenceOfBase, so an untyped
        // TypeReference offers everything and a typed one narrows itself without an attribute.
        private static Type ResolveBaseType(FieldInfo field)
        {
            if (field == null)
                return typeof(object);

            Type current = ElementType(field.FieldType);

            while (current != null)
            {
                if (current.IsGenericType
                    && current.GetGenericTypeDefinition() == typeof(TypeReferenceOfBase<>))
                    return current.GetGenericArguments()[0];

                current = current.BaseType;
            }

            return typeof(object);
        }

        private static Type ElementType(Type type)
        {
            if (type.IsArray)
                return type.GetElementType();

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return type.GetGenericArguments()[0];

            return type;
        }

        private bool HasCandidates() => Candidates().Length > 0;

        // The scope only matters for an unconstrained field, so an attribute on a constrained one is
        // harmless rather than contradictory: TypeCandidates ignores it there.
        private Type[] Candidates()
        {
            ETypeScope scope = fieldInfo?.GetCustomAttribute<TypeScopeAttribute>()?.Scope ?? ETypeScope.Project;

            return TypeCandidates.For(ResolveBaseType(fieldInfo), scope);
        }

        private void Draw(Rect rect, SerializedProperty stored, GUIContent label)
        {
            Rect buttonRect = EditorGUI.PrefixLabel(rect, label);

            if (!EditorGUI.DropdownButton(buttonRect, new GUIContent(CaptionFor(stored)), FocusType.Keyboard))
                return;

            Type[] candidates = Candidates();

            List<string> labels = new(candidates.Length + 1)
            {
                ClearLabel
            };

            foreach (Type candidate in candidates)
                labels.Add(LabelFor(candidate));

            // The property is captured for the callback, which runs after this OnGUI call has returned.
            SerializedProperty captured = stored.Copy();

            SearchableDropdown menu = new(_state, label.text, labels, index =>
            {
                captured.stringValue = index <= 0
                    ? string.Empty
                    : candidates[index - 1].AssemblyQualifiedName;

                captured.serializedObject.ApplyModifiedProperties();
            });

            menu.Show(buttonRect);
        }
    }
}