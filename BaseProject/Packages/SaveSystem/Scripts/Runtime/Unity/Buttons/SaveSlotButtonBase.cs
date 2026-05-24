using System;
using System.Threading;
using Base.SaveSystemPackage.Slots;
using Base.SaveSystemPackage.Unity.Composition;
using Base.SystemsCorePackage.Services;
using Base.UtilityPackage.Identification;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace Base.SaveSystemPackage.Unity.Buttons
{
    /// <summary>
    /// Base for save-related buttons. Handles busy state, service resolution and cancellation.
    /// Subclasses implement the specific action. The slot reference is optional: with one assigned
    /// the button acts on that slot; without one, a save button can mint a new slot via the provider.
    /// </summary>
    public abstract class SaveSlotButtonBase : MonoBehaviour
    {
        [Tooltip("Which save slot this button acts on. Leave empty on a Save button to create a new slot.")]
        [SerializeField] protected UniqueIdScriptableObject slot;

        protected ISaveSystem Saves { get; private set; }
        protected ISaveSlotProvider Slots { get; private set; }

        private bool _busy;
        private Button _button;
        private CancellationTokenSource _cts;

        protected virtual void Awake()
        {
            if (TryGetComponent(out _button))
                _button.onClick.AddListener(Trigger);
        }

        protected virtual void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(Trigger);

            _cts?.Cancel();
            _cts?.Dispose();
        }

        /// <summary>What this specific button does.</summary>
        protected abstract Awaitable OnClickAsync(CancellationToken ct);

        /// <summary>Resolve the id of an existing slot this button targets, or null with a warning.</summary>
        protected string RequireAssignedSlotId()
        {
            if (slot != null)
                return slot.UniqueId;

            CustomLogger.LogWarning("This button needs a SaveSlot assigned.", this);
            return null;
        }

        // ReSharper disable once AsyncVoidMethod
        private async void Trigger()
        {
            if (_busy)
                return;

            if (!EnsureServices())
                return;

            _busy = true;
            SetInteractable(false);
            _cts = new CancellationTokenSource();
            try
            {
                await OnClickAsync(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                // Button destroyed mid-action; nothing to do.
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Save button action failed: {e.Message}", this);
            }
            finally
            {
                _busy = false;
                SetInteractable(true);
                _cts?.Dispose();
                _cts = null;
            }
        }

        private bool EnsureServices()
        {
            if (Saves != null && Slots != null)
                return true;

            if (!ServiceLocator.TryGet(out SaveManager manager))
            {
                CustomLogger.LogWarning("No SaveManager available.", this);
                return false;
            }

            Saves = manager.SaveSystem;
            Slots = manager.Slots;
            return Saves != null && Slots != null;
        }

        private void SetInteractable(bool value)
        {
            if (_button != null)
                _button.interactable = value;
        }
    }
}