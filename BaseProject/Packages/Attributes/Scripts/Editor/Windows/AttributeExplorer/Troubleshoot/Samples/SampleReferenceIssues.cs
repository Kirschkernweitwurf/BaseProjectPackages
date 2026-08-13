using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeExplorer.Troubleshoot.Samples
{
    /// <summary>
    /// Polymorphic reference fields the picker cannot serve, so the samples tab can show the two ways a
    /// type picker ends up doing nothing.
    /// </summary>
    [TroubleshootSample]
    internal sealed class SampleReferenceIssues
    {
        /// <summary>Without [SerializeReference] there is no managed reference to pick a type for.</summary>
        [ReferencePicker] public ISampleAbility missingSerializeReference;

        /// <summary>Nothing implements this interface, so the picker opens empty.</summary>
        [SerializeReference] [ReferencePicker] public ISampleUnimplemented emptyPicker;
    }
}