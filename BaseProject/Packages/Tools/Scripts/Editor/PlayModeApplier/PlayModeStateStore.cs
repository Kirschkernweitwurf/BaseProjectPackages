using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base.ToolPackage.Editor.PlayModeApplier
{
    /// <summary>
    /// Persists captured payloads to disk so they survive the domain reload that happens when play mode ends.
    /// Marks are not stored here, they live in <see cref="PlayModeMarks"/> and are meant to die with the session.
    /// </summary>
    [FilePath(StoreFilePath, FilePathAttribute.Location.ProjectFolder)]
    public class PlayModeStateStore : ScriptableSingleton<PlayModeStateStore>
    {
        private const int MaximumHistoryEntries = 200;
        private const string StoreFilePath = "UserSettings/PlayModeSaver.asset";

        [SerializeField]
        private List<PlayModeSavePayload> payloads = new();

        [SerializeField]
        private List<PlayModeHistoryEntry> history = new();

        /// <summary>State captured at the end of the last play session, waiting for a manual apply.</summary>
        public IReadOnlyList<PlayModeSavePayload> Payloads => payloads;

        /// <summary>What happened since the last play session, oldest first.</summary>
        public IReadOnlyList<PlayModeHistoryEntry> History => history;

        /// <summary>Replaces the pending payloads with the given set.</summary>
        public void SetPayloads(List<PlayModeSavePayload> capturedPayloads)
        {
            payloads.Clear();
            payloads.AddRange(capturedPayloads);
        }

        /// <summary>Removes one pending payload by list index.</summary>
        public void RemovePayload(int index)
        {
            if (index < 0
                || index >= payloads.Count)
                return;

            payloads.RemoveAt(index);
        }

        /// <summary>Changes where a pending payload is written on apply.</summary>
        public void SetPayloadApplyTarget(int index, EPlayModeApplyTarget applyTarget)
        {
            if (index < 0
                || index >= payloads.Count)
                return;

            payloads[index].applyTarget = applyTarget;
        }

        /// <summary>Points a pending payload at a different destination prefab.</summary>
        public void SetPayloadPrefab(int index, string prefabGuid)
        {
            if (index < 0
                || index >= payloads.Count)
                return;

            payloads[index].sourcePrefabGuid = prefabGuid;
        }

        /// <summary>Drops every pending payload.</summary>
        public void ClearPayloads() => payloads.Clear();

        /// <summary>Appends one history entry, dropping the oldest once the cap is reached.</summary>
        public void AddHistoryEntry(PlayModeHistoryEntry entry)
        {
            history.Add(entry);

            if (history.Count > MaximumHistoryEntries)
                history.RemoveAt(0);
        }

        /// <summary>Drops the whole history.</summary>
        public void ClearHistory() => history.Clear();

        /// <summary>Writes the store to disk. Required for anything to survive a domain reload.</summary>
        public void Persist() => Save(true);
    }
}