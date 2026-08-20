using Base.AttributePackage.Editor.Core.Interfaces;
using Base.AttributePackage.Editor.Drawers;
using UnityEngine;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>Resets the background tint applied by <see cref="GUIColorAttribute"/> after the field draws.</summary>
    internal sealed class GUIColorResetHandler : IAfterFieldHandler
    {
        public int Order => 0;

        public void AfterField(in MemberContext context)
        {
            if (context.GetAttribute<GUIColorAttribute>() != null)
                GUI.backgroundColor = Color.white;
        }
    }
}