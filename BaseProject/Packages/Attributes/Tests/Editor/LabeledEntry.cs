using System;
using UnityEngine;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// An element whose first serialized field is a string, which is the shape a list row is meant to
    /// be named after.
    /// </summary>
    [Serializable]
    internal struct LabeledEntry
    {
        /// <summary>Serialized name of the title field, so a test can reach it without a literal.</summary>
        internal const string TitleField = nameof(title);

        [SerializeField] private string title;
        [SerializeField] private int amount;
    }
}