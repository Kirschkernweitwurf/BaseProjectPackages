using Base.AttributePackage.ColorAttributes;
using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>Resets the background tint applied by <see cref="GUIColorAttribute"/> after the field draws.</summary>
    public sealed class GUIColorResetHandler : IAfterFieldHandler
    {
        public int Order => 0;

        public void AfterField(in MemberContext context)
        {
            if (context.GetAttribute<GUIColorAttribute>() != null)
                GUI.backgroundColor = Color.white;
        }
    }
}