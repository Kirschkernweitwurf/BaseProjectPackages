using System;
using UnityEngine;

namespace Base.UtilityPackage.Tests
{
    /// <summary>A struct with no tick count in it, so resolving one has to fail.</summary>
    [Serializable]
    internal struct EmptyWrapper
    {
        [SerializeField] internal int unrelated;
    }
}