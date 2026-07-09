using System;
using UnityEngine;

namespace Base.AttributePackage.References
{
    /// <summary>
    /// Draws an AudioMixerGroup field as a dropdown of the groups of a mixer. The mixer comes from an
    /// optional sibling AudioMixer field, otherwise from the currently assigned group.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class AudioMixerGroupAttribute : PropertyAttribute
    {
        /// <summary>Optional name of an AudioMixer field on the same object.</summary>
        public string MixerField { get; }

        /// <summary>Creates the attribute with an optional AudioMixer field reference.</summary>
        public AudioMixerGroupAttribute(string mixerField = null) => MixerField = mixerField;
    }
}