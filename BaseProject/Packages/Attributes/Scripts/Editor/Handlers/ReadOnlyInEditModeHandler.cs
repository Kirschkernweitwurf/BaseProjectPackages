using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>Disables <see cref="ReadOnlyInEditModeAttribute"/> fields while not in play mode.</summary>
    internal sealed class ReadOnlyInEditModeHandler : IEnableHandler
    {
        public bool ShouldEnable(in MemberContext context)
            => context.GetAttribute<ReadOnlyInEditModeAttribute>() == null || Application.isPlaying;
    }
}