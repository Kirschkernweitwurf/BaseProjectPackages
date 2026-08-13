using System;
using UnityEngine;

namespace Base.AttributePackage.Samples
{
    /// <summary>A member Unity does not serialize, shown anyway.</summary>
    [AttributeSample(typeof(ShowNonSerializedAttribute), EAttributeCategory.Callbacks,
        Description = "Shows a member Unity would never serialize, for runtime state that would otherwise "
            + "be invisible without attaching a debugger. Read-only by nature: there is no serialized "
            + "value behind it to write to.",
        Requirements = "Press the button to change the state below and watch both rows follow. Nothing "
            + "here survives a domain reload, which is the point.",
        Info = "The value is read by reflection on every repaint, so it should be a plain field rather "
            + "than something expensive to look at.",
        Variations = new[]
        {
            "Works on a private field.",
            "Works on a constant, which has no serialized value either."
        })]
    internal sealed class ShowNonSerializedSample : ScriptableObject
    {
        [ShowNonSerialized]
        [Tooltip("Runtime state. Not serialized, so it is shown rather than edited.")]
        private string lastEvent = "Nothing yet";

        [ShowNonSerialized]
        [Tooltip("A constant, which has no serialized value either.")]
        private const int Limit = 42;

        [NonSerialized] private int _count;

        /// <summary>Changes the state the rows above report.</summary>
        [Button("Record an event")]
        public void Record()
        {
            _count++;
            lastEvent = $"event {_count} of at most {Limit}";
        }
    }
}