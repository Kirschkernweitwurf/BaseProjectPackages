using System.Threading;
using Base.AttributePackage;
using Base.CorePackage.Services;
using Base.SaveSystemPackage.Model;
using Base.SaveSystemPackage.Slots;
using Base.SaveSystemPackage.Unity.Capture;
using Base.SaveSystemPackage.Unity.Playtime;
using Base.UtilityPackage.Logging;
using UnityEngine;

namespace Base.SaveSystemPackage.Unity.Buttons
{
    /// <summary>
    /// Writes the current game state to the slot the active model resolves. With
    /// <see cref="forceNewSlot"/> the selection is ignored so the model mints a new slot, giving a
    /// "New Save" button alongside an "Overwrite selected" button. For a fixed slot layout,
    /// <see cref="fixedSlotIndex"/> targets a specific slot so the button is self-contained.
    /// Captures a screenshot and stamps play time when those services are present.
    /// </summary>
    public sealed class SaveGameButton : SaveSlotButtonBase
    {
        private const int UseSelection = -1;

        [Tooltip("Ignore the current selection and ask the model for a new slot.")]
        [SerializeField] private bool forceNewSlot;

        [Tooltip("Fixed-slot index to save into. Used only by the " + nameof(ESlotModel.Fixed)
            + " model; -1 to use the selection.")]
        [DisableIf(nameof(forceNewSlot))]
        [SerializeField] private int fixedSlotIndex = UseSelection;

        protected override async Awaitable OnClickAsync(CancellationToken ct)
        {
            if (!TryResolveTarget(out string slotId))
            {
                CustomLogger.LogWarning($"The {Slots.Model} model could not resolve a save target.", this);
                return;
            }

            ScreenshotData? screenshot = await CaptureScreenshotAsync();

            double? playtimeSeconds = ServiceLocator.TryGet(out IPlaytimeProvider playtimeProvider)
                ? playtimeProvider.TotalSeconds
                : null;

            await Saves.SaveAsync(new SaveRequest(slotId, playtimeSeconds: playtimeSeconds, screenshot: screenshot),
                ct);

            await Slots.EnforcePolicyAsync(slotId, ct);
            Selection.Select(slotId);

            CustomLogger.Log($"Saved game to slot '{slotId}'.", this);
        }

        private async Awaitable<ScreenshotData?> CaptureScreenshotAsync()
        {
            // Thumbnails are opt-in: without a capturer in the scene a save simply has no image.
            if (!ServiceLocator.TryGet(out IScreenshotCapturer capturer))
                return null;

            Texture2D texture = await capturer.CaptureAsync();
            if (texture == null)
                return null;

            ScreenshotData screenshot = new(texture.EncodeToPNG(), texture.width, texture.height);
            Destroy(texture);

            return screenshot;
        }

        private bool TryResolveTarget(out string slotId)
        {
            if (Slots.Model == ESlotModel.Fixed && fixedSlotIndex > UseSelection)
                return Slots.TryResolveSaveTarget(FixedSlotProvider.SlotId(fixedSlotIndex), out slotId);

            string selected = forceNewSlot
                ? null
                : Selection.SelectedSlotId;

            return Slots.TryResolveSaveTarget(selected, out slotId);
        }
    }
}