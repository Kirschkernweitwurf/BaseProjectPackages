using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Shows a compact error per duplicate group of a <see cref="UniqueAttribute"/> list, followed by a
    /// button that removes the repeats. A custom message collapses all groups into a single box.
    /// The button only exists while duplicates exist.
    /// </summary>
    public sealed class UniqueHandler : IAfterFieldHandler
    {
        private const float ButtonHeight = 18f;

        public int Order => 20;

        private static readonly GUIContent FixContent =
            new("Remove Duplicates", "Keeps the first occurrence of every value and removes the repeats.");

        // Reused across draws. Filled and consumed inside one call, so the handler stays stateless.
        private static readonly List<string> Groups = new();
        private static readonly List<int> Repeats = new();

        public void AfterField(in MemberContext context)
        {
            UniqueAttribute attribute = context.GetAttribute<UniqueAttribute>();
            if (attribute == null)
                return;

            if (!context.Property.isArray || context.Property.propertyType == SerializedPropertyType.String)
                return;

            if (context.Field?.GetValue(context.DeclaringObject) is not IList list)
                return;

            DuplicateFinder.Collect(list, Groups);
            if (Groups.Count == 0)
                return;

            DrawMessages(context, attribute);

            // Indices come from the first target only, so a shared fix would corrupt the others.
            if (context.Editor.serializedObject.isEditingMultipleObjects)
                return;

            if (DrawButton())
            {
                DuplicateFinder.CollectRepeats(list, Repeats);
                RemoveRepeats(context.Property, Repeats);
            }
        }

        private static void DrawMessages(in MemberContext context, UniqueAttribute attribute)
        {
            if (attribute.Message != null)
            {
                CompactHelpBox.Error(attribute.Message);
                return;
            }

            foreach (string group in Groups)
                CompactHelpBox.Error(context.DisplayName + " " + DuplicateFinder.Describe(group));
        }

        private static bool DrawButton()
        {
            Rect rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, ButtonHeight));
            return GUI.Button(rect, FixContent, EditorStyles.miniButton);
        }

        private static void RemoveRepeats(SerializedProperty property, List<int> indices)
        {
            for (int i = indices.Count - 1; i >= 0; i--)
            {
                int size = property.arraySize;
                property.DeleteArrayElementAtIndex(indices[i]);

                // Arrays of object references clear the slot on the first call instead of removing it.
                if (property.arraySize == size)
                    property.DeleteArrayElementAtIndex(indices[i]);
            }
        }
    }
}