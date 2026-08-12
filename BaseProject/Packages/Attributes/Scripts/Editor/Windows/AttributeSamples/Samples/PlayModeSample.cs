using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples.Samples
{
    /// <summary>Fields that behave differently while the editor is playing.</summary>
    [AttributeSample("Conditions")]
    internal sealed class PlayModeSample : ScriptableObject
    {
        [ShowInPlayMode]
        [Tooltip("Only visible while the editor is playing. Enter play mode to see it appear.")]
        public string playModeOnly = "Play mode only";

        [HideInPlayMode]
        [Tooltip("Disappears while the editor is playing, for setup you cannot change once running.")]
        public string editModeOnly = "Edit mode only";

        [EnableInPlayMode]
        [Tooltip("Editable only during play, greyed out otherwise. For values worth tuning live.")]
        public float tunable = 1f;

        [DisableInPlayMode]
        [Tooltip("Locked during play, editable while stopped. The inverse of the field above.")]
        public float locked = 2f;

        [ReadOnlyInPlayMode]
        [Tooltip("The same lock under a name that reads better on a value the game writes itself.")]
        public int readOnlyInPlay = 3;

        [ReadOnlyInEditMode]
        [Tooltip("Locked while stopped, editable during play.")]
        public int readOnlyInEdit = 4;

        [DisableIf(nameof(locked))]
        [Tooltip("Greyed out while another member is true. The inverse of enable-if.")]
        public string disabledByOther = "Disabled by a sibling";
    }
}