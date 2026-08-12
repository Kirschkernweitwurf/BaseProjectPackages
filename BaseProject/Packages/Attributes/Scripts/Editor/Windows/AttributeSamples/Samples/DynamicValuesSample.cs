using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples.Samples
{
    /// <summary>Titles, boxes and labels computed from a member instead of typed.</summary>
    [AttributeSample("Layout")]
    internal sealed class DynamicValuesSample : ScriptableObject
    {
        [Title("$" + nameof(Heading), EColor.Purple)]
        [InfoBox("$" + nameof(Status))]
        [Tooltip("The heading above and the box under it both read a member instead of a literal.")]
        public int enemyCount = 3;

        [Label("$" + nameof(CountLabel))]
        public int itemCount = 1;

        [Label("Renamed in the inspector only")]
        [Tooltip("A plain literal label, for a field whose good code name and good inspector name differ.")]
        public string internalName = "The field is still called internalName";

        [DisplayAsString]
        [Tooltip("A collection collapsed to one line of read-only text instead of a foldout and a row each.")]
        public string[] tags =
        {
            "fast",
            "armored",
            "ranged"
        };

        // A string argument starting with a dollar names a member to read. Written as "$" + nameof(X) so
        // a rename still moves the reference with it.
        private string Heading => $"Wave of {enemyCount}";

        private string Status => enemyCount > 5
            ? "That is a lot of enemies."
            : "A manageable wave.";

        private string CountLabel => itemCount == 1
            ? "One item"
            : $"{itemCount} items";
    }
}