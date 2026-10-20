using Base.AttributesPackage;
using UnityEngine;

namespace Base.ToolsPackage.LightingProfileApplier
{
    /// <summary>
    /// Applies a lighting profile as soon as the scene holding this component is loaded. While synced, saving
    /// or baking the scene copies its Lighting window into the profile, so the profile never goes stale.
    /// </summary>
    public class LightingProfileApplier : MonoBehaviour
    {
        [Required] [SerializeField] private LightingProfile profile;

        [Tooltip("The directional light used as the sun. Filled from the Lighting window while synced.")]
        [SerializeField] private Light sun;

        [Tooltip("Copies this scene's lighting into the profile on every save and bake. Turn off to apply a "
            + "profile captured in another scene.")]
        [SerializeField] private bool syncWithScene = true;

        /// <summary>The profile applied on load.</summary>
        internal LightingProfile Profile => profile;

        /// <summary>The light set as the sun on load, or null to keep Unity's pick.</summary>
        internal Light Sun => sun;

        /// <summary>True when saving or baking the scene should capture into the profile.</summary>
        internal bool IsSyncedWithScene => syncWithScene;

#region Unity Callbacks
        private void Awake()
        {
            profile.Apply();

            if (sun != null)
                RenderSettings.sun = sun;

            // Light probes of an additively loaded scene are not used until the probe set is rebuilt.
            LightProbes.TetrahedralizeAsync();
        }
#endregion

        /// <summary>Points the applier at a profile and sun, used by the editor sync.</summary>
        internal void Assign(LightingProfile newProfile, Light newSun)
        {
            profile = newProfile;
            sun = newSun;
        }
    }
}