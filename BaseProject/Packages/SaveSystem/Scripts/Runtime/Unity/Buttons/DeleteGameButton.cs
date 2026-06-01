using System.Threading;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SaveSystemPackage.Unity.Buttons
{
    /// <summary>Deletes the selected slot's data, metadata and screenshot.</summary>
    public sealed class DeleteGameButton : SaveSlotButtonBase
    {
        protected override async Awaitable OnClickAsync(CancellationToken ct)
        {
            string slotId = RequireSelectedSlotId();
            if (slotId == null)
                return;

            await Saves.DeleteAsync(slotId, ct);
            if (Selection.SelectedSlotId == slotId)
                Selection.Clear();

            CustomLogger.Log($"Deleted slot '{slotId}'.", this);
        }
    }
}