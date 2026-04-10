using System;
using UnityEngine;

namespace Systems.Tweening.Core.Data
{
    /// <summary>
    /// Settings for looping tweens.
    /// </summary>
    [Serializable]
    public class LoopSettings
    {
        [field: Tooltip("Number of times to loop the tween. Set to -1 for infinite loops.")]
        [field: SerializeField] public int LoopCount { get; private set; }

        [field: Tooltip("Type of looping behavior.")]
        [field: SerializeField] public ELoopType LoopType { get; private set; }
    }
}