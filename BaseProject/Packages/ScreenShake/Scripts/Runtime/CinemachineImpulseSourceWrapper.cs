using Base.UtilityPackage.Logging;
using Unity.Cinemachine;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Base.ScreenShakePackage
{
    /// <summary>
    /// Wrapper for <see cref="CinemachineImpulseSource"/> to apply
    /// <see cref="ScreenShakeProfile"/> settings and generate screen shakes.
    /// </summary>
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CinemachineImpulseSourceWrapper : MonoBehaviour
    {
        [SerializeField] private ScreenShakeProfile profile;

        private CinemachineImpulseSource _source;

#region Unity Callbacks
        private void Awake()
        {
            _source = GetComponent<CinemachineImpulseSource>();
            ApplyProfile(profile);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_source == null)
                _source = GetComponent<CinemachineImpulseSource>();

            if (profile != null && _source != null)
                ApplyProfile(profile);
        }
#endif
#endregion

        /// <summary>
        /// Generates a screen shake using the current profile settings.
        /// </summary>
        public void GenerateShake(float multiplier) => GenerateShake(null, multiplier);

        /// <summary>
        /// Generates a screen shake using the current profile settings.
        /// </summary>
        public void GenerateShake(Vector3? position = null, float multiplier = 1f)
        {
            if (!Validate(profile))
                return;

            float force = profile.ImpactForce * multiplier;

            if (position.HasValue)
                _source.GenerateImpulseAt(position.Value, profile.DefaultVelocity * force);
            else
                _source.GenerateImpulseWithForce(force);

            ScreenShakeManager.NotifyShake(profile);
        }

        /// <summary>
        /// Applies the settings from the given <see cref="ScreenShakeProfile"/>
        /// to the <see cref="CinemachineImpulseSource"/>.
        /// </summary>
        private void ApplyProfile(ScreenShakeProfile newProfile)
        {
            if (!Validate(newProfile))
                return;

            CinemachineImpulseDefinition def = _source.ImpulseDefinition;
            def.ImpulseDuration = newProfile.ImpulseDuration;
            def.ImpulseType = newProfile.ImpulseType;
            def.ImpulseShape = newProfile.ImpulseShape;
            def.CustomImpulseShape = newProfile.CustomImpulseShape;
            _source.ImpulseDefinition = def;

            _source.DefaultVelocity = newProfile.DefaultVelocity;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                EditorUtility.SetDirty(_source);
#endif
        }

        /// <summary>
        /// Validates that a profile and the impulse source are present, logging a warning if not.
        /// </summary>
        private bool Validate(ScreenShakeProfile profileToCheck)
        {
            if (profileToCheck == null)
            {
                CustomLogger.LogWarning($"Attempted to apply null {nameof(ScreenShakeProfile)}.", this);
                return false;
            }

            if (_source == null)
            {
                CustomLogger.LogWarning($"{nameof(CinemachineImpulseSource)} component is missing.", this);
                return false;
            }

            return true;
        }

        [ContextMenu("Generate Shake")]
        private void GenerateShakeEditor() => GenerateShake();
    }
}