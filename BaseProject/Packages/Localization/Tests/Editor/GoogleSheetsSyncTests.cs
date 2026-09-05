using System.Text.RegularExpressions;
using Base.LocalizationPackage.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Base.LocalizationPackage.Tests
{
    /// <summary>
    /// The first coverage this package has. A sync overwrites either the sheet or the local tables in
    /// full, so the guards that stop one starting are the difference between a refused click and a
    /// half written collection.
    /// </summary>
    /// <remarks>
    /// Everything past the guards needs a real String Table Collection with a Google Sheets extension
    /// and a live service provider behind it, so the guards and the result they hand back are what is
    /// coverable without committing credentials to the repository.
    /// </remarks>
    public sealed class GoogleSheetsSyncTests
    {
        private const string FailureMessage = "Something went wrong.";
        private const string NullCollectionLog = "collection";

        /// <summary>A result that succeeded carries no message, since there is nothing to report.</summary>
        [Test]
        public void ASuccessCarriesNoMessage()
        {
            SyncResult result = SyncResult.Ok();

            Assert.That(result.Success, Is.True);
            Assert.That(result.Message, Is.Null);
        }

        /// <summary>A failure carries the reason, which is what the window puts in front of the user.</summary>
        [Test]
        public void AFailureCarriesItsReason()
        {
            SyncResult result = SyncResult.Fail(FailureMessage);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo(FailureMessage));
        }

        /// <summary>
        /// A sync without a collection is refused before anything is written, and said out loud rather
        /// than reported as a quiet no op.
        /// </summary>
        [Test]
        public void SyncingNothingIsRefusedAndReported()
        {
            LogAssert.Expect(LogType.Error, new Regex(NullCollectionLog));

            SyncResult result = GoogleSheetsSync.Sync(null, ESyncDirection.Pull);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo(GoogleSheetsSync.MissingCollectionMessage));
        }

        /// <summary>
        /// The guard does not depend on which way the sync would have gone, so a push with nothing to
        /// push is refused the same way a pull is.
        /// </summary>
        [Test]
        public void PushingNothingIsRefusedTheSameWay()
        {
            LogAssert.Expect(LogType.Error, new Regex(NullCollectionLog));

            SyncResult result = GoogleSheetsSync.Sync(null, ESyncDirection.Push);

            Assert.That(result.Message, Is.EqualTo(GoogleSheetsSync.MissingCollectionMessage));
        }
    }
}