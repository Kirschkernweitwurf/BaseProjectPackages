using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>Disables <see cref="EnableInPlayModeAttribute"/> fields while not in play mode.</summary>
    public sealed class EnableInPlayModeHandler : IEnableHandler
    {
        public bool ShouldEnable(in MemberContext context)
            => context.GetAttribute<EnableInPlayModeAttribute>() == null || Application.isPlaying;
    }
}
