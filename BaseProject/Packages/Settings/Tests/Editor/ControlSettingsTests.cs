using Base.SettingsPackage.Controls;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Base.SettingsPackage.Tests
{
    /// <summary>
    /// Covers the values gameplay reads instead of looking settings up by key. The transform applied
    /// to raw look input is the whole point, so both the multiplier and each flipped axis are checked
    /// against a known delta.
    /// </summary>
    public sealed class ControlSettingsTests
    {
        private const float Sensitivity = 2f;
        private const float Tolerance = 0.0001f;

        private GameObject _object;
        private ControlSettings _controls;
        private int _changes;

        /// <summary>Every test starts from a fresh instance with its change event counted.</summary>
        [SetUp]
        public void Build()
        {
            _object = new GameObject(typeof(ControlSettings).Name);
            _controls = _object.AddComponent<ControlSettings>();
            _changes = 0;

            _controls.OnControlsChanged += OnChanged;
        }

        /// <summary>Takes the instance back down so nothing survives into the next test.</summary>
        [TearDown]
        public void Release()
        {
            if (_object != null)
                Object.DestroyImmediate(_object);

            _object = null;
            _controls = null;
        }

        /// <summary>
        /// A scene with no settings menu at all still gets usable values, so look input works before
        /// anything was ever configured.
        /// </summary>
        [Test]
        public void TheValuesAreUsableBeforeAnythingIsConfigured()
        {
            Assert.That(_controls.LookSensitivity, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(_controls.IsHorizontalInverted, Is.False);
            Assert.That(_controls.IsVerticalInverted, Is.False);
        }

        /// <summary>Untouched controls hand the raw input straight through.</summary>
        [Test]
        public void UntouchedControlsPassTheInputThrough()
        {
            Vector2 raw = new(3f, -4f);

            Assert.That(_controls.ApplyLook(raw), Is.EqualTo(raw));
        }

        /// <summary>The sensitivity scales both axes of the look delta.</summary>
        [Test]
        public void TheSensitivityScalesBothAxes()
        {
            _controls.SetLookSensitivity(Sensitivity);

            Vector2 applied = _controls.ApplyLook(new Vector2(3f, -4f));

            Assert.That(applied.x, Is.EqualTo(6f).Within(Tolerance));
            Assert.That(applied.y, Is.EqualTo(-8f).Within(Tolerance));
        }

        /// <summary>Flipping one axis leaves the other alone.</summary>
        [Test]
        public void EachAxisIsFlippedOnItsOwn()
        {
            _controls.SetInverted(ELookAxis.Vertical, true);

            Vector2 applied = _controls.ApplyLook(new Vector2(3f, -4f));

            Assert.That(applied.x, Is.EqualTo(3f).Within(Tolerance));
            Assert.That(applied.y, Is.EqualTo(4f).Within(Tolerance));
        }

        /// <summary>Both axes can be flipped at once.</summary>
        [Test]
        public void BothAxesCanBeFlippedTogether()
        {
            _controls.SetInverted(ELookAxis.Horizontal, true);
            _controls.SetInverted(ELookAxis.Vertical, true);

            Vector2 applied = _controls.ApplyLook(new Vector2(3f, -4f));

            Assert.That(applied.x, Is.EqualTo(-3f).Within(Tolerance));
            Assert.That(applied.y, Is.EqualTo(4f).Within(Tolerance));
        }

        /// <summary>The sensitivity and a flipped axis combine rather than replace each other.</summary>
        [Test]
        public void TheSensitivityAndTheFlipCombine()
        {
            _controls.SetLookSensitivity(Sensitivity);
            _controls.SetInverted(ELookAxis.Horizontal, true);

            Vector2 applied = _controls.ApplyLook(new Vector2(3f, -4f));

            Assert.That(applied.x, Is.EqualTo(-6f).Within(Tolerance));
            Assert.That(applied.y, Is.EqualTo(-8f).Within(Tolerance));
        }

        /// <summary>A new sensitivity is announced once.</summary>
        [Test]
        public void ANewSensitivityIsAnnounced()
        {
            _controls.SetLookSensitivity(Sensitivity);

            Assert.That(_controls.LookSensitivity, Is.EqualTo(Sensitivity).Within(Tolerance));
            Assert.That(_changes, Is.EqualTo(1));
        }

        /// <summary>Setting the sensitivity it already has announces nothing.</summary>
        [Test]
        public void AnUnchangedSensitivityIsNotAnnounced()
        {
            _controls.SetLookSensitivity(_controls.LookSensitivity);

            Assert.That(_changes, Is.EqualTo(0));
        }

        /// <summary>A flipped axis is announced once, per axis.</summary>
        [Test]
        public void AFlippedAxisIsAnnounced()
        {
            _controls.SetInverted(ELookAxis.Horizontal, true);
            _controls.SetInverted(ELookAxis.Vertical, true);

            Assert.That(_controls.IsHorizontalInverted, Is.True);
            Assert.That(_controls.IsVerticalInverted, Is.True);
            Assert.That(_changes, Is.EqualTo(2));
        }

        /// <summary>Setting an axis to what it already is announces nothing.</summary>
        [Test]
        public void AnUnchangedAxisIsNotAnnounced()
        {
            _controls.SetInverted(ELookAxis.Horizontal, false);
            _controls.SetInverted(ELookAxis.Vertical, false);

            Assert.That(_changes, Is.EqualTo(0));
        }

        /// <summary>An axis can be flipped back.</summary>
        [Test]
        public void AnAxisCanBeFlippedBack()
        {
            _controls.SetInverted(ELookAxis.Horizontal, true);
            _controls.SetInverted(ELookAxis.Horizontal, false);

            Assert.That(_controls.IsHorizontalInverted, Is.False);
            Assert.That(_changes, Is.EqualTo(2));
        }

        private void OnChanged() => _changes++;
    }
}