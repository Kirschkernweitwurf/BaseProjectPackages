using System;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot.Samples
{
    /// <summary>
    /// Second implementation of <see cref="ISampleAbility"/>, so the reference picker in the showcase has
    /// more than one type to offer and swapping between them can actually be seen.
    /// </summary>
    [Serializable]
    public sealed class SampleHealAbility : ISampleAbility
    {
        /// <summary>Health restored per use.</summary>
        [SerializeField] private int amount = 25;

        /// <summary>Whether the heal also removes status effects.</summary>
        [SerializeField] private bool cleanses;

        /// <inheritdoc/>
        public string DisplayName => nameof(SampleHealAbility);

        /// <summary>Health restored per use.</summary>
        public int Amount => amount;

        /// <summary>Whether the heal also removes status effects.</summary>
        public bool Cleanses => cleanses;
    }
}
