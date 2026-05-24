using System.Threading;
using Base.SaveSystemPackage.Model;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SaveSystemPackage.Unity.Buttons
{
    /// <summary>Loads the assigned slot and lets all savables restore themselves.</summary>
    public sealed class LoadGameButton : SaveSlotButtonBase
    {
        protected override async Awaitable OnClickAsync(CancellationToken ct)
        {
            string slotId = RequireAssignedSlotId();
            if (slotId == null)
                return;

            ESaveLoadResult result = await Saves.LoadAsync(slotId, ct);
            CustomLogger.Log($"Load result for '{slotId}': {result}.", this);
        }
    }
}