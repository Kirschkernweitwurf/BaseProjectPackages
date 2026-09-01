using Base.AttributePackage.Editor.Core.Interfaces;
using UnityEngine;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>Hides <see cref="ShowInPlayModeAttribute"/> fields while not in play mode.</summary>
    internal sealed class ShowInPlayModeHandler : IVisibilityHandler
    {
        /// <inheritdoc/>
        public bool ShouldShow(in MemberContext context)
            => context.GetAttribute<ShowInPlayModeAttribute>() == null || Application.isPlaying;
    }
}