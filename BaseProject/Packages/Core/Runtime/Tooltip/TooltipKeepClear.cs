using System;
using Base.AttributesPackage;
using Base.ServicesPackage;
using UnityEngine;

namespace Base.CorePackage.Tooltip
{
    /// <summary>
    /// Keeps this UI element clear of the tooltips of the things inside it or alongside it: a tooltip beside one of
    /// them opens past this element instead of covering it, like a list whose rows explain themselves, so the tooltip
    /// and its tail sit next to the list, not on it. Registers with the <see cref="TooltipService"/> while enabled.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TooltipKeepClear : MonoBehaviour
    {
        [Required] [GetComponent]
        [Tooltip("The area a tooltip beside a target inside it or alongside it opens past, usually this panel's own "
            + "visible rect.")]
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

            _service.KeepClear(this, _getArea);
        }

        private void Start()
        {
            // TryGet logs on its own. Disabling keeps a missing service from failing again on every enable.
            if (!ServiceLocator.TryGet(out _service))
            {
                enabled = false;
                return;
            }

            _service.KeepClear(this, _getArea);
        }

        private void OnDisable()
        {
            if (_service == null)
                return;

            _service.ReleaseClear(this);
        }
#endregion

        private Rect GetScreenArea() => TooltipAnchor.Of(area);
    }
}