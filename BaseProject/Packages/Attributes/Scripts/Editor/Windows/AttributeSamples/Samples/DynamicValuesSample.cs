using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples.Samples
{
    /// <summary>Titles, boxes and labels computed from a member instead of typed.</summary>
    [AttributeSample("Layout")]
    internal sealed class DynamicValuesSample : ScriptableObject
    {
        [Title("$" + nameof(Heading), EColor.Purple)]
        [Tooltip("The heading above this field is not typed out. It names a property, and shows whatever "
            + "that property returns. Change the number and the heading changes with it.")]
        public int enemyCount = 3;

        [InfoBox("$" + nameof(Status))]
        [Tooltip("The box above works the same way: it reads a property instead of a fixed sentence, so "
            + "it can comment on the values around it.")]
        public string readsAProperty = "See the box above.";

        [Label("$" + nameof(CountLabel))]
        [Tooltip("Replaces the label of this field. Here it reads a property, so the label can say what "
            + "the number currently means instead of repeating the field name.")]
        public int itemCount = 1;

        [Label("Renamed in the inspector only")]
        [Tooltip("A plain literal label, for a field whose good code name and good inspector name differ.")]
        public string internalName = "The field is still called internalName";

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