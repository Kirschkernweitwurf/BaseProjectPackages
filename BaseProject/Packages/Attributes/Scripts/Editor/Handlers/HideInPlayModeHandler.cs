using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>Hides <see cref="HideInPlayModeAttribute"/> fields while in play mode.</summary>
    public sealed class HideInPlayModeHandler : IVisibilityHandler
    {
        public bool ShouldShow(in MemberContext context)
            => context.GetAttribute<HideInPlayModeAttribute>() == null || !Application.isPlaying;
    }
}
