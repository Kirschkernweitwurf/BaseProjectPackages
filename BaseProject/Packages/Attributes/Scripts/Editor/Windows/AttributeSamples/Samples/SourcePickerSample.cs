using UnityEngine;
using UnityEngine.Audio;

namespace Base.AttributePackage.Editor.Windows.AttributeSamples.Samples
{
    /// <summary>Pickers that read their options from another field on the same object.</summary>
    [AttributeSample("Pickers")]
    internal sealed class SourcePickerSample : ScriptableObject
    {
        [InfoBox("Assign the three sources first. Each picker below reads its options from one of them, "
            + "and says so when the source is empty.")]
        [Tooltip("The animator whose parameters and states feed the two pickers below it.")]
        public Animator animator;

        [Tooltip("The mixer whose exposed parameters and groups feed the two pickers below it.")]
        public AudioMixer mixer;

        [Tooltip("The material whose shader properties and keywords feed the two pickers below it.")]
        public Material material;

        [AnimatorParam(nameof(animator))]
        [Tooltip("Lists the parameters on the assigned animator, so a trigger name cannot be misspelled.")]
        public string animatorParameter;

        [AnimatorState(nameof(animator))]
        [Tooltip("Lists the states on the animator's controller, prefixed by their layer.")]
        public string animatorState;

        [MixerParameter(nameof(mixer))]
        [Tooltip("Lists the parameters exposed on the assigned mixer.")]
        public string mixerParameter;

        [AudioMixerGroup(nameof(mixer))]
        [Tooltip("Restricts a mixer group reference to the groups of one mixer.")]
        public AudioMixerGroup mixerGroup;

        [ShaderParam(nameof(material), EShaderParamType.Color)]
        [Tooltip("Lists the shader properties of one type, so a tint field cannot point at a texture.")]
        public string shaderColor;

        [ShaderKeyword(nameof(material))]
        [Tooltip("Lists the keywords declared by the material's shader.")]
        public string shaderKeyword;

        [ResourcesPath(typeof(Texture2D))]
        [Tooltip("Stores a Resources-relative path chosen from a dropdown, ready for Resources.Load.")]
        public string resourcePath;

        [ComponentPicker]
        [Tooltip("Drop a GameObject and pick which of its components to store, instead of guessing.")]
        public Collider component;

        [OpenAsset("Edit")]
        [Tooltip("Adds a button that opens the referenced asset in whatever editor owns it.")]
        public TextAsset openable;

        [ShowAssetPreview(64)]
        [Tooltip("Draws a thumbnail of the assigned asset under the field.")]
        public Sprite preview;
    }
}