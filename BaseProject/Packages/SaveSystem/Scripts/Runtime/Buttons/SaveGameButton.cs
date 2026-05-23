using System.Threading;
using Base.SaveSystemPackage.SaveSlot;
using Base.SystemsCorePackage.Services;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SaveSystemPackage.Buttons
{
    /// <summary>
    /// Writes the current game state into the slot, with a fresh screenshot.
    /// </summary>
    public sealed class SaveGameButton : SaveSlotButtonBase
    {
        protected override async Awaitable OnClickAsync(SaveSlotData saveSlotData, CancellationToken ct)
        {
            Texture2D shot = null;
            if (ServiceLocator.TryGet(out IScreenshotCapturer capturer))
                shot = await capturer.CaptureAsync();

            double? playSeconds = ServiceLocator.TryGet(out IPlaytimeProvider pt)
                ? pt.TotalSeconds
                : null;

            await Saves.SaveAsync(saveSlotData.UniqueId, saveSlotData.DisplayName, shot, playSeconds, ct);
            CustomLogger.Log($"Saved '{saveSlotData.DisplayName}'.", this);
        }
    }
}