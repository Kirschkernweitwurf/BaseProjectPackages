using Base.AttributePackage.Editor.Core.Interfaces;
using UnityEngine;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>
    /// Decides the editable state for the two play-mode attributes:
    /// <see cref="DisableInPlayModeAttribute"/> locks a field while the editor is playing and
    /// <see cref="EnableInPlayModeAttribute"/> locks it while it is not.
    /// </summary>
    /// <remarks>
    /// One handler for both, rather than one each. They ask the same question in two directions, and two
    /// classes of a single line apiece cost more to read than the branch below.
    /// </remarks>
    internal sealed class PlayModeEnableHandler : IEnableHandler
    {
        /// <inheritdoc/>
        public bool ShouldEnable(in MemberContext context)
        {
            if (context.GetAttribute<DisableInPlayModeAttribute>() != null)
                return !Application.isPlaying;

            if (context.GetAttribute<EnableInPlayModeAttribute>() != null)
                return Application.isPlaying;

            return true;
        }
    }
}