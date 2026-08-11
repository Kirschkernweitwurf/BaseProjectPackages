using UnityEditor;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Shared drawing for <see cref="InfoBoxAttribute"/>. Used by <see cref="InfoBoxHandler"/> for
    /// serialized fields and by <see cref="NativeMemberRenderer"/> for the read-only members below them,
    /// so a box looks the same wherever it is declared.
    /// </summary>
    public static class InfoBoxRenderer
    {
        /// <summary>Draws the box for the given attribute.</summary>
        /// <param name="attribute">The attribute to draw.</param>
        public static void Draw(InfoBoxAttribute attribute)
        {
            if (attribute == null)
                return;

            if (attribute.Compact || attribute.HasExplicitColor)
                CompactHelpBox.Draw(attribute.Message, attribute.Type, attribute.ColorHex, attribute.PresetColor);
            else
                EditorGUILayout.HelpBox(attribute.Message, ToMessageType(attribute.Type));
        }

        /// <summary>Maps the package's box type onto Unity's own.</summary>
        /// <param name="type">The box type to map.</param>
        /// <returns>The matching message type.</returns>
        public static MessageType ToMessageType(EInfoBoxType type)
        {
            switch (type)
            {
                case EInfoBoxType.Info:
                    return MessageType.Info;
                case EInfoBoxType.Warning:
                    return MessageType.Warning;
                case EInfoBoxType.Error:
                    return MessageType.Error;
                default:
                    return MessageType.None;
            }
        }
    }
}
