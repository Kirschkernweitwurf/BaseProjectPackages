using System;
using Base.AttributesPackage;
using Base.ServicesPackage;
using UnityEngine;

namespace Base.CorePackage.Tooltip
{
    /// <summary>
    /// Keeps this UI element readable while tooltips open above their targets: a tooltip that would cover it opens
    /// beside its target instead. For HUD panels the player reads while choosing, like a score or a list of rules.
    /// Registers with the <see cref="TooltipService"/> while enabled.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TooltipKeepVisible : MonoBehaviour
    {
        [Required] [GetComponent]
        [Tooltip("The area tooltips above their targets must not cover, usually this panel's own rect.")]
        [SerializeField] private RectTransform area;

        private TooltipService _service;
        private Func<Rect> _getArea;

#region Unity Callbacks
        private void Awake() => _getArea = GetScreenArea;

        // The service is fetched in Start, so the first enable registers there.
        private void OnEnable()
        {
            if (_service == null)
                return;

            _service.KeepVisible(this, _getArea);
        }

        private void Start()
        {
            // TryGet logs on its own. Disabling keeps a missing service from failing again on every enable.
            if (!ServiceLocator.TryGet(out _service))
            {
                enabled = false;
                return;
            }

            _service.KeepVisible(this, _getArea);
        }

        private void OnDisable()
        {
            if (_service == null)
                return;

            _service.ReleaseVisible(this);
        }
#endregion

        private Rect GetScreenArea() => TooltipAnchor.Of(area);
    }
}