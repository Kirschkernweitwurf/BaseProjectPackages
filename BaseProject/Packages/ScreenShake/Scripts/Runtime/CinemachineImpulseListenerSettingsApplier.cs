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
        private CinemachineImpulseListener _listener;

#region Unity Callbacks
        private void Awake()
        {
            _listener = GetComponent<CinemachineImpulseListener>();

            ScreenShakeManager.RegisterListener(ApplyProfile);
        }

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
                CustomLogger.LogWarning("Attempted to apply null ScreenShakeProfile.", this);
                return;
            }

            _listener.ReactionSettings = new CinemachineImpulseListener.ImpulseReaction
            {
                AmplitudeGain = profile.ListenerAmplitude,
                FrequencyGain = profile.ListenerFrequency,
                Duration = profile.ListenerDuration
            };
        }
    }
}