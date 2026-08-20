using System.IO;
using Base.ToolPackage.Editor.NamingConventions.Data;
using Base.ToolPackage.Editor.NamingConventions.Renaming;
using Base.UtilityPackage.Logging;
using UnityEditor;

namespace Base.ToolPackage.Editor.NamingConventions.Window
{
    /// <summary>
    /// Every edit the asset naming window defers. The rows request a change while they are drawn,
    /// and <see cref="Apply"/> runs it after the layout pass, which keeps IMGUI from reporting a
    /// mismatch between its layout and its repaint. At most one structural change runs per frame.
    /// </summary>
    internal sealed class AssetNamingEdits
    {
        private AssetNamingHistoryEntry _pendingUndo;
        private AssetNamingViolation _pendingDismiss;
        private AssetNamingViolation _pendingRename;
        private string _pendingRestoreGuid = string.Empty;
        private int _pendingFragmentRemoval = AssetNamingRuleGui.NoIndex;
        private int _pendingRuleRemoval = AssetNamingRuleGui.NoIndex;
        private bool _isAddFragmentPending;
        private bool _isAddRulePending;
        private bool _isClearDismissedPending;
        private bool _isClearHistoryPending;
        private bool _isRenameAllPending;

        /// <summary>Queues a new empty rule.</summary>
        public void RequestAddRule() => _isAddRulePending = true;

        /// <summary>Queues the removal of the rule at the given index.</summary>
        /// <param name="index">Index in the rule set.</param>
        public void RequestRuleRemoval(int index) => _pendingRuleRemoval = index;

        /// <summary>Queues a new empty ignored path fragment.</summary>
        public void RequestAddFragment() => _isAddFragmentPending = true;

        /// <summary>Queues the removal of the ignored path fragment at the given index.</summary>
        /// <param name="index">Index in the fragment list.</param>
        public void RequestFragmentRemoval(int index) => _pendingFragmentRemoval = index;

        /// <summary>Queues taking one asset out of the scan.</summary>
        /// <param name="violation">The row that was dismissed.</param>
        public void RequestDismiss(AssetNamingViolation violation) => _pendingDismiss = violation;

        /// <summary>Queues bringing one dismissed asset back into the scan.</summary>
        /// <param name="guid">GUID of the asset to restore.</param>
        public void RequestRestore(string guid) => _pendingRestoreGuid = guid;

        /// <summary>Queues restoring every dismissed asset.</summary>
        public void RequestClearDismissed() => _isClearDismissedPending = true;

        /// <summary>Queues dropping the whole history.</summary>
        public void RequestClearHistory() => _isClearHistoryPending = true;

        /// <summary>Queues taking one history entry back.</summary>
        /// <param name="entry">The entry to undo.</param>
        public void RequestUndo(AssetNamingHistoryEntry entry) => _pendingUndo = entry;

        /// <summary>Queues the rename of one asset to its current suggestion.</summary>
        /// <param name="violation">The row that was confirmed.</param>
        public void RequestRename(AssetNamingViolation violation) => _pendingRename = violation;

        /// <summary>Queues the rename of every asset in the filtered list.</summary>
        public void RequestRenameAll() => _isRenameAllPending = true;

        /// <summary>
        /// Runs at most one queued change. Rule edits come first because they resize the rule
        /// table, then the dismiss and history stores, then the renames.
        /// </summary>
        /// <param name="ruleSet">Rule set the edits are applied to, also used as the log context.</param>
        /// <param name="query">Query that has to be refreshed when the results change.</param>
        /// <returns>What the window still has to do.</returns>
        public EAssetNamingEditOutcome Apply(AssetNamingRuleSet ruleSet, AssetNamingQuery query)
        {
            if (ApplyRuleEdits(ruleSet))
                return EAssetNamingEditOutcome.None;

            if (TryApplyStoreEdits(query, ruleSet, out EAssetNamingEditOutcome outcome))
                return outcome;

            return ApplyRenames(ruleSet, query);
        }

        private static void Refresh(AssetNamingQuery query)
        {
            query.InvalidateDismissed();
            query.Run();
        }

        private static bool Revert(AssetNamingHistoryEntry entry, AssetNamingRuleSet ruleSet)
        {
            string guid = AssetNamingHistoryStore.GuidOf(entry);

            if (string.IsNullOrEmpty(guid))
            {
                CustomLogger.LogWarning($"Cannot undo, {entry.oldName} is gone.", ruleSet);
                return false;
            }

            if (entry.action == EAssetNamingAction.Renamed)
                return AssetRenamer.RenameTo(AssetDatabase.GUIDToAssetPath(guid), entry.oldName);

            if (entry.action == EAssetNamingAction.Dismissed)
                AssetNamingDismissStore.Restore(guid);
            else
                AssetNamingDismissStore.Dismiss(guid);

            return true;
        }

        private bool ApplyRuleEdits(AssetNamingRuleSet ruleSet)
        {
            if (_isAddRulePending)
            {
                _isAddRulePending = false;
                ruleSet.AddRule(new AssetNamingRule
                {
                    UserCreated = true
                });

                ruleSet.Persist();
                return true;
            }

            if (_pendingRuleRemoval != AssetNamingRuleGui.NoIndex)
            {
                ruleSet.RemoveRuleAt(_pendingRuleRemoval);
                _pendingRuleRemoval = AssetNamingRuleGui.NoIndex;
                ruleSet.Persist();
                return true;
            }

            if (_isAddFragmentPending)
            {
                _isAddFragmentPending = false;
                ruleSet.AddIgnoredFragment(string.Empty);
                ruleSet.Persist();
                return true;
            }

            if (_pendingFragmentRemoval != AssetNamingRuleGui.NoIndex)
            {
                ruleSet.RemoveIgnoredFragmentAt(_pendingFragmentRemoval);
                _pendingFragmentRemoval = AssetNamingRuleGui.NoIndex;
                ruleSet.Persist();
                return true;
            }

            return false;
        }

        private bool TryApplyStoreEdits(AssetNamingQuery query, AssetNamingRuleSet ruleSet,
            out EAssetNamingEditOutcome outcome)
        {
            outcome = EAssetNamingEditOutcome.Repaint;

            if (_pendingDismiss != null)
            {
                AssetNamingDismissStore.Dismiss(_pendingDismiss.Guid);
                AssetNamingHistoryStore.AddDismiss(_pendingDismiss.CurrentName, _pendingDismiss.AssetPath,
                    _pendingDismiss.Guid);

                _pendingDismiss = null;
                Refresh(query);
                return true;
            }

            if (_pendingRestoreGuid.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(_pendingRestoreGuid);

                AssetNamingDismissStore.Restore(_pendingRestoreGuid);
                AssetNamingHistoryStore.AddRestore(Path.GetFileNameWithoutExtension(path), path,
                    _pendingRestoreGuid);

                _pendingRestoreGuid = string.Empty;
                Refresh(query);
                return true;
            }

            if (_isClearDismissedPending)
            {
                _isClearDismissedPending = false;
                AssetNamingDismissStore.Clear();
                Refresh(query);
                return true;
            }

            if (_isClearHistoryPending)
            {
                _isClearHistoryPending = false;
                AssetNamingHistoryStore.Clear();
                return true;
            }

            return TryApplyUndo(query, ruleSet, out outcome);
        }

        /// <summary>
        /// Takes one history entry back. A rename is renamed again, a dismiss is restored and the
        /// other way round. The entry is forgotten afterwards, so the history stays a list of what
        /// is still in effect.
        /// </summary>
        private bool TryApplyUndo(AssetNamingQuery query, AssetNamingRuleSet ruleSet,
            out EAssetNamingEditOutcome outcome)
        {
            outcome = EAssetNamingEditOutcome.None;

            if (_pendingUndo == null)
                return false;

            AssetNamingHistoryEntry entry = _pendingUndo;
            _pendingUndo = null;

            if (!Revert(entry, ruleSet))
                return true;

            AssetNamingHistoryStore.Remove(entry);
            Refresh(query);

            outcome = query.HasScanned
                ? EAssetNamingEditOutcome.Rescan
                : EAssetNamingEditOutcome.Repaint;

            return true;
        }

        private EAssetNamingEditOutcome ApplyRenames(AssetNamingRuleSet ruleSet, AssetNamingQuery query)
        {
            if (_isRenameAllPending)
            {
                _isRenameAllPending = false;
                CustomLogger.Log($"Renamed {AssetRenamer.RenameAll(query.Filtered)} asset(s).", ruleSet);

                return EAssetNamingEditOutcome.Rescan;
            }

            if (_pendingRename == null)
                return EAssetNamingEditOutcome.None;

            AssetNamingViolation violation = _pendingRename;
            _pendingRename = null;

            if (!AssetRenamer.Rename(violation))
                return EAssetNamingEditOutcome.None;

            query.Remove(violation);

            return EAssetNamingEditOutcome.Repaint;
        }
    }
}