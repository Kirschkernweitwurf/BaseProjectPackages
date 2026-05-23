using System;
using System.Threading;
using Base.SaveSystemPackage.SaveSlot;
using Base.SystemsCorePackage.Services;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace Base.SaveSystemPackage.Buttons
{
    /// <summary>
    /// Base for save game related buttons. Handles busy state, slot assignment
    /// and save system access. Subclasses just implement the specific action.
    /// </summary>
    public abstract class SaveSlotButtonBase : MonoBehaviour
    {
        [Tooltip("Which save slot this button acts on.")]
        [SerializeField] protected SaveSlotData slot;

        protected ISaveSystem Saves { get; private set; }

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

        /// <summary>
        /// What this specific button does.
        /// </summary>
        protected abstract Awaitable OnClickAsync(SaveSlotData slot, CancellationToken ct);

        /// <summary>
        /// Run the button's action.
        /// </summary>
        // ReSharper disable once AsyncVoidMethod
        private async void Trigger()
        {
            if (_busy)
                return;

            if (slot == null)
            {
                CustomLogger.LogWarning("Save button has no SaveSlot assigned.", this);
                return;
            }

            if (Saves == null)
            {
                if (!ServiceLocator.TryGet(out SaveManager saves))
                    return;

                Saves = saves.SaveSystem;
            }

            _busy = true;
            SetInteractable(false);
            _cts = new CancellationTokenSource();
            try
            {
                await OnClickAsync(slot, _cts.Token);
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Save button action failed: {e.Message}", this);
            }
            finally
            {
                _busy = false;
                SetInteractable(true);
                _cts.Dispose();
                _cts = null;
            }
        }

        private void SetInteractable(bool value)
        {
            if (_button != null)
                _button.interactable = value;
        }
    }
}