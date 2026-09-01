using Base.AttributePackage.Editor.Core.Interfaces;
using UnityEngine;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>Resets the background tint applied by <see cref="GUIColorAttribute"/> after the field draws.</summary>
    internal sealed class GUIColorResetHandler : IAfterFieldHandler
    {
        /// <inheritdoc/>
        public int Order => 0;

        /// <inheritdoc/>
        public void AfterField(in MemberContext context)
        {
            if (context.GetAttribute<GUIColorAttribute>() != null)
                GUI.backgroundColor = Color.white;
        }
    }
}