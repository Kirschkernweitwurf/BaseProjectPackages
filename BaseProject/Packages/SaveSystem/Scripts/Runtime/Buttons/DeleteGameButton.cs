using System.Threading;
using Base.SaveSystemPackage.SaveSlot;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SaveSystemPackage.Buttons
{
    /// <summary>
    /// Deletes the slot's data, metadata and screenshot.
    /// </summary>
    public sealed class DeleteGameButton : SaveSlotButtonBase
    {
        protected override async Awaitable OnClickAsync(SaveSlotData saveSlotData, CancellationToken ct)
        {
            await Saves.DeleteAsync(saveSlotData.UniqueId, ct);
            CustomLogger.Log($"Deleted '{saveSlotData.DisplayName}'.", this);
        }
    }
}