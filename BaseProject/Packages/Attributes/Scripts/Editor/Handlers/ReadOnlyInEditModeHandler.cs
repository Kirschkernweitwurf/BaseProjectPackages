using Base.AttributePackage.Conditional;
using Base.AttributePackage.Editor.Core;
using Base.AttributePackage.Editor.Core.Interfaces;
using UnityEngine;

namespace Base.AttributePackage.Editor.Handlers
{
    /// <summary>Disables <see cref="ReadOnlyInEditModeAttribute"/> fields while not in play mode.</summary>
    public sealed class ReadOnlyInEditModeHandler : IEnableHandler
    {
        public bool ShouldEnable(in MemberContext context)
            => context.GetAttribute<ReadOnlyInEditModeAttribute>() == null || Application.isPlaying;
    }
}