using UnityEngine;

namespace Base.AttributePackage.Editor
{
    /// <summary>
    /// Decides the editable state for the play-mode attributes. <see cref="ReadOnlyInPlayModeAttribute"/>
    /// and <see cref="DisableInPlayModeAttribute"/> disable the field while in play mode,
    /// <see cref="ReadOnlyInEditModeAttribute"/> and <see cref="EnableInPlayModeAttribute"/> while not.
    /// The two pairs are aliases of each other, so one handler covers all four.
    /// </summary>
    /// <remarks>
    /// One handler for all four, rather than one each. They ask the same question in two directions, and
    /// four classes of a single line apiece cost more to read than the branch below.
    /// </remarks>
    internal sealed class PlayModeEnableHandler : IEnableHandler
    {
        public bool ShouldEnable(in MemberContext context)
        {
            if (context.GetAttribute<ReadOnlyInPlayModeAttribute>() != null
                || context.GetAttribute<DisableInPlayModeAttribute>() != null)
                return !Application.isPlaying;

            if (context.GetAttribute<ReadOnlyInEditModeAttribute>() != null
                || context.GetAttribute<EnableInPlayModeAttribute>() != null)
                return Application.isPlaying;

            return true;
        }
    }
}