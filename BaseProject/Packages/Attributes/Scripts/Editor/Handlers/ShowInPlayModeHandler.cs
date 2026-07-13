using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>Hides <see cref="ShowInPlayModeAttribute"/> fields while not in play mode.</summary>
    public sealed class ShowInPlayModeHandler : IVisibilityHandler
    {
        public bool ShouldShow(in MemberContext context)
            => context.GetAttribute<ShowInPlayModeAttribute>() == null || Application.isPlaying;
    }
}
