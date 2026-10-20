using Base.AttributesPackage;
using Base.UtilityPackage.Menus;
using UnityEngine;
using UnityEngine.Rendering;

namespace Base.ToolsPackage.LightingProfileApplier
{
    /// <summary>
    /// Stores the render settings of a scene so they can be applied without making that scene the active one.
    /// Besides the Lighting window values it keeps the baked ambient light and default reflection, because
    /// those live in the scene's lighting data, which Unity only reads for the active scene.
    /// </summary>
    [DynamicCreateAssetMenu("Scriptable Objects/Base/Lighting Profile/New Profile", "LP_LightingProfile")]
    public class LightingProfile : ScriptableObject
    {
        private const int ProbeChannels = 3;
        private const int ProbeCoefficients = 9;

        [SerializeField] private Material skybox;
        [SerializeField] private AmbientMode ambientMode = AmbientMode.Skybox;

        [ShowIfEnum(nameof(ambientMode), AmbientMode.Flat, AmbientMode.Trilight)]
        [SerializeField] private Color ambientSkyColor = Color.gray;

        [ShowIfEnum(nameof(ambientMode), AmbientMode.Trilight)]
        [SerializeField] private Color ambientEquatorColor = Color.gray;

        [ShowIfEnum(nameof(ambientMode), AmbientMode.Trilight)]
        [SerializeField] private Color ambientGroundColor = Color.gray;

        [ShowIfEnum(nameof(ambientMode), AmbientMode.Skybox)]
        [SerializeField] private float ambientIntensity = 1f;

        [SerializeField] private Color subtractiveShadowColor = Color.gray;
        [SerializeField] private bool fog;

        [ShowIf(nameof(fog))]
        [SerializeField] private FogMode fogMode = FogMode.ExponentialSquared;

        [ShowIf(nameof(fog))]
        [SerializeField] private Color fogColor = Color.gray;

        [ShowIf(nameof(fog))]
        [ShowIfEnum(nameof(fogMode), FogMode.Exponential, FogMode.ExponentialSquared)]
        [SerializeField] private float fogDensity = 0.01f;

        [ShowIf(nameof(fog))]
        [ShowIfEnum(nameof(fogMode), FogMode.Linear)]
        [SerializeField] private float fogStartDistance;

        [ShowIf(nameof(fog))]
        [ShowIfEnum(nameof(fogMode), FogMode.Linear)]
        [SerializeField] private float fogEndDistance = 300f;

        [SerializeField] private DefaultReflectionMode reflectionMode = DefaultReflectionMode.Skybox;

        [ShowIfEnum(nameof(reflectionMode), DefaultReflectionMode.Skybox)]
        [SerializeField] private int reflectionResolution = 128;

        [ShowIfEnum(nameof(reflectionMode), DefaultReflectionMode.Custom)]
        [SerializeField] private Texture customReflection;
        [SerializeField] private float reflectionIntensity = 1f;
        [SerializeField] private int reflectionBounces = 1;
        [SerializeField] private float haloStrength = 0.5f;
        [SerializeField] private float flareStrength = 1f;
        [SerializeField] private float flareFadeSpeed = 3f;

        [Tooltip("The default reflection the scene showed when it was captured. Empty means none was baked.")]
        [SerializeField] private Texture environmentReflection;

        [HideInInspector] [SerializeField] private float[] ambientProbe;
        [HideInInspector] [SerializeField] private string sceneGuid;

        /// <summary>
        /// The GUID of the scene this profile was captured from. Empty until the first capture, so a scene
        /// copied together with its applier can tell that the profile belongs to the original.
        /// </summary>
        internal string SceneGuid
        {
            get => sceneGuid;
            set => sceneGuid = value;
        }

        /// <summary>The skybox material of the captured scene.</summary>
        internal Material Skybox => skybox;

        /// <summary>Where the captured scene took its default reflection from.</summary>
        internal DefaultReflectionMode ReflectionMode => reflectionMode;

        /// <summary>The captured default reflection, or null when the scene had none baked.</summary>
        internal Texture EnvironmentReflection => environmentReflection;

        /// <summary>True when the baked environment was captured. Older profiles only hold the settings.</summary>
        internal bool HasAmbientProbe => ambientProbe != null
            && ambientProbe.Length == ProbeChannels * ProbeCoefficients;

        /// <summary>
        /// Writes the stored values into the global render settings. The active scene is not touched.
        /// A captured environment replaces the active scene's baked ambient light and reflection, so the
        /// result looks exactly like the captured scene did on its own.
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

            if (!HasAmbientProbe)
            {
                DynamicGI.UpdateEnvironment();
                return;
            }

            // A recalculation would overwrite the baked values, so they are written last and nothing
            // regenerates the environment afterwards.
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.customReflectionTexture = environmentReflection;
            RenderSettings.ambientProbe = ReadProbe();
        }

        /// <summary>
        /// Copies the current render settings and the active scene's baked environment into this profile.
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
            environmentReflection = ReflectionProbe.defaultTexture;
            WriteProbe(RenderSettings.ambientProbe);
        }

        /// <summary>
        /// Drops a captured reflection that is not an asset, like one Unity generated on the fly. A reference
        /// to it would not survive saving the profile.
        /// </summary>
        internal void ForgetEnvironmentReflection() => environmentReflection = null;

        private void WriteProbe(SphericalHarmonicsL2 probe)
        {
            ambientProbe = new float[ProbeChannels * ProbeCoefficients];

            for (int channel = 0; channel < ProbeChannels; channel++)
            {
                for (int coefficient = 0; coefficient < ProbeCoefficients; coefficient++)
                    ambientProbe[channel * ProbeCoefficients + coefficient] = probe[channel, coefficient];
            }
        }

        private SphericalHarmonicsL2 ReadProbe()
        {
            SphericalHarmonicsL2 probe = new();

            for (int channel = 0; channel < ProbeChannels; channel++)
            {
                for (int coefficient = 0; coefficient < ProbeCoefficients; coefficient++)
                    probe[channel, coefficient] = ambientProbe[channel * ProbeCoefficients + coefficient];
            }

            return probe;
        }
    }
}