using System;
using System.Collections;
using System.Collections.Generic;
using Base.AttributesPackage;
using Base.ServicesPackage;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace Base.CorePackage.Tooltip
{
    /// <summary>
    /// Draws the tooltip beside its anchor, the cursor or the hovered object, without ever letting it leave the
    /// screen. Every <see cref="TooltipEntry"/> gets its own <see cref="TooltipPanel"/>, copied from the first one,
    /// so the content's layout group decides whether they stack or line up side by side. A tooltip fades in after a
    /// short delay, so sweeping the pointer across the screen stays calm, but moving straight on to the next
    /// tooltip shows it at once. The tooltip never takes the pointer, so it cannot flicker by covering its anchor.
    /// Registers itself with the <see cref="TooltipService"/> on start.
    /// </summary>
    [DisallowMultipleComponent]
    public class TooltipView : MonoBehaviour
    {
        private const int BottomLeftCorner = 0;
        private const int CornerCount = 4;
        private const float Opaque = 1f;
        private const int TopRightCorner = 2;
        private const float Transparent = 0f;

        private static readonly Vector2 TopLeftPivot = new(0f, 1f);

        [Title("Settings")]
        [Tooltip("Distance in pixels between the anchor and the tooltip.")]
        [SerializeField] private Vector2 screenOffset = new(15f, 15f);

        [Tooltip("Distance in pixels the tooltip keeps away from the screen edge.")]
        [Min(0f)] [SerializeField] private float edgeMargin = 8f;

        [Title("Timing")]
        [Min(0f)]
        [Tooltip("Seconds the pointer rests before a tooltip shows. Unscaled, so it also works while paused.")]
        [SerializeField] private float showDelay = 0.3f;

        [Min(0f)]
        [Tooltip("Seconds a tooltip takes to fade in.")]
        [SerializeField] private float fadeDuration = 0.1f;

        [Min(0f)]
        [Tooltip("Seconds after a tooltip hid in which the next one shows at once, without delay or fade.")]
        [SerializeField] private float warmDuration = 0.4f;

        [Title("References")]
        [Tooltip("Content GameObject that holds the tooltip visuals.")]
        [Required] [SerializeField] private GameObject content;

        [Tooltip("First panel inside the content. Further entries get copies of it, next to it.")]
        [Required] [SerializeField] private TooltipPanel panelTemplate;

        [Tooltip("RectTransform used for positioning, the rect of the content object.")]
        [Required] [SerializeField] private RectTransform tooltipRect;

        [Tooltip("Canvas the tooltip lives under. Auto-assigned from the parents when empty.")]
        [GetComponentInParent] [SerializeField] private Canvas canvas;

        private readonly Vector3[] _corners = new Vector3[CornerCount];
        private readonly List<TooltipPanel> _panels = new();

        private Camera _canvasCamera;
        private RectTransform _canvasRect;
        private CanvasGroup _contentGroup;
        private Coroutine _showRoutine;
        private Func<Rect> _getScreenAnchor;
        private float _hiddenAt = float.NegativeInfinity;
        private bool _isVisible;

#region Unity Callbacks
        private void Start()
        {
            _panels.Add(panelTemplate);
            _canvasRect = (RectTransform)canvas.rootCanvas.transform;
            _canvasCamera = canvas.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.rootCanvas.worldCamera;

            _contentGroup = PrepareContentGroup();
            tooltipRect.pivot = TopLeftPivot;

            if (ServiceLocator.TryGet(out TooltipService service))
                service.SetView(this);

            Hide();
        }
#endregion

        /// <summary>
        /// Shows the tooltip beside the anchor supplied by the data and keeps following it. Waits for the show delay
        /// first, unless a tooltip is visible or just hid.
        /// </summary>
        /// <param name="data">Panels and screen anchor to show.</param>
        public void Show(TooltipData data)
        {
            if (data.GetScreenAnchor == null)
            {
                CustomLogger.LogError($"{nameof(Show)} was called without a screen anchor.", this);
                return;
            }

            if (data.Entries == null
                || data.Entries.Count == 0)
            {
                CustomLogger.LogError($"{nameof(Show)} was called without entries.", this);
                return;
            }

            bool isWarm = _isVisible || Time.unscaledTime - _hiddenAt <= warmDuration;

            _getScreenAnchor = data.GetScreenAnchor;
            content.SetActive(true);
            ShowEntries(data.Entries);

            // Apply the size fitter now, so the very first placement already uses the final size.
            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);

            StopShowing();
            _showRoutine = StartCoroutine(ShowAndFollow(isWarm));
        }

        /// <summary>
        /// Hides the tooltip and stops following its anchor.
        /// </summary>
        public void Hide()
        {
            StopShowing();

            if (_isVisible)
                _hiddenAt = Time.unscaledTime;

            _isVisible = false;
            content.SetActive(false);
            _getScreenAnchor = null;
        }

        /// <summary>
        /// Gives the content a canvas group that never takes the pointer, so the tooltip cannot cover its own anchor
        /// and make it lose the hover.
        /// </summary>
        private CanvasGroup PrepareContentGroup()
        {
            if (!content.TryGetComponent(out CanvasGroup group))
                group = content.AddComponent<CanvasGroup>();

            group.blocksRaycasts = false;
            group.interactable = false;

            return group;
        }

        private void StopShowing()
        {
            if (_showRoutine == null)
                return;

            StopCoroutine(_showRoutine);
            _showRoutine = null;
        }

        /// <summary>
        /// Fills one panel per entry and hides the panels left over from a bigger tooltip before.
        /// </summary>
        private void ShowEntries(IReadOnlyList<TooltipEntry> entries)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                TooltipPanel panel = PanelAt(i);
                panel.gameObject.SetActive(true);
                panel.Show(entries[i]);
            }

            for (int i = entries.Count; i < _panels.Count; i++)
                _panels[i].gameObject.SetActive(false);
        }

        /// <summary>
        /// The panel for the entry at <paramref name="index"/>, copied from the template the first time it is
        /// needed and reused after that.
        /// </summary>
        private TooltipPanel PanelAt(int index)
        {
            while (_panels.Count <= index)
                _panels.Add(Instantiate(panelTemplate, panelTemplate.transform.parent));

            return _panels[index];
        }

        /// <summary>
        /// Waits and fades in unless <paramref name="isWarm"/>, then repositions the tooltip every frame for as
        /// long as it stays visible. It is placed from the first frame on, so it never pops in at an old spot.
        /// </summary>
        private IEnumerator ShowAndFollow(bool isWarm)
        {
            if (!isWarm)
                yield return WaitAndFadeIn();

            _contentGroup.alpha = Opaque;
            _isVisible = true;

            while (_getScreenAnchor != null)
            {
                PlaceTooltip();
                yield return null;
            }
        }

        private IEnumerator WaitAndFadeIn()
        {
            _contentGroup.alpha = Transparent;
            float fadeStart = Time.unscaledTime + showDelay;

            while (Time.unscaledTime < fadeStart)
            {
                PlaceTooltip();
                yield return null;
            }

            float elapsed = Time.unscaledTime - fadeStart;

            while (elapsed < fadeDuration)
            {
                _contentGroup.alpha = elapsed / fadeDuration;
                PlaceTooltip();
                yield return null;

                elapsed = Time.unscaledTime - fadeStart;
            }
        }

        /// <summary>
        /// Moves the tooltip to the resolved position for this frame.
        /// </summary>
        private void PlaceTooltip()
        {
            Vector2 size = GetTooltipSize();
            Vector2 pivotScreen = ResolvePivotPosition(_getScreenAnchor.Invoke(), size);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, pivotScreen, _canvasCamera,
                out Vector2 localPoint);

            tooltipRect.localPosition = localPoint;
        }

        /// <summary>
        /// Measures the tooltip in screen pixels, using the layout as it stands this frame.
        /// </summary>
        private Vector2 GetTooltipSize()
        {
            tooltipRect.GetWorldCorners(_corners);
            Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(_canvasCamera, _corners[BottomLeftCorner]);
            Vector2 topRight = RectTransformUtility.WorldToScreenPoint(_canvasCamera, _corners[TopRightCorner]);

            return new Vector2(Mathf.Abs(topRight.x - bottomLeft.x), Mathf.Abs(topRight.y - bottomLeft.y));
        }

        /// <summary>
        /// Resolves the top left corner in screen space. Prefers right of the anchor with the tops lined up, flips
        /// to the left or to the bottoms lined up when that would overflow, and clamps as a last resort so it stays
        /// on screen. A cursor is a zero-size anchor, so the tooltip sits below right of it.
        /// </summary>
        private Vector2 ResolvePivotPosition(Rect anchor, Vector2 size)
        {
            float offsetX = Mathf.Abs(screenOffset.x);
            float offsetY = Mathf.Abs(screenOffset.y);

            float left = anchor.xMax + offsetX;
            if (left + size.x > Screen.width - edgeMargin)
                left = anchor.xMin - offsetX - size.x;

            float maxLeft = Mathf.Max(edgeMargin, Screen.width - edgeMargin - size.x);
            left = Mathf.Clamp(left, edgeMargin, maxLeft);

            float top = anchor.yMax - offsetY;
            if (top - size.y < edgeMargin)
                top = anchor.yMin + offsetY + size.y;

            float minTop = edgeMargin + size.y;
            float maxTop = Mathf.Max(minTop, Screen.height - edgeMargin);
            top = Mathf.Clamp(top, minTop, maxTop);

            return new Vector2(left, top);
        }
    }
}