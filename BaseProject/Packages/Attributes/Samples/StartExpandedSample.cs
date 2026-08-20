using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A collection that opens itself the first time.</summary>
    [AttributeSample(typeof(StartExpandedAttribute), EAttributeCategory.Layout,
        Description = "Opens the field the first time it is seen. Only the first draw is forced, so folding it up "
            + "afterwards sticks.",
        Requirements = "Nothing.",
        Variations = new[]
        {
            "Works on anything with a foldout: arrays, lists and nested serializable types."
        })]
    internal sealed class StartExpandedSample : ScriptableObject
    {
        [StartExpanded]
        [Tooltip("Open on the first draw. Fold it up and it stays folded.")]
        public string[] expanded =
        {
            "first",
            "second"
        };

        [Tooltip("The same collection without the attribute, closed as usual.")]
        public string[] collapsed =
        {
            "first",
            "second"
        };
    }
}