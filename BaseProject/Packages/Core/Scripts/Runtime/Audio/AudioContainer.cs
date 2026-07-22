using Base.AttributePackage;
using Base.ToolPackage.MenuManagerWindow;
using UnityEngine;

namespace Base.CorePackage.Audio
{
    /// <summary>
    /// ScriptableObject container for audio clips and their properties.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Audio/New Audio Container", "AC_AudioContainer")]
    public class AudioContainer : ScriptableObject
    {
        /// <summary>
        /// What type of audio this container is for. Relevant for assigning to the correct audio mixer group.
        /// </summary>
        public EAudioType audioType;

        [NotNullOrEmpty] public AudioClip[] clips;

        [Min(0f)] public float delay;

        /// <summary>
        /// Whether to ignore the pause state of the audio listener, like e.g., UI sounds.
        /// </summary>
        public bool ignorePause;
        public bool loop;
        public bool randomizePitch;

        [MinMax(0f, 1f)] public float volume = 1;

        /// <summary>
        /// Maximum simultaneous clips from this container. Set to -1 for unlimited.
        /// </summary>
        [Min(-1)] public int maxClipsPlaying = -1;
    }
}