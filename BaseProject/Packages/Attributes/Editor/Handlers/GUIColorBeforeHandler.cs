using Base.AttributePackage.Editor.Core;
using Base.AttributePackage.Editor.Core.Interfaces;
using UnityEngine;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>Applies the background tint of <see cref="GUIColorAttribute"/> before the field draws.</summary>
    internal sealed class GUIColorBeforeHandler : IBeforeFieldHandler
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