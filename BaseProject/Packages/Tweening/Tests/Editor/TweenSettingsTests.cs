using Base.TweeningPackage.Core.Data;
using NUnit.Framework;

namespace Base.TweeningPackage.Tests
{
    /// <summary>
    /// Covers the per tween settings and the copy that keeps a runtime change from leaking back into
    /// the shared asset it came from. Without that copy, one object tweaking its duration would change
    /// it for every object using the same profile.
    /// </summary>
    public sealed class TweenSettingsTests
    {
        private const float Tolerance = 0.0001f;

        /// <summary>Fresh settings carry a usable duration rather than an instant tween.</summary>
        [Test]
        public void FreshSettingsHaveAUsableDuration()
        {
            TweenSettings settings = new();

            Assert.That(settings.Duration, Is.GreaterThan(0f));
            Assert.That(settings.Delay, Is.EqualTo(0f).Within(Tolerance));
        }

        /// <summary>Settings keep what they were built with.</summary>
        [Test]
        public void SettingsKeepWhatTheyWereBuiltWith()
        {
            TweenSettings settings = new(2f, 0.5f, EEasingType.EaseOutBack);

            Assert.That(settings.Duration, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(settings.Delay, Is.EqualTo(0.5f).Within(Tolerance));
            Assert.That(settings.Easing, Is.EqualTo(EEasingType.EaseOutBack));
        }

        /// <summary>Each value can be changed on its own.</summary>
        [Test]
        public void EachValueCanBeChangedOnItsOwn()
        {
            TweenSettings settings = new();

            settings.SetDuration(3f);
            settings.SetDelay(1f);
            settings.SetEasing(EEasingType.EaseInBounce);

            Assert.That(settings.Duration, Is.EqualTo(3f).Within(Tolerance));
            Assert.That(settings.Delay, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(settings.Easing, Is.EqualTo(EEasingType.EaseInBounce));
        }

        /// <summary>A copy carries the same values.</summary>
        [Test]
        public void ACopyCarriesTheSameValues()
        {
            TweenSettings original = new(2f, 0.5f, EEasingType.EaseOutBack);
            TweenSettings copy = original.Copy();

            Assert.That(copy.Duration, Is.EqualTo(original.Duration).Within(Tolerance));
            Assert.That(copy.Delay, Is.EqualTo(original.Delay).Within(Tolerance));
            Assert.That(copy.Easing, Is.EqualTo(original.Easing));
        }

        /// <summary>A copy is its own object, so changing it leaves the asset alone.</summary>
        [Test]
        public void ACopyIsIndependent()
        {
            TweenSettings original = new(2f, 0.5f, EEasingType.EaseOutBack);
            TweenSettings copy = original.Copy();

            copy.SetDuration(9f);
            copy.SetEasing(EEasingType.Linear);

            Assert.That(original.Duration, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(original.Easing, Is.EqualTo(EEasingType.EaseOutBack));
        }
    }
}