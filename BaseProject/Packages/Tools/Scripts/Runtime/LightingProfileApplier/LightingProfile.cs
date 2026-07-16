using Base.ToolPackage.MenuManagerWindow;
using UnityEngine;
using UnityEngine.Rendering;

namespace Base.ToolPackage.LightingProfileApplier
{
    /// <summary>
    /// Stores the render settings of a scene so they can be applied without making that scene the active one.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Lighting Profile/New Profile", "LP_LightingProfile")]
    public class LightingProfile : ScriptableObject
    {
        [SerializeField] private Material skybox;
        [SerializeField] private AmbientMode ambientMode = AmbientMode.Skybox;
        [SerializeField] private Color ambientSkyColor = Color.gray;
        [SerializeField] private Color ambientEquatorColor = Color.gray;
        [SerializeField] private Color ambientGroundColor = Color.gray;
        [SerializeField] private float ambientIntensity = 1f;
        [SerializeField] private Color subtractiveShadowColor = Color.gray;
        [SerializeField] private bool fog;
        [SerializeField] private FogMode fogMode = FogMode.ExponentialSquared;
        [SerializeField] private Color fogColor = Color.gray;
        [SerializeField] private float fogDensity = 0.01f;
        [SerializeField] private float fogStartDistance;
        [SerializeField] private float fogEndDistance = 300f;
        [SerializeField] private DefaultReflectionMode reflectionMode = DefaultReflectionMode.Skybox;
        [SerializeField] private int reflectionResolution = 128;
        [SerializeField] private Texture customReflection;
        [SerializeField] private float reflectionIntensity = 1f;
        [SerializeField] private int reflectionBounces = 1;
        [SerializeField] private float haloStrength = 0.5f;
        [SerializeField] private float flareStrength = 1f;
        [SerializeField] private float flareFadeSpeed = 3f;

        /// <summary>
        /// Writes the stored values into the global render settings. The active scene is not touched.
        /// </summary>
        public void Apply()
        {
            RenderSettings.skybox = skybox;
            RenderSettings.ambientMode = ambientMode;
            RenderSettings.ambientSkyColor = ambientSkyColor;
            RenderSettings.ambientEquatorColor = ambientEquatorColor;
            RenderSettings.ambientGroundColor = ambientGroundColor;
            RenderSettings.ambientIntensity = ambientIntensity;
            RenderSettings.subtractiveShadowColor = subtractiveShadowColor;
            RenderSettings.fog = fog;
            RenderSettings.fogMode = fogMode;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;
            RenderSettings.defaultReflectionMode = reflectionMode;
            RenderSettings.defaultReflectionResolution = reflectionResolution;
            RenderSettings.customReflectionTexture = customReflection;
            RenderSettings.reflectionIntensity = reflectionIntensity;
            RenderSettings.reflectionBounces = reflectionBounces;
            RenderSettings.haloStrength = haloStrength;
            RenderSettings.flareStrength = flareStrength;
            RenderSettings.flareFadeSpeed = flareFadeSpeed;
            DynamicGI.UpdateEnvironment();
        }

        /// <summary>
        /// Copies the current render settings into this profile.
        /// </summary>
        public void Capture()
        {
            skybox = RenderSettings.skybox;
            ambientMode = RenderSettings.ambientMode;
            ambientSkyColor = RenderSettings.ambientSkyColor;
            ambientEquatorColor = RenderSettings.ambientEquatorColor;
            ambientGroundColor = RenderSettings.ambientGroundColor;
            ambientIntensity = RenderSettings.ambientIntensity;
            subtractiveShadowColor = RenderSettings.subtractiveShadowColor;
            fog = RenderSettings.fog;
            fogMode = RenderSettings.fogMode;
            fogColor = RenderSettings.fogColor;
            fogDensity = RenderSettings.fogDensity;
            fogStartDistance = RenderSettings.fogStartDistance;
            fogEndDistance = RenderSettings.fogEndDistance;
            reflectionMode = RenderSettings.defaultReflectionMode;
            reflectionResolution = RenderSettings.defaultReflectionResolution;
            customReflection = RenderSettings.customReflectionTexture;
            reflectionIntensity = RenderSettings.reflectionIntensity;
            reflectionBounces = RenderSettings.reflectionBounces;
            haloStrength = RenderSettings.haloStrength;
            flareStrength = RenderSettings.flareStrength;
            flareFadeSpeed = RenderSettings.flareFadeSpeed;
        }
    }
}