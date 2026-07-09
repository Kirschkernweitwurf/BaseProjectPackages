using System;
using UnityEngine;

namespace Base.AttributePackage.References
{
    /// <summary>
    /// Draws a dropdown of the exposed parameters of a sibling AudioMixer field. Stores the name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class MixerParameterAttribute : PropertyAttribute
    {
        /// <summary>Name of the AudioMixer field on the same object.</summary>
        public string MixerField { get; }

        /// <summary>Creates the attribute referencing the AudioMixer field.</summary>
        public MixerParameterAttribute(string mixerField) => MixerField = mixerField;
    }
}