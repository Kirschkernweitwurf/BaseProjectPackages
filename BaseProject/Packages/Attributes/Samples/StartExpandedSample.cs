using System;
using UnityEngine;

namespace Base.AttributesPackage.Samples
{
    /// <summary>A field that opens itself the first time it is seen.</summary>
    [AttributeSample(typeof(StartExpandedAttribute), EAttributeCategory.Layout,
        Description = "Opens the field the first time it is seen this session. Only the first draw is forced, so "
            + "folding it up afterwards sticks.",
        Requirements = "Nothing.",
        Variations = new[]
        {
            "Works on anything with a foldout: arrays, lists and nested serializable types.",
            "Arrays and lists already open on their own, so the attribute changes nothing there. A nested "
            + "serializable type is what it is actually for."
        })]
    internal sealed class StartExpandedSample : ScriptableObject
    {
        [StartExpanded]
        [Tooltip("Open on the first draw. Fold it up and it stays folded.")]
        public Group expandedGroup = new();

        [Tooltip("The same nested type without the attribute, folded away as usual. This is the pair that shows "
            + "what the attribute does.")]
        public Group foldedGroup = new();

        [StartExpanded]
        [Tooltip("A collection with the attribute.")]
        public string[] expandedArray =
        {
            "first",
            "second"
        };

        [Tooltip("The same collection without the attribute. Unity already opens arrays by default, so both look "
            + "the same and the attribute is redundant here.")]
        public string[] plainArray =
        {
            "first",
            "second"
        };

        /// <summary>A small nested type, present only to have a foldout to open.</summary>
        [Serializable]
        public sealed class Group
        {
            /// <summary>Any value. Only the foldout above it matters.</summary>
            public string label = "value";

            /// <summary>A second value, so the opened state is obvious at a glance.</summary>
            public int amount = 1;
        }
    }
}