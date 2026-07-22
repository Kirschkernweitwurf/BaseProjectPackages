using Base.AttributePackage;
using Base.UtilityPackage.Logging;
using Unity.Cinemachine;
using UnityEngine;

namespace Base.ScreenShakePackage
{
    /// <summary>
    /// Applies settings from a <see cref="ScreenShakeProfile"/>
    /// to a <see cref="CinemachineImpulseListener"/> component.
    /// </summary>
    [RequireComponent(typeof(CinemachineImpulseListener))]
    public class CinemachineImpulseListenerSettingsApplier : MonoBehaviour
    {
        [GetComponent] [Required]
        [SerializeField] private CinemachineImpulseListener listener;

#region Unity Callbacks
        private void Awake() => ScreenShakeManager.RegisterListener(ApplyProfile);

        private void OnDestroy() => ScreenShakeManager.DeregisterListener(ApplyProfile);
#endregion

        /// <summary>
        /// Applies the settings from the given <see cref="ScreenShakeProfile"/>
        /// to the <see cref="CinemachineImpulseListener"/>.
        /// </summary>
        private void ApplyProfile(ScreenShakeProfile profile)
        {
            if (profile == null)
            {
                CustomLogger.LogWarning($"Attempted to apply null {nameof(ScreenShakeProfile)}.", this);
                return;
            }

            CinemachineImpulseListener.ImpulseReaction reaction = listener.ReactionSettings;
            reaction.AmplitudeGain = profile.ListenerAmplitude;
            reaction.FrequencyGain = profile.ListenerFrequency;
            reaction.Duration = profile.ListenerDuration;
            listener.ReactionSettings = reaction;
        }
    }
}