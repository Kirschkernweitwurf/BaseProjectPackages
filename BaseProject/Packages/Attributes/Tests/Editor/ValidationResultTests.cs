using NUnit.Framework;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Covers the result a validator hands back. It exists so a check can say which failure it ran
    /// into rather than only that something failed, and so a value that still works can be downgraded
    /// to a warning instead of blocking on an error.
    /// </summary>
    public sealed class ValidationResultTests
    {
        private const string Message = "the texture is unreadable";

        /// <summary>A passing result has nothing to draw and nothing to say.</summary>
        [Test]
        public void APassingResultHasNothingToSay()
        {
            Assert.That(ValidationResult.Valid.IsValid, Is.True);
            Assert.That(ValidationResult.Valid.Severity, Is.EqualTo(EValidationSeverity.Valid));
            Assert.That(ValidationResult.Valid.Message, Is.Null);
        }

        /// <summary>A warning carries its own words and does not pass.</summary>
        [Test]
        public void AWarningCarriesItsOwnWords()
        {
            ValidationResult result = ValidationResult.Warning(Message);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Severity, Is.EqualTo(EValidationSeverity.Warning));
            Assert.That(result.Message, Is.EqualTo(Message));
        }

        /// <summary>An error carries its own words and does not pass either.</summary>
        [Test]
        public void AnErrorCarriesItsOwnWords()
        {
            ValidationResult result = ValidationResult.Error(Message);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Severity, Is.EqualTo(EValidationSeverity.Error));
            Assert.That(result.Message, Is.EqualTo(Message));
        }

        /// <summary>A warning and an error are told apart, so they can be drawn differently.</summary>
        [Test]
        public void AWarningAndAnErrorAreToldApart() => Assert.That(ValidationResult.Error(Message).Severity,
            Is.Not.EqualTo(ValidationResult.Warning(Message).Severity));

        /// <summary>
        /// A result without words falls back to whatever the attribute says, so a validator may leave
        /// the wording to the field it sits on.
        /// </summary>
        [Test]
        public void AResultWithoutWordsLeavesTheWordingToTheAttribute()
            => Assert.That(ValidationResult.Error(null).Message, Is.Null);

        /// <summary>An untouched result passes, so a default value never blocks anything.</summary>
        [Test]
        public void AnUntouchedResultPasses() => Assert.That(default(ValidationResult).IsValid, Is.True);
    }
}