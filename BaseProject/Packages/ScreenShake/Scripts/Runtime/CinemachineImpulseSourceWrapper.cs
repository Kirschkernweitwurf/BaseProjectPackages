using Base.AttributePackage;
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
        [Required] [SerializeField] private ScreenShakeProfile profile;

        [GetComponent] [Required] [SerializeField] private CinemachineImpulseSource source;

#region Unity Callbacks
        private void Awake() => ApplyProfile(profile);

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (profile != null && source != null)
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
            float force = profile.ImpactForce * multiplier;

            if (position.HasValue)
                source.GenerateImpulseAt(position.Value, profile.DefaultVelocity * force);
            else
                source.GenerateImpulseWithForce(force);

            ScreenShakeManager.NotifyShake(profile);
        }

        /// <summary>
        /// Applies the settings from the given <see cref="ScreenShakeProfile"/>
        /// to the <see cref="CinemachineImpulseSource"/>.
        /// </summary>
        private void ApplyProfile(ScreenShakeProfile newProfile)
        {
            if (newProfile == null)
            {
                CustomLogger.LogWarning($"Attempted to apply null {nameof(ScreenShakeProfile)}.", this);
                return;
            }

            CinemachineImpulseDefinition def = source.ImpulseDefinition;
            def.ImpulseDuration = newProfile.ImpulseDuration;
            def.ImpulseType = newProfile.ImpulseType;
            def.ImpulseShape = newProfile.ImpulseShape;
            def.CustomImpulseShape = newProfile.CustomImpulseShape;
            source.ImpulseDefinition = def;

            source.DefaultVelocity = newProfile.DefaultVelocity;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                EditorUtility.SetDirty(source);
#endif
        }

        [ContextMenu("Generate Shake")]
        private void GenerateShakeEditor() => GenerateShake();
    }
}