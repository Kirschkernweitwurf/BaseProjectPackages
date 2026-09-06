using Base.UIPackage.Confirmation;
using NUnit.Framework;

namespace Base.UIPackage.Tests
{
    /// <summary>
    /// The optional half of a confirmation prompt. A caller that leaves a button label out gets the
    /// menu's own wording, and one that builds a label from a value it turned out not to have gets the
    /// same, rather than a button with nothing written on it.
    /// </summary>
    public sealed class ConfirmationRequestTests
    {
        private const string CancelFallback = "Cancel";
        private const string ConfirmFallback = "Confirm";
        private const string GivenCancel = "Back";
        private const string GivenConfirm = "Delete";
        private const string Message = "Delete this save?";
        private const string Whitespace = " ";

        /// <summary>The message is the one thing a request always carries.</summary>
        [Test]
        public void ARequestKeepsTheMessageItWasGiven()
            => Assert.That(new ConfirmationRequest(Message).Message, Is.EqualTo(Message));

        /// <summary>
        /// A label left out stays null rather than becoming an empty string, since that is the state the
        /// fallback reads.
        /// </summary>
        [Test]
        public void AnOmittedLabelIsLeftUnset()
        {
            ConfirmationRequest request = new(Message);

            Assert.That(request.ConfirmText, Is.Null);
            Assert.That(request.CancelText, Is.Null);
        }

        /// <summary>A label the caller named is the one shown, which is the whole point of naming it.</summary>
        [Test]
        public void AGivenLabelIsTheOneShown()
        {
            ConfirmationRequest request = new(Message, GivenConfirm, GivenCancel);

            Assert.That(request.ResolveConfirmText(ConfirmFallback), Is.EqualTo(GivenConfirm));
            Assert.That(request.ResolveCancelText(CancelFallback), Is.EqualTo(GivenCancel));
        }

        /// <summary>
        /// Nothing and an empty string read the same. A caller that built the label from a value it did
        /// not have arrives here just as often as one that left the argument out.
        /// </summary>
        /// <param name="given">The label the caller passed.</param>
        [TestCase(null)]
        [TestCase("")]
        public void AMissingConfirmLabelFallsBack(string given)
        {
            ConfirmationRequest request = new(Message, given);

            Assert.That(request.ResolveConfirmText(ConfirmFallback), Is.EqualTo(ConfirmFallback));
        }

        /// <summary>The cancel side reads its own label, not the confirm one.</summary>
        /// <param name="given">The label the caller passed.</param>
        [TestCase(null)]
        [TestCase("")]
        public void AMissingCancelLabelFallsBack(string given)
        {
            ConfirmationRequest request = new(Message, GivenConfirm, given);

            Assert.That(request.ResolveCancelText(CancelFallback), Is.EqualTo(CancelFallback));
        }

        /// <summary>
        /// A label of spaces is a label. It draws as a blank button, which is a wiring mistake worth
        /// seeing rather than one the fallback quietly covers up.
        /// </summary>
        [Test]
        public void ALabelOfSpacesIsKept()
        {
            ConfirmationRequest request = new(Message, Whitespace, Whitespace);

            Assert.That(request.ResolveConfirmText(ConfirmFallback), Is.EqualTo(Whitespace));
            Assert.That(request.ResolveCancelText(CancelFallback), Is.EqualTo(Whitespace));
        }

        /// <summary>Each side falls back on its own, so one given label does not cover for the other.</summary>
        [Test]
        public void OneGivenLabelDoesNotCoverForTheOther()
        {
            ConfirmationRequest request = new(Message, GivenConfirm);

            Assert.That(request.ResolveConfirmText(ConfirmFallback), Is.EqualTo(GivenConfirm));
            Assert.That(request.ResolveCancelText(CancelFallback), Is.EqualTo(CancelFallback));
        }
    }
}