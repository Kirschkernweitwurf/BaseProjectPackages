using System;
using UnityEngine;

namespace Base.AttributesPackage.Editor.Windows.AttributeExplorer.Showcase
{
    /// <summary>
    /// Two-field value object, used by the showcase to show what an inline property saves: three rows
    /// and a click for two numbers, against one row.
    /// </summary>
    [Serializable]
    internal sealed class ShowcaseRange
    {
        /// <summary>Low end of the range.</summary>
        [Tooltip("Low end of the range.")]
        public float min = 1f;

        /// <summary>High end of the range.</summary>
        [Tooltip("High end of the range.")]
        public float max = 5f;
    }
}