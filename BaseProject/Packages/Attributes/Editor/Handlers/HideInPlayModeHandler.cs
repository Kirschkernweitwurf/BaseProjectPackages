using Base.AttributePackage.Editor.Handlers;
using UnityEngine;

namespace Base.AttributePackage.Editor.Drawers
{
    /// <summary>Hides <see cref="HideInPlayModeAttribute"/> fields while in play mode.</summary>
    internal sealed class HideInPlayModeHandler : IVisibilityHandler
    {
        public bool ShouldShow(in MemberContext context)
            => context.GetAttribute<HideInPlayModeAttribute>() == null || !Application.isPlaying;
    }
}