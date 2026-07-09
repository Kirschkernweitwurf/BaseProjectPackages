using UnityEditor;

namespace Base.AttributePackage.Editor
{
    /// <summary>Draws the help box for <see cref="InfoBoxAttribute"/>.</summary>
    public sealed class InfoBoxHandler : IBeforeFieldHandler
    {
        public int Order => 20;

        public void BeforeField(in MemberContext context)
        {
            InfoBoxAttribute attribute = context.GetAttribute<InfoBoxAttribute>();
            if (attribute == null)
                return;

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
