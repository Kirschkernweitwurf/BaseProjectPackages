using System;
using UnityEngine;

namespace Base.AttributePackage
{
    /// <summary>
    /// Draws a dropdown of the exposed parameters of a sibling AudioMixer field. Stores the name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class AudioMixerParameterAttribute : PropertyAttribute
    {
        /// <summary>Name of the AudioMixer field on the same object.</summary>
        public string MixerField { get; }

        /// <summary>Creates the attribute referencing the AudioMixer field.</summary>
        public AudioMixerParameterAttribute(string mixerField) => MixerField = mixerField;
    }
}