using Base.AttributesPackage;
using Base.UtilityPackage.Collections;
using Base.UtilityPackage.Menus;
using UnityEngine;

namespace Base.CorePackage.Audio
{
    /// <summary>
    /// ScriptableObject container for audio clips and their playback properties.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Audio/New Audio Container", "AUC_AudioContainer")]
    public class AudioContainer : ScriptableObject
    {
        [field: Header("Routing")]

        [field: Tooltip("Category of this container. Decides which mixer group the sound is routed to and which"
            + " pool the audio source is taken from.")]
        [field: SerializeField] public EAudioType AudioType { get; private set; }

        [field: Header("Clips")]

        [field: Tooltip("Clips this container can play. One is picked at random on every play, so adding several"
            + " variations keeps repeated sounds from sounding identical.")]
        [field: NotNullOrEmpty]
        [field: SerializeField] public AudioClip[] Clips { get; private set; }

        [field: Tooltip("Seconds to wait before playback starts. Leave at 0 to play immediately.")]
        [field: Min(0f)]
        [field: SerializeField] public float Delay { get; private set; }

        [field: Header("Playback")]

        [field: Tooltip("Volume multiplier for this container. 1 = full volume, 0 = silent.")]
        [field: MinMax(0f, 1f)]
        [field: SerializeField] public float Volume { get; private set; } = 1f;

        [field: Tooltip("Whether the clip restarts forever until it is stopped or faded out explicitly.")]
        [field: SerializeField] public bool Loop { get; private set; }

        [field: Tooltip("Whether playback continues while the audio listener is paused. Enable this for sounds"
            + " that have to stay audible during a pause, like UI clicks.")]
        [field: SerializeField] public bool IgnorePause { get; private set; }

        [field: Tooltip("Whether to slightly randomize the pitch of the audio source every time it is played."
            + " This can help make repeated sounds feel less repetitive.")]
        [field: SerializeField] public bool RandomizePitch { get; private set; }

        [field: Tooltip("Maximum number of clips from this container playing at the same time. The oldest source"
            + " is released when the limit is reached. Set to -1 for unlimited.")]
        [field: Min(-1)]
        [field: SerializeField] internal int MaxClipsPlaying { get; private set; } = -1;

        /// <summary>
        /// Whether this container is allowed to have any number of clips playing at the same time.
        /// </summary>
        public bool HasUnlimitedClips => MaxClipsPlaying < 0;

        /// <summary>
        /// Picks one of the assigned clips at random. An empty slot in the array is reported by the caller
        /// instead of being skipped here, so the wrong data gets fixed instead of hidden.
        /// </summary>
        /// <returns>A clip to play, or null if the container has none assigned.</returns>
        public AudioClip GetRandomClip() => Clips.GetRandomElement();
    }
}