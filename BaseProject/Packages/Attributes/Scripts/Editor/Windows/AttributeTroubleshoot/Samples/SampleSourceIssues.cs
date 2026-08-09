using UnityEngine;

namespace Base.AttributePackage.Editor.Windows.AttributeTroubleshoot.Samples
{
    /// <summary>
    /// Deliberately broken option sources, so the samples tab can show what happens when a picker cannot
    /// find the member it reads its options from. These drawers fall back to the plain field.
    /// </summary>
    /// <remarks>
    /// The broken member names are string literals on purpose, matching the state a field ends up in
    /// after the member it pointed at was renamed.
    /// </remarks>
    [TroubleshootSample]
    public sealed class SampleSourceIssues
    {
        /// <summary>A float, used to show a source pointing at the wrong type.</summary>
        public float speed;

        /// <summary>A Transform, used to show an Animator source of the wrong type.</summary>
        public Transform notAnAnimator;

        /// <summary>Reads its options from something that is not enumerable.</summary>
        [Dropdown(nameof(speed))] public string notEnumerable;

        /// <summary>Reads its options from a member that no longer exists.</summary>
        [Dropdown("RenamedOptions")] public string missingOptions;

        /// <summary>Reads its maximum from a member that no longer exists.</summary>
        [ProgressBar("RenamedMaximum")] public float missingMaximum;

        /// <summary>Points at an Animator field that no longer exists.</summary>
        [AnimatorParam("renamedAnimator")] public string missingAnimator;

        /// <summary>Points at a field that exists but is not an Animator.</summary>
        [AnimatorState(nameof(notAnAnimator))] public string wrongAnimatorType;

        /// <summary>Points at an AudioMixer field that no longer exists.</summary>
        [MixerParameter("renamedMixer")] public string missingMixer;

        /// <summary>Points at a field that is neither a Material, a Renderer nor a Shader.</summary>
        [ShaderParam(nameof(speed))] public string wrongShaderSource;
    }
}
