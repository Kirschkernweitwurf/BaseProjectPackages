using System;
using UnityEngine;

namespace Base.AttributePackage.Editor.Drawers.Windows.AttributeExplorer.Troubleshoot.Samples
{
    /// <summary>The one implementation of <see cref="ISampleAbility"/>, used by the showcase picker.</summary>
    [Serializable]
    internal sealed class SampleDashAbility : ISampleAbility
    {
        /// <summary>How far the dash travels.</summary>
        [SerializeField] private float distance = 4f;

        /// <inheritdoc/>
        public string DisplayName => nameof(SampleDashAbility);

        /// <summary>How far the dash travels.</summary>
        public float Distance => distance;
    }
}