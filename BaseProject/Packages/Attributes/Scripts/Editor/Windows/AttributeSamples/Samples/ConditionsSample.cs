using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples.Samples
{
    /// <summary>Showing, hiding and greying fields based on other fields.</summary>
    [AttributeSample("Conditions")]
    internal sealed class ConditionsSample : ScriptableObject
    {
        /// <summary>Modes the conditional fields below react to.</summary>
        public enum EMode : byte
        {
            /// <summary>Nothing special.</summary>
            Simple = 0,

            /// <summary>The advanced settings apply.</summary>
            Advanced = 1
        }

        [InfoBox("Toggle these two and watch the fields below appear and grey out.")]
        [Tooltip("Drives most of the conditional fields below.")]
        public bool useOverride;

        [Tooltip("The second toggle, so the multi-member conditions have two things to combine.")]
        public bool verbose;

        [ShowIf(nameof(useOverride))] public float overrideValue = 1f;

        [ShowIf(EConditionMode.Any, nameof(useOverride), nameof(verbose))]
        [Tooltip("Visible while either toggle is on, using the explicit Any mode.")]
        public string shownByEither = "Either one is enough";

        [ShowIf(nameof(useOverride), nameof(verbose))]
        [Tooltip("Visible only while both toggles are on, which is what multiple members mean by default.")]
        public string shownByBoth = "Both are needed";

        [EnableIf(nameof(verbose))] public int logDepth = 3;

        [HideIf(nameof(verbose))] public string quietOnly = "Hidden while verbose";

        [Tooltip("Drives the enum-based condition below.")]
        public EMode mode = EMode.Simple;

        [ShowIfEnum(nameof(mode), EMode.Advanced)] public float tolerance = 0.01f;

        [ReadOnly] public string computed = "Never editable";
    }
}