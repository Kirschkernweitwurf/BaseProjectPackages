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

        private static GUIStyle _style;

        public void BeforeField(in MemberContext context)
        {
            TitleAttribute attribute = context.GetAttribute<TitleAttribute>();
            if (attribute == null)
                return;

            bool hasColor =
                ColorAttributeUtility.TryResolve(attribute.ColorHex, attribute.PresetColor, out Color color);

            GUILayout.Space(6f);

            if (_style == null)
                _style = new GUIStyle(EditorStyles.boldLabel);

            _style.normal.textColor = hasColor
                ? color
                : EditorStyles.boldLabel.normal.textColor;

            EditorGUILayout.LabelField(attribute.Title, _style);

            Rect lineRect = EditorGUILayout.GetControlRect(false, 3f);
            lineRect.height = 1f;
            EditorGUI.DrawRect(lineRect, hasColor
                ? color
                : new Color(0.5f, 0.5f, 0.5f, 0.5f));

            GUILayout.Space(2f);
        }
    }
}