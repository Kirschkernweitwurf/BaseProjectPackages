using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>Applies the background tint of <see cref="GUIColorAttribute"/> before the field draws.</summary>
    public sealed class GUIColorBeforeHandler : IBeforeFieldHandler
    {
        public int Order => 100;

        public void BeforeField(in MemberContext context)
        {
            GUIColorAttribute attribute = context.GetAttribute<GUIColorAttribute>();
            if (attribute == null)
                return;

            if (ColorAttributeUtility.TryResolve(attribute.ColorHex, attribute.PresetColor, out Color color))
                GUI.backgroundColor = color;
        }
    }
}