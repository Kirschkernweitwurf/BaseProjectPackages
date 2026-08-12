using System.Collections.Generic;
using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples.Samples
{
    /// <summary>Methods that run when a value changes, and buttons that run on demand.</summary>
    [AttributeSample("Callbacks")]
    internal sealed class CallbacksSample : ScriptableObject
    {
        private const string Actions = "Actions";

        [InfoBox("Edit a field or press a button and the log below records what happened.")]
        [OnValueChanged(nameof(OnSpeedChanged))]
        [Tooltip("Calls a method whenever you edit the field in the inspector.")]
        public float speed = 5f;

        [OnCollectionChanged(nameof(BeforeItemsChanged), nameof(AfterItemsChanged))]
        [Tooltip("Calls one method before the size changes and one after, so what is leaving can be released.")]
        public List<string> items = new();

        [ShowNonSerialized] private string log = "Nothing yet";

        /// <summary>A plain button, drawn under the fields.</summary>
        [Button("Reset", Foldout = Actions)]
        public void ResetSpeed() => Record($"{nameof(ResetSpeed)}");

        /// <summary>Two buttons sharing a row, since they are opposites.</summary>
        [Button("Apply", Row = "applyRevert", Foldout = Actions)]
        public void Apply() => Record(nameof(Apply));

        /// <summary>The other half of that row.</summary>
        [Button("Revert", Row = "applyRevert", Foldout = Actions)]
        public void Revert() => Record(nameof(Revert));

        /// <summary>A button that takes arguments, so a one-off call needs no serialized fields.</summary>
        [Button("Spawn", Size = EButtonSize.Large, Foldout = Actions)]
        public void Spawn(int count = 3, float radius = 5f)
            => Record($"{nameof(Spawn)} {count} within {radius}");

        private void OnSpeedChanged() => Record($"speed is now {speed}");

        // The before half runs while the old contents are still there, which is what a collection that
        // owns something needs in order to release what is leaving.
        private void BeforeItemsChanged(int size) => Record($"before: {size} items");

        private void AfterItemsChanged(int size) => Record($"after: {size} items");

        private void Record(string message) => log = message;
    }
}