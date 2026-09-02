using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Base.UtilityPackage.Tests
{
    /// <summary>
    /// Covers the translation from what reflection reports to what a person reads: the arity suffix
    /// goes, generic arguments are spelled out, and a nested type is qualified by the type it sits in.
    /// </summary>
    public sealed class TypeNameUtilityTests
    {
        private const string ArityName = "Pool`1";
        private const string PlainName = "Pool";

        /// <summary>The backtick suffix is not part of the name anything searches by.</summary>
        [Test]
        public void TrimArityDropsTheGenericSuffix()
            => Assert.That(TypeNameUtility.TrimArity(ArityName), Is.EqualTo(PlainName));

        /// <summary>A name without a suffix has to come back untouched.</summary>
        [Test]
        public void TrimArityLeavesAPlainNameAlone()
            => Assert.That(TypeNameUtility.TrimArity(PlainName), Is.EqualTo(PlainName));

        /// <summary>No name in means an empty string out, not a crash.</summary>
        [Test]
        public void TrimArityAnswersEmptyForNoName()
            => Assert.That(TypeNameUtility.TrimArity(null), Is.Empty);

        /// <summary>A generic type reads the way it is written in source.</summary>
        [Test]
        public void AGenericTypeSpellsOutItsArgument()
            => Assert.That(TypeNameUtility.Format(typeof(List<int>)),
                Is.EqualTo($"{nameof(List<int>)}<{nameof(Int32)}>"));

        /// <summary>Two arguments are separated, not run together.</summary>
        [Test]
        public void AGenericTypeSeparatesSeveralArguments()
            => Assert.That(TypeNameUtility.Format(typeof(Dictionary<string, int>)),
                Is.EqualTo($"{nameof(Dictionary<string, int>)}<{nameof(String)}, {nameof(Int32)}>"));

        /// <summary>An array is the element name plus brackets.</summary>
        [Test]
        public void AnArrayKeepsItsBrackets()
            => Assert.That(TypeNameUtility.Format(typeof(int[])), Is.EqualTo($"{nameof(Int32)}[]"));

        /// <summary>A nested type is only findable through the chain it is declared in.</summary>
        [Test]
        public void ANestedTypeIsQualifiedByTheTypesAroundIt()
            => Assert.That(TypeNameUtility.FormatShortName(typeof(Outer.Inner)),
                Is.EqualTo($"{nameof(TypeNameUtilityTests)}.{nameof(Outer)}.{nameof(Outer.Inner)}"));

        /// <summary>The full name carries the namespace in front of the short name.</summary>
        [Test]
        public void AFullNameCarriesTheNamespace()
            => Assert.That(TypeNameUtility.FormatFullName(typeof(TypeNameUtilityTests)),
                Is.EqualTo($"{typeof(TypeNameUtilityTests).Namespace}.{nameof(TypeNameUtilityTests)}"));

        /// <summary>No type in means an empty string out, from every entry point.</summary>
        [Test]
        public void NoTypeAnswersEmpty()
        {
            Assert.That(TypeNameUtility.Format(null), Is.Empty);
            Assert.That(TypeNameUtility.FormatShortName(null), Is.Empty);
            Assert.That(TypeNameUtility.FormatFullName(null), Is.Empty);
        }

        // Nesting is the subject of the test, so the pair has to be declared here rather than in
        // files of their own.
        private sealed class Outer
        {
            internal sealed class Inner { }
        }
    }
}