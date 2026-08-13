using UnityEngine;
using UnityEngine.Audio;

namespace Base.AttributePackage.Samples
{
    /// <summary>An exposed parameter picked from a mixer.</summary>
    [AttributeSample(typeof(AudioMixerParameterAttribute), EAttributeCategory.Pickers,
        Description = "Lists the parameters exposed on an assigned AudioMixer, so a volume parameter cannot be named "
            + "wrong.",
        Requirements = "Assign the mixer field first, and the mixer needs at least one exposed parameter.",
        Variations = new[]
        {
            "Nothing to configure beyond the source field."
        })]
    internal sealed class AudioMixerParameterSample : ScriptableObject
    {
        [Tooltip("Assign a mixer here first. The picker below reads from it.")]
        public AudioMixer mixer;

        [AudioMixerParameter(nameof(mixer))]
        [Tooltip("Lists the parameters exposed on the mixer above.")]
        public string mixerParameter;
    }
}