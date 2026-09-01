using Base.AttributePackage.Editor.Core.Interfaces;
using UnityEngine;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>Hides <see cref="HideInPlayModeAttribute"/> fields while in play mode.</summary>
    internal sealed class HideInPlayModeHandler : IVisibilityHandler
    {
        public bool ShouldShow(in MemberContext context)
            => context.GetAttribute<HideInPlayModeAttribute>() == null || !Application.isPlaying;
    }
}