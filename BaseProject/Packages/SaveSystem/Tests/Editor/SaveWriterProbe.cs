using System.Collections.Generic;
using System.Threading;
using Base.SaveSystemPackage.Core;
using Base.SaveSystemPackage.Model;
using Base.UtilityPackage.Async;
using UnityEngine;

namespace Base.SaveSystemPackage.Tests
{
    /// <summary>
    /// A writer that records what it was asked to do instead of touching disk. The slot provider tests
    /// only need it to exist and to report deletions.
    /// </summary>
    internal sealed class SaveWriterProbe : ISaveWriter
    {
        /// <summary>The slots the writer was asked to delete, in order.</summary>
        internal IReadOnlyList<string> Deleted => _deleted;

        private readonly List<string> _deleted = new();

        /// <inheritdoc/>
        public Awaitable SaveAsync(SaveRequest request, CancellationToken ct = default) => AwaitableUtility.Completed();

        /// <inheritdoc/>
        public Awaitable DeleteAsync(string slotId, CancellationToken ct = default)
        {
            _deleted.Add(slotId);

            return AwaitableUtility.Completed();
        }
    }
}