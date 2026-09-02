using Base.AttributesPackage;
using Base.ServicesPackage;
using Base.ServicesPackage.Tracking;
using Base.UtilityPackage.Menus;
using UnityEngine;

namespace Base.ControllerSupportPackage.Haptics
{
    /// <summary>
    /// Shared haptic asset. Author a rumble once and reference the asset everywhere it plays, so tuning
    /// happens in a single place. For a one-off, build a <see cref="RumblePatternData"/> in code instead
    /// of creating an asset for it.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Input/New Rumble Pattern", "RP_RumblePattern")]
    public sealed class RumblePattern : ScriptableObject
    {
        [Tooltip("The curves and timing this asset represents.")]
        [SerializeField] private RumblePatternData pattern = new();

        /// <summary>The curves and timing this asset represents.</summary>
        public RumblePatternData Pattern => pattern;

        // Curves are guesswork until they are felt on the pad, so tuning happens against a live preview.
        // The asset itself is the caller, so previewing twice restarts instead of stacking.
        [Button("Preview On Gamepad", Mode = EButtonMode.PlayMode)]
        private void Preview()
        {
            if (!ServiceLocator.TryGet(out RumbleService rumbleService))
                return;

            rumbleService.Play(this, this, EPriority.Critical);
        }

        [Button("Stop Preview", Mode = EButtonMode.PlayMode)]
        private void StopPreview()
        {
            if (!ServiceLocator.TryGet(out RumbleService rumbleService))
                return;

            rumbleService.Stop(this);
        }
    }
}