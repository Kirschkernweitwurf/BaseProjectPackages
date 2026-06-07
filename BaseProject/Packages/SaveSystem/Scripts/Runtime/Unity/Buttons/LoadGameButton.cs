using System.Threading;
using Base.SaveSystemPackage.Model;
using UnityEngine;

namespace Base.SaveSystemPackage.Unity.Buttons
{
    /// <summary>Loads the selected slot and lets all savables restore themselves.</summary>
    public sealed class LoadGameButton : SaveSlotButtonBase
    {
        protected override async Awaitable OnClickAsync(CancellationToken ct)
        {
            string slotId = RequireSelectedSlotId();
            if (slotId == null)
                return;

            ESaveLoadResult result = await Saves.LoadAsync(slotId, ct);
            Debug.Log($"Load result for '{slotId}': {result}.", this);
        }
    }
}