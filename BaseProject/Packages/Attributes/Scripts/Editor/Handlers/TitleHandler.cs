using Base.AttributePackage.Editor.Core;
using Base.AttributePackage.Editor.Core.Interfaces;
using Base.AttributePackage.Layout;
using UnityEditor;
using UnityEngine;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>Draws the bold title and underline for <see cref="TitleAttribute"/>.</summary>
    public sealed class TitleHandler : IBeforeFieldHandler
    {
        public int Order => 0;

        public void BeforeField(in MemberContext context)
        {
            TitleAttribute attribute = context.GetAttribute<TitleAttribute>();
            if (attribute == null)
                return;

            bool hasColor =
                ColorAttributeUtility.TryResolve(attribute.ColorHex, attribute.PresetColor, out Color color);

            GUILayout.Space(6f);

            GUIStyle style = new(EditorStyles.boldLabel);
            if (hasColor)
                style.normal.textColor = color;

            EditorGUILayout.LabelField(attribute.Title, style);

            Rect lineRect = EditorGUILayout.GetControlRect(false, 3f);
            lineRect.height = 1f;
            EditorGUI.DrawRect(lineRect, hasColor
                ? color
                : new Color(0.5f, 0.5f, 0.5f, 0.5f));

            GUILayout.Space(2f);
        }
    }
}