using UnityEngine;

namespace Base.ScreenShakePackage
{
    /// <summary>
    /// Base class for generating screen shake effects in response to gameplay events.
    /// </summary>
    [RequireComponent(typeof(CinemachineImpulseSourceWrapper))]
    public abstract class BaseScreenShakeGenerator : MonoBehaviour
    {
        [Tooltip("Multiplier for the screen shake intensity when a card is played.")]
        [SerializeField] protected float multiplier = 1f;

        protected CinemachineImpulseSourceWrapper ImpulseSourceWrapper;

        private void Awake() => ImpulseSourceWrapper = GetComponent<CinemachineImpulseSourceWrapper>();
    }
}