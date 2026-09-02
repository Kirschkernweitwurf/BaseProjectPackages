using Base.AttributesPackage.Editor.Core.Interfaces;
using UnityEngine;

namespace Base.AttributesPackage.Editor.Handlers
{
    /// <summary>Hides <see cref="HideInPlayModeAttribute"/> fields while in play mode.</summary>
    internal sealed class HideInPlayModeHandler : IVisibilityHandler
    {
        /// <inheritdoc/>
        public bool ShouldShow(in MemberContext context)
            => context.GetAttribute<HideInPlayModeAttribute>() == null || !Application.isPlaying;
    }
}