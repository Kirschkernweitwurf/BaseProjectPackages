using System.Text.RegularExpressions;
using Base.SaveSystemPackage.Serialization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Base.SaveSystemPackage.Tests
{
    /// <summary>
    /// Covers the conversion of a vector into something Unity's serializer can round trip, and the
    /// refusal to guess when the array coming back has the wrong shape.
    /// </summary>
    public sealed class SerializationUtilitiesTests
    {
        private const float Tolerance = 0.0001f;

        /// <summary>A vector flattens into its three components, in order.</summary>
        [Test]
        public void AVectorFlattensIntoItsComponents()
        {
            float[] values = SerializationUtilities.ToArray(new Vector3(1f, 2f, 3f));

            Assert.That(values.Length, Is.EqualTo(3));
            Assert.That(values[0], Is.EqualTo(1f).Within(Tolerance));
            Assert.That(values[1], Is.EqualTo(2f).Within(Tolerance));
            Assert.That(values[2], Is.EqualTo(3f).Within(Tolerance));
        }

        /// <summary>A flattened vector rebuilds into the vector it came from.</summary>
        [Test]
        public void AFlattenedVectorRebuilds()
        {
            Vector3 original = new(1.5f, -2.5f, 0.25f);

            Assert.That(SerializationUtilities.ToVector3(SerializationUtilities.ToArray(original)),
                Is.EqualTo(original));
        }

        /// <summary>An array of the wrong shape is reported rather than guessed at.</summary>
        [Test]
        public void AnArrayOfTheWrongShapeIsReported()
        {
            LogAssert.Expect(LogType.Warning, new Regex(nameof(Vector3)));

            Assert.That(SerializationUtilities.ToVector3(new[]
            {
                1f,
                2f
            }), Is.EqualTo(Vector3.zero));
        }

        /// <summary>A missing array is reported rather than walked into.</summary>
        [Test]
        public void AMissingArrayIsReported()
        {
            LogAssert.Expect(LogType.Warning, new Regex(nameof(Vector3)));

            Assert.That(SerializationUtilities.ToVector3(null), Is.EqualTo(Vector3.zero));
        }
    }
}