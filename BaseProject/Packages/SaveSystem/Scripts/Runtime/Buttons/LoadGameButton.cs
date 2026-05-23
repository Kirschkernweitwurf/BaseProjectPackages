using System.Threading;
using Base.SaveSystemPackage.SaveSlot;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SaveSystemPackage.Buttons
{
    /// <summary>Loads the slot and lets all savables restore themselves.</summary>
    public sealed class LoadGameButton : SaveSlotButtonBase
    {
        protected override async Awaitable OnClickAsync(SaveSlotData saveSlotData, CancellationToken ct)
        {
            bool loaded = await Saves.LoadAsync(saveSlotData.UniqueId, ct);
            CustomLogger.Log(loaded ? $"Loaded '{saveSlotData.DisplayName}'." : "Nothing to load.", this);
        }
    }
}