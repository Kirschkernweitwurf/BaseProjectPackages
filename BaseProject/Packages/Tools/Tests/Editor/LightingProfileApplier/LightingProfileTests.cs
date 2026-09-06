using System;
using System.Collections.Generic;
using System.Reflection;
using Base.ToolsPackage.LightingProfileApplier;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// Covers the two halves of a lighting profile against each other. Capture and Apply are twenty
    /// one hand paired assignments each, so a line pasted twice or left out of one of them reads
    /// exactly like its neighbours and shows up only as one lighting setting quietly not travelling
    /// with the profile.
    /// </summary>
    /// <remarks>
    /// These settings are global and belong to the open scene, so the whole set is put back as it was
    /// in the teardown. The scene may be left marked dirty even so, since writing them is what marks
    /// it. The reflection texture is a cubemap rather than a flat one, which is the only kind the
    /// render settings accept.
    /// </remarks>
    public sealed class LightingProfileTests
    {
        private const int CubemapSize = 16;
        private const string FallbackShader = "Sprites/Default";
        private const int FirstBounces = 1;
        private const float FirstNumber = 0.25f;
        private const int FirstResolution = 128;
        private const int SecondBounces = 2;
        private const float SecondNumber = 0.75f;
        private const int SecondResolution = 256;
        private const string UnlitShader = "Unlit/Color";

        private static readonly string[] SettingNames =
        {
            "skybox",
            "ambientMode",
            "ambientSkyColor",
            "ambientEquatorColor",
            "ambientGroundColor",
            "ambientIntensity",
            "subtractiveShadowColor",
            "fog",
            "fogMode",
            "fogColor",
            "fogDensity",
            "fogStartDistance",
            "fogEndDistance",
            "defaultReflectionMode",
            "defaultReflectionResolution",
            "customReflectionTexture",
            "reflectionIntensity",
            "reflectionBounces",
            "haloStrength",
            "flareStrength",
            "flareFadeSpeed"
        };

        private readonly List<Object> _created = new();

        private Dictionary<string, object> _original;

        /// <summary>Remembers the scene's own lighting before anything is written over it.</summary>
        [SetUp]
        public void Prepare() => _original = Snapshot();

        /// <summary>Puts the scene's lighting back and destroys anything the test made.</summary>
        [TearDown]
        public void Cleanup()
        {
            Restore(_original);
            _original = null;

            foreach (Object made in _created)
            {
                if (made != null)
                    Object.DestroyImmediate(made);
            }

            _created.Clear();
        }

        /// <summary>
        /// Every setting the profile stores comes back through a full round trip. A setting that
        /// Capture or Apply forgot stays on the second value instead, and is named in the failure.
        /// </summary>
        [Test]
        public void EverySettingSurvivesARoundTrip()
        {
            LightingProfile profile = CreateProfile();

            Write(second: false);
            Dictionary<string, object> expected = Snapshot();

            profile.Capture();

            Write(second: true);
            profile.Apply();

            Assert.That(Differences(expected), Is.Empty);
        }

        /// <summary>
        /// The second set of values has to actually differ from the first, or the round trip above
        /// would pass without proving anything.
        /// </summary>
        [Test]
        public void TheTwoValueSetsAreTellableApart()
        {
            Write(second: false);
            Dictionary<string, object> first = Snapshot();

            Write(second: true);

            Assert.That(Differences(first), Has.Count.EqualTo(SettingNames.Length));
        }

        /// <summary>
        /// A profile that has never been captured into applies its own defaults rather than throwing,
        /// which is what a freshly created asset does the first time somebody presses the button.
        /// </summary>
        [Test]
        public void AFreshProfileAppliesWithoutComplaint()
            => Assert.That(code: () => CreateProfile().Apply(), Throws.Nothing);

        /// <summary>The number of settings listed here has to match what the profile stores.</summary>
        [Test]
        public void TheProfileStoresExactlyTheSettingsListedHere()
        {
            FieldInfo[] fields = typeof(LightingProfile).GetFields(BindingFlags.Instance
                | BindingFlags.NonPublic
                | BindingFlags.DeclaredOnly);

            Assert.That(fields, Has.Length.EqualTo(SettingNames.Length));
        }

        /// <summary>The names whose current value differs from the given one.</summary>
        private static List<string> Differences(IReadOnlyDictionary<string, object> expected)
        {
            List<string> changed = new();
            Dictionary<string, object> current = Snapshot();

            foreach (string name in SettingNames)
            {
                if (!Equals(current[name], expected[name]))
                    changed.Add(name);
            }

            return changed;
        }

        /// <summary>The current value of every setting the profile stores.</summary>
        private static Dictionary<string, object> Snapshot()
        {
            Dictionary<string, object> values = new();

            foreach (string name in SettingNames)
                values[name] = Property(name).GetValue(null);

            return values;
        }

        /// <summary>Writes the given values back into the render settings.</summary>
        private static void Restore(IReadOnlyDictionary<string, object> values)
        {
            foreach (string name in SettingNames)
                Property(name).SetValue(null, values[name]);
        }

        /// <summary>The static property on the render settings holding the named value.</summary>
        private static PropertyInfo Property(string name)
            => typeof(RenderSettings).GetProperty(name, BindingFlags.Static | BindingFlags.Public);

        /// <summary>
        /// Two integers that survive the clamping Unity puts on them. Bounces tops out low enough that
        /// a resolution sized number would be clamped to the same value on both passes.
        /// </summary>
        private static int NumberFor(string name, bool second)
        {
            if (name.Contains(nameof(RenderSettings.reflectionBounces)))
                return second
                    ? SecondBounces
                    : FirstBounces;

            return second
                ? SecondResolution
                : FirstResolution;
        }

        /// <summary>Any shader at all, since the material is never rendered and only has to exist.</summary>
        private static Shader AnyShader()
        {
            Shader shader = Shader.Find(UnlitShader);

            if (shader == null)
                shader = Shader.Find(FallbackShader);

            return shader;
        }

        /// <summary>A value of the given type, different between the two passes.</summary>
        private object ValueFor(string name, Type type, bool second)
        {
            if (type == typeof(bool))
                return second;

            if (type == typeof(float))
                return second
                    ? SecondNumber
                    : FirstNumber;

            if (type == typeof(int))
                return NumberFor(name, second);

            if (type == typeof(Color))
                return second
                    ? Color.magenta
                    : Color.cyan;

            if (type.IsEnum)
                return Enum.GetValues(type)
                    .GetValue(second
                        ? 1
                        : 0);

            return second
                ? Made(type)
                : null;
        }

        /// <summary>Writes one of the two value sets into every setting the profile stores.</summary>
        private void Write(bool second)
        {
            foreach (string name in SettingNames)
            {
                PropertyInfo property = Property(name);
                property.SetValue(null, ValueFor(name, property.PropertyType, second));
            }
        }

        /// <summary>An asset of the given type, remembered so the teardown can destroy it.</summary>
        private Object Made(Type type)
        {
            Object made = type == typeof(Material)
                ? new Material(AnyShader())
                : new Cubemap(CubemapSize, TextureFormat.RGBA32, false);

            _created.Add(made);

            return made;
        }

        /// <summary>A profile that is never saved, remembered so the teardown can destroy it.</summary>
        private LightingProfile CreateProfile()
        {
            LightingProfile profile = ScriptableObject.CreateInstance<LightingProfile>();
            _created.Add(profile);

            return profile;
        }
    }
}