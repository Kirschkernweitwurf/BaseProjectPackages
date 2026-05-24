using System.Threading;
using Base.SaveSystemPackage.Model;
using Base.SaveSystemPackage.Savable;
using UnityEngine;

namespace Base.SaveSystemPackage.System
{
    /// <summary>
    /// Write side of the save system. Gameplay code that only saves/deletes can depend on just this.
    /// Data is not passed in: every object that wants saving implements <see cref="ISavable"/> and registers;
    /// on save the system collects their state.
    /// </summary>
    public interface ISaveWriter
    {
        /// <summary>Collect state from all registered savables and write it to the request's slot.</summary>
        Awaitable SaveAsync(SaveRequest request, CancellationToken ct = default);

        /// <summary>Delete all files for this slot. Does nothing if the slot is empty.</summary>
        Awaitable DeleteAsync(string slotId, CancellationToken ct = default);
    }
}