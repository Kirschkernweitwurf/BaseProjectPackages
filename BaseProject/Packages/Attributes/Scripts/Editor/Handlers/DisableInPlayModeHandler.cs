using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>Disables <see cref="DisableInPlayModeAttribute"/> fields while in play mode.</summary>
    public sealed class DisableInPlayModeHandler : IEnableHandler
    {
        public bool ShouldEnable(in MemberContext context)
            => context.GetAttribute<DisableInPlayModeAttribute>() == null || !Application.isPlaying;
    }
}