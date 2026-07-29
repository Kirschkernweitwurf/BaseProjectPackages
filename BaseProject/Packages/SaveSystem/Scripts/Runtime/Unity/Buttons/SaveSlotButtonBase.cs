using System;
using System.Threading;
using Base.AttributePackage;
using Base.CorePackage.Services;
using Base.SaveSystemPackage.Core;
using Base.SaveSystemPackage.Slots;
using Base.SaveSystemPackage.Unity.Composition;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace Base.SaveSystemPackage.Unity.Buttons
{
    /// <summary>
    /// Base for save-related buttons. Handles busy state, service resolution and cancellation, while
    /// subclasses implement the specific action. Slot identity is read from the runtime
    /// <see cref="SaveSlotSelection"/>, not from an authored asset.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public abstract class SaveSlotButtonBase : MonoBehaviour
    {
        [GetComponent] [Required] [SerializeField] private Button button;

        /// <summary>The save system, resolved on the first click.</summary>
        protected ISaveSystem Saves { get; private set; }

        /// <summary>The active slot provider, resolved on the first click.</summary>
        protected ISaveSlotProvider Slots { get; private set; }

        /// <summary>The shared slot selection, resolved on the first click.</summary>
        protected SaveSlotSelection Selection { get; private set; }

        private bool _busy;
        private CancellationTokenSource _cts;

#region Unity Callbacks
        protected virtual void Awake() => button.onClick.AddListener(Trigger);

        protected virtual void OnDestroy()
        {
            button.onClick.RemoveListener(Trigger);

            _cts?.Cancel();
            _cts?.Dispose();
        }
#endregion

        /// <summary>What this specific button does.</summary>
        protected abstract Awaitable OnClickAsync(CancellationToken ct);

        /// <summary>The selected slot id, or <c>null</c> with a warning when none is selected.</summary>
        /// <returns>The selected slot id.</returns>
        protected string RequireSelectedSlotId()
        {
            string slotId = Selection.SelectedSlotId;
            if (!string.IsNullOrEmpty(slotId))
                return slotId;

            CustomLogger.LogWarning("No save slot is selected.", this);
            return null;
        }

        // Async void is the only shape a UnityEvent listener can take. It is safe here because the
        // whole body is wrapped in a try/catch, so nothing can escape into an unobserved exception.
        // ReSharper disable once AsyncVoidMethod
        private async void Trigger()
        {
            if (_busy)
                return;

            if (!EnsureServices())
                return;

            _busy = true;
            button.interactable = false;
            _cts = new CancellationTokenSource();
            try
            {
                await OnClickAsync(_cts.Token);
            }
            catch (OperationCanceledException) { }
            catch (Exception exception)
            {
                CustomLogger.LogError($"Save button action failed: {exception.Message}", this);
            }
            finally
            {
                _busy = false;
                button.interactable = true;
                _cts?.Dispose();
                _cts = null;
            }
        }

        private bool EnsureServices()
        {
            if (Saves != null && Slots != null && Selection != null)
                return true;

            // TryGet logs on its own. Going dead instead of guarding every click keeps a missing
            // manager from filling the console one click at a time.
            if (!ServiceLocator.TryGet(out SaveManager manager))
            {
                button.interactable = false;
                enabled = false;

                return false;
            }

            Saves = manager.SaveSystem;
            Slots = manager.Slots;
            Selection = manager.Selection;

            return Saves != null && Slots != null && Selection != null;
        }
    }
}