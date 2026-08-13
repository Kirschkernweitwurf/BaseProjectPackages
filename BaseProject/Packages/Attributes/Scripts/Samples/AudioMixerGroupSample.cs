using UnityEngine;
using UnityEngine.Audio;

namespace Base.AttributePackage.Samples
{
    /// <summary>A mixer group restricted to one mixer.</summary>
    [AttributeSample(typeof(AudioMixerGroupAttribute), EAttributeCategory.Pickers,
        Description = "Restricts a mixer group reference to the groups of one mixer, so a group from an unrelated "
            + "mixer cannot be assigned.",
        Requirements = "Assign the mixer field first.",
        Variations = new[]
        {
            "Left without a source field, every group in the project is offered."
        })]
    internal sealed class AudioMixerGroupSample : ScriptableObject
    {
        [Tooltip("Assign a mixer here first. The picker below reads from it.")]
        public AudioMixer mixer;

        [AudioMixerGroup(nameof(mixer))]
        [Tooltip("Only the groups of the mixer above are offered.")]
        public AudioMixerGroup mixerGroup;
    }
}