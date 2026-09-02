using Base.AttributesPackage.Editor.Drawers;
using Base.AttributesPackage.Editor.Handlers;
using UnityEditor;
using UnityEngine;

namespace Base.AttributesPackage.Editor.Core
{
    /// <summary>
    /// Shared drawing for <see cref="InfoBoxAttribute"/>. Used by <see cref="InfoBoxHandler"/> for
    /// serialized fields and by <see cref="NativeMemberRenderer"/> for the read-only members below them,
    /// so a box looks the same wherever it is declared.
    /// </summary>
    /// <remarks>
    /// The full box is drawn here rather than handed to Unity's own help box. Unity puts its icon flush
    /// against the text with no gap, which on a long message reads as one run of ink starting with a
    /// symbol. Drawing the two apart costs a rect calculation and buys a box that can actually be
    /// skimmed.
    /// </remarks>
    internal static class InfoBoxRenderer
    {
        private const float IconGap = 5f;

        // The large console icons are 32 points. Drawing one at any other size resamples a
        // point-filtered editor texture, which is what makes it look chewed up.
        private const float IconSize = 32f;
        private const float MinimumHeight = 40f;

        // A box with an icon starts at the icon, which is itself a visual left margin. A box without
        // one starts at bare text, and the same padding leaves it hard against the edge.
        private const float PaddingX = 6f;
        private const float PaddingY = 5f;
        private const float TextOnlyPaddingX = 10f;

        private static GUIStyle _label;

        /// <summary>Draws the box for the given attribute with an already resolved message.</summary>
        /// <param name="attribute">The attribute to draw.</param>
        /// <param name="message">The message to show.</param>
        internal static void Draw(InfoBoxAttribute attribute, string message)
        {
            if (attribute == null)
                return;

            if (attribute.Compact || attribute.HasExplicitColor)
            {
                CompactHelpBox.Draw(message, attribute.Type, attribute.ColorHex, attribute.PresetColor);
                return;
            }

            DrawBox(message, attribute.Type);
        }

        private static void DrawBox(string message, EInfoBoxType type)
        {
            Build();

            Texture icon = IconFor(type);
            float indent = EditorGUI.indentLevel * EditorGUIUtility.singleLineHeight;
            float textWidth = EditorGUIUtility.currentViewWidth - indent - PaddingX * 2f - IconSize - IconGap;

            float height = Mathf.Max(_label.CalcHeight(ScratchContent.For(message), textWidth) + PaddingY * 2f,
                MinimumHeight);

            Rect box = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, height));
            GUI.Box(box, GUIContent.none, EditorStyles.helpBox);

            float x = box.x
                + (icon == null
                    ? TextOnlyPaddingX
                    : PaddingX);

            if (icon != null)
            {
                Rect iconRect = new(x, box.y + (box.height - IconSize) * 0.5f, IconSize, IconSize);
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
                x = iconRect.xMax + IconGap;
            }

            Rect text = new(x, box.y + PaddingY, box.xMax - x - PaddingX, box.height - PaddingY * 2f);
            GUI.Label(text, message, _label);
        }

        // The icons are the console ones at their large size, which is what Unity's own help box uses.
        // The small variants look starved next to a wrapped paragraph.
        private static Texture IconFor(EInfoBoxType type)
        {
            switch (type)
            {
                case EInfoBoxType.Info:
                    return EditorGUIUtility.IconContent("console.infoicon").image;
                case EInfoBoxType.Warning:
                    return EditorGUIUtility.IconContent("console.warnicon").image;
                case EInfoBoxType.Error:
                    return EditorGUIUtility.IconContent("console.erroricon").image;
                default:
                    return null;
            }
        }

        private static void Build()
        {
            if (_label != null)
                return;

            _label = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true,
                richText = true
            };
        }
    }
}