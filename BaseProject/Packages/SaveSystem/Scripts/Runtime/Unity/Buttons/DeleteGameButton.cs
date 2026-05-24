using System.Threading;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SaveSystemPackage.Unity.Buttons
{
    /// <summary>Deletes the assigned slot's data, metadata and screenshot.</summary>
    public sealed class DeleteGameButton : SaveSlotButtonBase
    {
        protected override async Awaitable OnClickAsync(CancellationToken ct)
        {
            string slotId = RequireAssignedSlotId();
            if (slotId == null)
                return;

            await Saves.DeleteAsync(slotId, ct);
            CustomLogger.Log("Save slot deleted successfully.", this);
        }
    }
}