using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>Disables <see cref="ReadOnlyInPlayModeAttribute"/> fields while in play mode.</summary>
    public sealed class ReadOnlyInPlayModeHandler : IEnableHandler
    {
        public bool ShouldEnable(in MemberContext context)
            => context.GetAttribute<ReadOnlyInPlayModeAttribute>() == null || !Application.isPlaying;
    }
}