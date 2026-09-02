using System;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AudioRules.Data
{
    /// <summary>
    /// Optional. Tells the scanner where a clip's category and loop flag come from, so a rule can
    /// use them as conditions. The Tools package must not reference the package that owns the
    /// containers, so the type and its fields are addressed by name and read through a
    /// <c>SerializedObject</c>. Without a binding everything still works, the category conditions
    /// simply never match.
    /// </summary>
    [Serializable]
    internal sealed class AudioContainerBinding
    {
        private const string DefaultCategoryField = "AudioType";
        private const string DefaultClipsField = "Clips";
        private const string DefaultLoopField = "Loop";
        private const string DefaultTypeName = "AudioContainer";

        [field: Tooltip("Turns the binding off without deleting it.")]
        [field: SerializeField] public bool Enabled { get; set; } = true;

        [field: Tooltip("Class name of the container asset, without a namespace.")]
        [field: SerializeField] public string TypeName { get; set; } = DefaultTypeName;

        [field: Tooltip("Field or property holding the category. An enum is read by its entry name,"
            + " a string by its value.")]
        [field: SerializeField] public string CategoryField { get; set; } = DefaultCategoryField;

        [field: Tooltip("Field or property holding the clips. Works for a single clip and for an array.")]
        [field: SerializeField] public string ClipsField { get; set; } = DefaultClipsField;

        [field: Tooltip("Optional field or property holding the loop flag. Leave empty if there is none.")]
        [field: SerializeField] public string LoopField { get; set; } = DefaultLoopField;

        /// <summary>Creates a binding with the defaults. Needed by the serializer.</summary>
        public AudioContainerBinding() { }

        /// <summary>True when the binding is on and the names it needs are filled in.</summary>
        /// <returns>True when the scanner can use it.</returns>
        public bool IsUsable() => Enabled
            && !string.IsNullOrWhiteSpace(TypeName)
            && !string.IsNullOrWhiteSpace(CategoryField)
            && !string.IsNullOrWhiteSpace(ClipsField);
    }
}