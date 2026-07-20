using System;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Cached labels, values and sizing for one enum type drawn by
    /// <see cref="EnumToggleButtonsDrawer"/>, so names and button widths are not recomputed every
    /// repaint. For flags enums the labels map to <see cref="Values"/>, for plain enums they map to
    /// Unity's enum value index.
    /// </summary>
    public sealed class EnumButtonLayout
    {
        private const float ButtonPadding = 12f;
        private const float MinimumWidth = 40f;

        /// <summary>Display label per button.</summary>
        public string[] Labels { get; private set; }

        /// <summary>Flag bits per button. Null for plain enums, which map by value index instead.</summary>
        public int[] Values { get; private set; }

        /// <summary>Whether the enum is a flags enum and the buttons multi-select.</summary>
        public bool IsFlags { get; private set; }

        /// <summary>Width the widest label needs, used to decide how many buttons fit per row.</summary>
        public float MinButtonWidth { get; private set; }

        private EnumButtonLayout() { }

        /// <summary>
        /// Builds the layout for the given enum type, or null when the enum offers no drawable values.
        /// </summary>
        public static EnumButtonLayout Build(Type enumType, SerializedProperty property)
        {
            EnumButtonLayout layout = new()
            {
                IsFlags = Attribute.IsDefined(enumType, typeof(FlagsAttribute))
            };

            if (layout.IsFlags)
                layout.BuildFlags(enumType);
            else
                layout.Labels = property.enumDisplayNames;

            if (layout.Labels == null || layout.Labels.Length == 0)
                return null;

            layout.MeasureButtonWidth();
            return layout;
        }

        private void BuildFlags(Type enumType)
        {
            Array rawValues = Enum.GetValues(enumType);
            string[] rawNames = Enum.GetNames(enumType);

            int count = 0;
            for (int i = 0; i < rawValues.Length; i++)
            {
                if (Convert.ToInt32(rawValues.GetValue(i)) != 0)
                    count++;
            }

            Labels = new string[count];
            Values = new int[count];

            int index = 0;
            for (int i = 0; i < rawValues.Length; i++)
            {
                int value = Convert.ToInt32(rawValues.GetValue(i));
                if (value == 0)
                    continue;

                Labels[index] = ObjectNames.NicifyVariableName(rawNames[i]);
                Values[index] = value;
                index++;
            }
        }

        private void MeasureButtonWidth()
        {
            float widest = MinimumWidth;
            foreach (string label in Labels)
                widest = Mathf.Max(widest, EditorStyles.miniButton.CalcSize(new GUIContent(label)).x + ButtonPadding);

            MinButtonWidth = widest;
        }
    }
}