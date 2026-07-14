using Base.ToolPackage.MenuManagerWindow;
using UnityEngine;

namespace Base.CorePackage.Audio
{
    /// <summary>
    /// ScriptableObject container for audio clips and their properties.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Audio/New AudioContainer", "AudioContainer")]
    public class AudioContainer : ScriptableObject
    {
        /// <summary>
        /// What type of audio this container is for. Relevant for assigning to the correct audio mixer group.
        /// </summary>
        public EAudioType audioType;

        public AudioClip[] clips;
        public float delay;

        /// <summary>
        /// Whether to ignore the pause state of the audio listener, like e.g., UI sounds.
        /// </summary>
        public bool ignorePause;
        public bool loop;
        public bool randomizePitch;

        public float volume = 1;
        public int maxClipsPlaying = -1;
    }
}