using Base.ToolPackage.MenuManagerWindow;
using Unity.Cinemachine;
using UnityEngine;

namespace Base.ScreenShakePackage
{
    /// <summary>
    /// ScriptableObject that defines a screen shake profile.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Screen-Shake/New Profile", "ScreenShakeProfile")]
    public class ScreenShakeProfile : ScriptableObject
    {
        [field: Header("Shake Settings")]

        [field: Tooltip("The strength of the shake impact.")]
        [field: SerializeField] public float ImpactForce { get; private set; } = 1f;

        [field: Tooltip("The duration of the impulse effect in seconds.")]
        [field: SerializeField] public float ImpulseDuration { get; private set; } = 0.2f;

        [field: Tooltip("The type of impulse to generate.")]
        [field: SerializeField] public CinemachineImpulseDefinition.ImpulseTypes ImpulseType { get; private set; }

        [field: Tooltip("The shape of the impulse effect.")]
        [field: SerializeField] public CinemachineImpulseDefinition.ImpulseShapes ImpulseShape { get; private set; }

        [field: Tooltip("Custom impulse shape curve defining the shake intensity over time.")]
        [field: SerializeField] public AnimationCurve CustomImpulseShape { get; private set; } =
            AnimationCurve.EaseInOut(0, 0, 1, 1);

        [field: Tooltip("Default velocity direction of the impulse (e.g., downward).")]
        [field: SerializeField] public Vector3 DefaultVelocity { get; private set; } = new(0, -1, 0);

        [field: Header("Listener Settings")]

        [field: Tooltip("How strong the shake appears to the listener.")]
        [field: SerializeField] public float ListenerAmplitude { get; private set; } = 1f;

        [field: Tooltip("The frequency of the shake for the listener (higher = more jittery).")]
        [field: SerializeField] public float ListenerFrequency { get; private set; } = 1f;

        [field: Tooltip("How long the shake lasts for the listener in seconds.")]
        [field: SerializeField] public float ListenerDuration { get; private set; } = 1f;
    }
}