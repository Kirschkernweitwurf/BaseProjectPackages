using System;
using UnityEngine;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// An element with no string anywhere in it, so there is nothing to name the row after and the
    /// index has to stand in.
    /// </summary>
    [Serializable]
    internal struct UnlabeledEntry
    {
        [SerializeField] private int amount;
        [SerializeField] private float weight;
    }
}