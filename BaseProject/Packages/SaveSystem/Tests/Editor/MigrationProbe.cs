using System.Collections.Generic;
using Base.SaveSystemPackage.Serialization;

namespace Base.SaveSystemPackage.Tests
{
    /// <summary>
    /// One step in a migration chain under test. It records that it ran into a shared log, so a test
    /// can state which steps ran and in which order, and it can be told to fail on purpose.
    /// </summary>
    internal sealed class MigrationProbe : ISaveMigration
    {
        /// <summary>The message the failing variant throws with.</summary>
        internal const string FailureMessage = "The step failed on purpose.";

        /// <inheritdoc/>
        public int FromVersion { get; }

        private readonly List<int> _log;
        private readonly bool _shouldThrow;

        /// <summary>Creates a step.</summary>
        /// <param name="fromVersion">The version this step upgrades from.</param>
        /// <param name="log">The shared list every step reports into.</param>
        /// <param name="shouldThrow">True to make the step fail when it runs.</param>
        internal MigrationProbe(int fromVersion, List<int> log, bool shouldThrow = false)
        {
            FromVersion = fromVersion;
            _log = log;
            _shouldThrow = shouldThrow;
        }

        /// <inheritdoc/>
        public void Migrate(IDictionary<string, string> states)
        {
            if (_shouldThrow)
                throw new KeyNotFoundException(FailureMessage);

            _log.Add(FromVersion);
            states[FromVersion.ToString()] = FromVersion.ToString();
        }
    }
}