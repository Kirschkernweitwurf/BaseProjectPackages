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
    /// Writes the current game state to the slot the active model resolves. With
    /// <see cref="forceNewSlot"/> the selection is ignored so the model mints a new slot, giving a
    /// "New Save" button alongside an "Overwrite selected" button. Captures a screenshot and stamps
    /// play time when those services are present.
    /// </summary>
    public sealed class SaveGameButton : SaveSlotButtonBase
    {
        [Tooltip("Ignore the current selection and ask the model for a new slot.")]
        [SerializeField] private bool forceNewSlot;

        protected override async Awaitable OnClickAsync(CancellationToken ct)
        {
            string selected = forceNewSlot ? null : Selection.SelectedSlotId;
            if (!Slots.TryResolveSaveTarget(selected, out string slotId))
            {
                CustomLogger.LogWarning($"The {Slots.Model} model could not resolve a save target.", this);
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
            Selection.Select(slotId);
            CustomLogger.Log($"Saved game to slot '{slotId}'.", this);
        }
    }
}