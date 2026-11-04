using System.Collections.Generic;
using UnityEngine;

namespace Base.ToolsPackage.Editor.AssetZoo.Generation
{
    /// <summary>
    /// Outcome of a single scan. Used to give the artist feedback in the UI.
    /// </summary>
    internal readonly struct ZooGenerationResult
    {
        /// <summary>
        /// True when the scan ran and the zoo has something to show.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Human readable summary or error text. Kept to a single short line.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// What the scan left out and what it decided about prefixes, one item per line. Empty when there is
        /// nothing worth reporting.
        /// </summary>
        public string Details { get; }

        /// <summary>
        /// The assets this scan added, for telling the artist what is new. Never null.
        /// </summary>
        public IReadOnlyList<GameObject> NewAssets { get; }

        /// <summary>
        /// True when <see cref="Details"/> holds something to show.
        /// </summary>
        public bool HasDetails => !string.IsNullOrEmpty(Details);

        /// <summary>Creates a result describing what a scan produced.</summary>
        /// <param name="success">Whether the zoo has something to show.</param>
        /// <param name="message">Single line summary or error text.</param>
        /// <param name="details">Optional report about what was left out.</param>
        /// <param name="newAssets">The assets this scan added, or null for none.</param>
        public ZooGenerationResult(bool success, string message, string details = null,
            IReadOnlyList<GameObject> newAssets = null)
        {
            Success = success;
            Message = message;
            Details = details;
            NewAssets = newAssets ?? new List<GameObject>();
        }

        /// <summary>
        /// Creates a failed result with the given reason.
        /// </summary>
        public static ZooGenerationResult Failed(string message) => new(false, message);
    }
}