using System;
using UnityEngine;

namespace Base.UtilityPackage.Tests
{
    /// <summary>A struct holding a tick count, standing in for a serializable date or duration.</summary>
    [Serializable]
    internal struct TickWrapper
    {
        [SerializeField] internal long ticks;
    }
}