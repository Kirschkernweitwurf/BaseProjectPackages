using System.Threading;
using Base.SaveSystemPackage.System;
using UnityEngine;

namespace Base.SaveSystemPackage
{
    /// <summary>
    /// The full save API: read + write. Swap the concrete implementation (e.g. for a console)
    /// without touching callers. Prefer depending on <see cref="ISaveReader"/> or
    /// <see cref="ISaveWriter"/> where you only need one side.
    /// </summary>
    public interface ISaveSystem : ISaveReader, ISaveWriter
    {
        /// <summary>
        /// Wait until any in-flight save/delete has finished. Call this before quitting so a
        /// save started by a menu button is not abandoned mid-write.
        /// </summary>
        Awaitable FlushAsync(CancellationToken ct = default);
    }
}