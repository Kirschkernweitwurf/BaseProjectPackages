using System;
using System.Collections.Generic;
using Base.UtilityPackage.Logging;

namespace Base.SaveSystemPackage.Serialization
{
    /// <summary>
    /// The registered <see cref="ISaveMigration"/> steps, keyed by the version each upgrades from, plus
    /// the walk from a stored version up to the current one.
    /// <para>
    /// Split out of the save system so that one only has to decide whether a save needs upgrading, not
    /// how the chain is followed, and so a broken chain can be reported at startup rather than on the
    /// first old save a player happens to load.
    /// </para>
    /// </summary>
    public sealed class SaveMigrationChain
    {
        private readonly Dictionary<int, ISaveMigration> _steps = new();

        /// <summary>Registers the given steps, keeping the first of any two that start at the same version.</summary>
        /// <param name="migrations">The steps to register. Null entries and a null list are skipped.</param>
        public SaveMigrationChain(IEnumerable<ISaveMigration> migrations)
        {
            if (migrations == null)
                return;

            foreach (ISaveMigration migration in migrations)
            {
                if (migration == null)
                    continue;

                if (_steps.TryGetValue(migration.FromVersion, out ISaveMigration existing))
                {
                    CustomLogger.LogError($"Two save migrations start at version {migration.FromVersion}: "
                        + $"'{existing.GetType().Name}' and '{migration.GetType().Name}'. Only the first one "
                        + "runs, so the second is dead code.", null);

                    continue;
                }

                _steps[migration.FromVersion] = migration;
            }
        }

        /// <summary>
        /// Reports steps that can never run and gaps that would make an old save unloadable.
        /// Does nothing when no migrations are registered at all, since a project that never bumped its
        /// save version has nothing to say about.
        /// </summary>
        /// <param name="targetVersion">The version saves are written at now.</param>
        public void Validate(int targetVersion)
        {
            if (_steps.Count == 0)
                return;

            int lowest = targetVersion;

            foreach (int fromVersion in _steps.Keys)
            {
                if (fromVersion >= targetVersion)
                {
                    CustomLogger.LogWarning($"A save migration starts at version {fromVersion}, which is not "
                        + $"below the current save version {targetVersion}, so it never runs.", null);

                    continue;
                }

                lowest = Math.Min(lowest, fromVersion);
            }

            for (int version = lowest; version < targetVersion; version++)
            {
                if (!_steps.ContainsKey(version))
                    CustomLogger.LogWarning($"No save migration from version {version} to {version + 1}. Saves "
                        + $"written at version {version} cannot be loaded.", null);
            }
        }

        /// <summary>
        /// Steps a save up one version at a time until it matches the current version.
        /// </summary>
        /// <param name="slotId">The slot being loaded, for the message when a step is missing.</param>
        /// <param name="states">The id to state map to rewrite in place.</param>
        /// <param name="fromVersion">The version the save was written at.</param>
        /// <param name="targetVersion">The version it has to end up at.</param>
        /// <returns>True when the save now matches the target version.</returns>
        public bool TryMigrate(string slotId, IDictionary<string, string> states, int fromVersion, int targetVersion)
        {
            try
            {
                for (int version = fromVersion; version < targetVersion; version++)
                {
                    if (!_steps.TryGetValue(version, out ISaveMigration step))
                    {
                        CustomLogger.LogError($"No migration from version {version} for slot '{slotId}'. "
                            + "Cannot upgrade save.", null);

                        return false;
                    }

                    step.Migrate(states);
                }

                return true;
            }
            catch (Exception exception)
            {
                CustomLogger.LogError($"Migration failed for slot '{slotId}': {exception.Message}", null);
                return false;
            }
        }
    }
}