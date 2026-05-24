using System.Threading;
using Base.SaveSystemPackage.Model;
using Base.SaveSystemPackage.Unity.Capture;
using Base.SaveSystemPackage.Unity.Playtime;
using Base.SystemsCorePackage.Services;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SaveSystemPackage.Unity.Buttons
{
    /// <summary>
    /// Writes the current game state. With a slot assigned it overwrites that slot; with none it
    /// mints a new slot via the provider (the Appending and Named models). Captures a screenshot
    /// and stamps play time if those services are present.
    /// </summary>
    public sealed class SaveGameButton : SaveSlotButtonBase
    {
        protected override async Awaitable OnClickAsync(CancellationToken ct)
        {
            string slotId = slot != null
                ? slot.UniqueId
                : Slots.SupportsNewSlots
                    ? Slots.CreateNewSlotId()
                    : null;

            if (slotId == null)
            {
                CustomLogger.LogWarning("Save button has no slot and the slot model has no free slots.", this);
                return;
            }

            ScreenshotData? shot = null;
            if (ServiceLocator.TryGet(out IScreenshotCapturer capturer))
            {
                Texture2D tex = await capturer.CaptureAsync();
                if (tex != null)
                {
                    shot = new ScreenshotData(tex.EncodeToPNG(), tex.width, tex.height);
                    Destroy(tex);
                }
            }

            double? playSeconds = ServiceLocator.TryGet(out IPlaytimeProvider pt) ? pt.TotalSeconds : null;

            await Saves.SaveAsync(new SaveRequest(slotId, displayName: null, playSeconds, shot), ct);
            await Slots.EnforcePolicyAsync(slotId, ct);
            CustomLogger.Log("Saved game successfully.", this);
        }
    }
}