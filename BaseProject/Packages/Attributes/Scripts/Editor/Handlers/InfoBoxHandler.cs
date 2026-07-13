using UnityEditor;

namespace Base.AttributePackage.Editor
{
    /// <summary>Draws the box for <see cref="InfoBoxAttribute"/>, above or below, compact or full.</summary>
    public sealed class InfoBoxHandler : IBeforeFieldHandler, IAfterFieldHandler
    {
        public int Order => 20;

        public void AfterField(in MemberContext context) => Draw(context, EInfoBoxPosition.Below);

        public void BeforeField(in MemberContext context) => Draw(context, EInfoBoxPosition.Above);

        private static void Draw(in MemberContext context, EInfoBoxPosition position)
        {
            InfoBoxAttribute attribute = context.GetAttribute<InfoBoxAttribute>();
            if (attribute == null || attribute.Position != position)
                return;

            if (attribute.Compact || attribute.HasExplicitColor)
                CompactHelpBox.Draw(attribute.Message, attribute.Type, attribute.ColorHex, attribute.PresetColor);
            else
                EditorGUILayout.HelpBox(attribute.Message, ToMessageType(attribute.Type));
        }

        private static MessageType ToMessageType(EInfoBoxType type)
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