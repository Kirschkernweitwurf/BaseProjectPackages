using System;
using Base.TweeningPackage.Core;
using NUnit.Framework;
using UnityEngine;

namespace Base.TweeningPackage.Tests
{
    /// <summary>
    /// Covers the interpolation a tween drives its value through. It is deliberately unclamped, so an
    /// easing that overshoots actually overshoots instead of being flattened at the target.
    /// </summary>
    public sealed class TweenLerpUtilityTests
    {
        private const float Tolerance = 0.0001f;

        /// <summary>A float lands on each end at the ends and halfway in the middle.</summary>
        [Test]
        public void AFloatInterpolatesBetweenItsEnds()
        {
            Assert.That(TweenLerpUtility.LerpFloatUnclamped(2f, 6f, 0f), Is.EqualTo(2f).Within(Tolerance));
            Assert.That(TweenLerpUtility.LerpFloatUnclamped(2f, 6f, 0.5f), Is.EqualTo(4f).Within(Tolerance));
            Assert.That(TweenLerpUtility.LerpFloatUnclamped(2f, 6f, 1f), Is.EqualTo(6f).Within(Tolerance));
        }

        /// <summary>A value past the end keeps going, which is what makes an overshoot visible.</summary>
        [Test]
        public void AFloatOvershootsPastItsEnd()
        {
            Assert.That(TweenLerpUtility.LerpFloatUnclamped(0f, 10f, 1.5f), Is.EqualTo(15f).Within(Tolerance));
            Assert.That(TweenLerpUtility.LerpFloatUnclamped(0f, 10f, -0.5f), Is.EqualTo(-5f).Within(Tolerance));
        }

        /// <summary>A two component vector interpolates on both components.</summary>
        [Test]
        public void ATwoComponentVectorInterpolates()
        {
            Vector2 halfway = TweenLerpUtility.LerpVector2Unclamped(Vector2.zero, new Vector2(4f, 8f), 0.5f);

            Assert.That(halfway.x, Is.EqualTo(2f).Within(Tolerance));
            Assert.That(halfway.y, Is.EqualTo(4f).Within(Tolerance));
        }

        /// <summary>A three component vector interpolates on all three components.</summary>
        [Test]
        public void AThreeComponentVectorInterpolates()
        {
            Vector3 halfway = TweenLerpUtility.LerpVector3Unclamped(Vector3.zero, new Vector3(2f, 4f, 6f), 0.5f);

            Assert.That(halfway, Is.EqualTo(new Vector3(1f, 2f, 3f)));
        }

        /// <summary>A vector overshoots past its end as well.</summary>
        [Test]
        public void AVectorOvershootsPastItsEnd()
        {
            Vector3 past = TweenLerpUtility.LerpVector3Unclamped(Vector3.zero, Vector3.one, 2f);

            Assert.That(past, Is.EqualTo(new Vector3(2f, 2f, 2f)));
        }

        /// <summary>A color interpolates on every channel, alpha included.</summary>
        [Test]
        public void AColorInterpolatesOnEveryChannel()
        {
            Color halfway = TweenLerpUtility.LerpColorUnclamped(Color.clear, Color.white, 0.5f);

            Assert.That(halfway.r, Is.EqualTo(0.5f).Within(Tolerance));
            Assert.That(halfway.g, Is.EqualTo(0.5f).Within(Tolerance));
            Assert.That(halfway.b, Is.EqualTo(0.5f).Within(Tolerance));
            Assert.That(halfway.a, Is.EqualTo(0.5f).Within(Tolerance));
        }

        /// <summary>A rotation lands on each end at the ends.</summary>
        [Test]
        public void ARotationInterpolatesBetweenItsEnds()
        {
            Quaternion from = Quaternion.identity;
            Quaternion to = Quaternion.Euler(0f, 90f, 0f);

            Assert.That(Quaternion.Angle(TweenLerpUtility.LerpQuaternionUnclamped(from, to, 0f), from),
                Is.EqualTo(0f).Within(0.01f));

            Assert.That(Quaternion.Angle(TweenLerpUtility.LerpQuaternionUnclamped(from, to, 1f), to),
                Is.EqualTo(0f).Within(0.01f));
        }

        /// <summary>Every supported type resolves to the function that handles it.</summary>
        [Test]
        public void EverySupportedTypeResolves()
        {
            Assert.That(TweenLerpUtility.Resolve<float>(), Is.Not.Null);
            Assert.That(TweenLerpUtility.Resolve<Vector2>(), Is.Not.Null);
            Assert.That(TweenLerpUtility.Resolve<Vector3>(), Is.Not.Null);
            Assert.That(TweenLerpUtility.Resolve<Color>(), Is.Not.Null);
            Assert.That(TweenLerpUtility.Resolve<Quaternion>(), Is.Not.Null);
        }

        /// <summary>The resolved function is the one that type is interpolated with.</summary>
        [Test]
        public void TheResolvedFunctionInterpolatesThatType()
        {
            Func<float, float, float, float> lerp = TweenLerpUtility.Resolve<float>();

            Assert.That(lerp(2f, 6f, 0.5f), Is.EqualTo(4f).Within(Tolerance));
        }

        /// <summary>
        /// A type nobody wrote an interpolation for answers with nothing, so the caller can report it
        /// rather than run into a missing function later.
        /// </summary>
        [Test]
        public void AnUnsupportedTypeResolvesToNothing()
        {
            Assert.That(TweenLerpUtility.Resolve<int>(), Is.Null);
            Assert.That(TweenLerpUtility.Resolve<string>(), Is.Null);
        }
    }
}