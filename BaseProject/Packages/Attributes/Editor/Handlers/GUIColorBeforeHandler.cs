using Base.AttributesPackage.Editor.Core;
using Base.AttributesPackage.Editor.Core.Interfaces;
using UnityEngine;

namespace Base.AttributesPackage.Editor.Handlers
{
    /// <summary>Applies the background tint of <see cref="GUIColorAttribute"/> before the field draws.</summary>
    internal sealed class GUIColorBeforeHandler : IBeforeFieldHandler
    {
        /// <inheritdoc/>
        public int Order => 100;

        /// <inheritdoc/>
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