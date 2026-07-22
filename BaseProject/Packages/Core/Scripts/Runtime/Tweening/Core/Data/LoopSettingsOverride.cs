using System;
using Base.AttributePackage;
using UnityEngine;

namespace Base.CorePackage.Tweening.Core.Data
{
    /// <summary>
    /// Optional per field overrides applied on top of resolved <see cref="LoopSettings"/>.
    /// Every field is opt in, so an untouched override changes nothing.
    /// </summary>
    [Serializable]
    public class LoopSettingsOverride
    {
        [Tooltip("If true, the loop count below replaces the resolved loop count.")]
        [SerializeField] private bool overrideLoopCount;

        [ShowIf(nameof(overrideLoopCount))] [Min(-1)] [SerializeField] private int loopCount;

        [Tooltip("If true, the loop type below replaces the resolved loop type.")]
        [SerializeField] private bool overrideLoopType;

        [ShowIf(nameof(overrideLoopType))] [SerializeField] private ELoopType loopType;

        /// <summary>True if at least one field is overridden.</summary>
        public bool HasAnyOverride => overrideLoopCount || overrideLoopType;

        /// <summary>
        /// Returns an independent copy of <paramref name="source"/> with the enabled overrides
        /// applied. A null source falls back to fresh default settings.
        /// </summary>
        public LoopSettings Apply(LoopSettings source)
        {
            LoopSettings result = source?.Copy() ?? new LoopSettings();

            if (overrideLoopCount)
                result.SetLoopCount(loopCount);

            if (overrideLoopType)
                result.SetLoopType(loopType);

            return result;
        }
    }
}