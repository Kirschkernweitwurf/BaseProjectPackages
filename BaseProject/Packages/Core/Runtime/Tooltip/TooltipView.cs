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
    /// Draws the tooltip beside its anchor, the cursor or the hovered object, or in the dock area while the
    /// <see cref="TooltipService"/> holds one, without ever letting it leave the screen. The spot is picked while the
    /// tooltip waits to appear and then kept, so a tooltip never wanders when its anchor moves on screen, for example a
    /// 3D object under a camera that follows the pointer. Every <see cref="TooltipEntry"/> gets its own
    /// <see cref="TooltipPanel"/>, copied from the first one, so the content's layout group decides whether they stack
    /// or line up side by side. A tooltip in the dock slides in from the right as it fades in, and one beside its
    /// target shows a small tail pointing at it, like a popover. A tooltip fades in after a short delay, so sweeping
    /// the pointer across the screen stays calm, but moving straight on to the next tooltip shows it at once. The
    /// tooltip never takes the pointer, so it cannot flicker by covering its anchor. Registers itself with the
    /// <see cref="TooltipService"/> on start.
    /// </summary>
    [DisallowMultipleComponent]
    public class TooltipView : MonoBehaviour
    {
        private const int BottomLeftCorner = 0;
        private const int CornerCount = 4;
        private const float NoDelay = 0f;
        private const float Half = 0.5f;
        private const float Opaque = 1f;
        private const float SwapStartAlpha = 0.35f;
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

        [Min(0f)]
        [Tooltip("How far a tooltip in the dock slides in from the right while it fades in, in canvas units. 0 only "
            + "fades.")]
        [SerializeField] private float slideDistance = 8f;

        [Min(0f)]
        [Tooltip("Seconds a tooltip that moves straight on to the next thing takes to bring its new content in.")]
        [SerializeField] private float swapDuration = 0.12f;

        [Title("References")]
        [Tooltip("Content GameObject that holds the tooltip visuals.")]
        [Required] [SerializeField] private GameObject content;

        [Tooltip("First panel inside the content. Further entries get copies of it, next to it.")]
        [Required] [SerializeField] private TooltipPanel panelTemplate;

        [Tooltip("Header of the card, filled from the tooltip's source: what the panels explain. A tooltip with one "
            + "titled entry and no source shows that title here and only its text below. Optional: without it every "
            + "entry is a panel of its own.")]
        [SerializeField] private TooltipPanel sourcePanel;

        [Tooltip("RectTransform used for positioning, the rect of the content object.")]
        [Required] [SerializeField] private RectTransform tooltipRect;

        [Tooltip("Small pointer on the tooltip's edge toward its target while it opens beside it, like on a popover. "
            + "Authored on the left edge pointing left, pivot on its right; flipped when the tooltip opens left of "
            + "the target, hidden in the dock and at the cursor. Optional.")]
        [SerializeField] private RectTransform tail;

        [Min(0f)]
        [Tooltip("Distance the tail keeps from the tooltip's top and bottom edge, in canvas units, so it never sits on "
            + "a rounded corner.")]
        [SerializeField] private float tailMargin = 22f;

        [Tooltip("Canvas the tooltip lives under. Auto-assigned from the parents when empty.")]
        [GetComponentInParent] [SerializeField] private Canvas canvas;

        private readonly Vector3[] _corners = new Vector3[CornerCount];
        private readonly List<TooltipPanel> _panels = new();
        private readonly List<TooltipEntry> _shownEntries = new();

        private TooltipService _service;
        private Camera _canvasCamera;
        private RectTransform _canvasRect;
        private CanvasGroup _contentGroup;
        private Coroutine _showRoutine;
        private Func<Rect> _getScreenAnchor;
        private float _hiddenAt = float.NegativeInfinity;
        private bool _isVisible;
        private bool _isDocked;
        private string _shownName;
        private Vector3 _restPosition;

        /// <summary>
        /// Seconds the pointer rests before a tooltip shows. Settable at runtime, so a settings menu can let the
        /// player pick it. Never below zero.
        /// </summary>
        public float ShowDelay
        {
            get => showDelay;
            set => showDelay = Mathf.Max(NoDelay, value);
        }

        /// <summary>
        /// Whether motion is reduced: a tooltip then only fades in, without sliding. Settable at runtime from an
        /// accessibility setting.
        /// </summary>
        public bool IsMotionReduced { get; set; }

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

            if (ServiceLocator.TryGet(out _service))
                _service.SetView(this);

            Hide();
        }
#endregion

        /// <summary>
        /// Shows the tooltip beside the anchor supplied by the data and keeps it at that spot. Waits for the show
        /// delay first, unless a tooltip is visible or just hid.
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
            string tooltipName = GetName(data);
            bool isSame = isWarm
                && !string.IsNullOrEmpty(tooltipName)
                && tooltipName == _shownName;
            _shownName = tooltipName;

            _getScreenAnchor = data.GetScreenAnchor;
            content.SetActive(true);
            ShowEntries(data);

            // Apply the size fitter now, so the very first placement already uses the final size.
            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);

            StopShowing();
            _showRoutine = StartCoroutine(Appear(isWarm, isSame));
        }

        /// <summary>
        /// Hides the tooltip.
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

        // What a tooltip is about: its header, or the title of its first panel. Two requests with the same name are the
        // same tooltip again, like one its trigger rebuilt.
        private static string GetName(TooltipData data)
        {
            if (data.Source.HasValue)
                return data.Source.Value.Title;

            return data.Entries.Count > 0
                ? data.Entries[0].Title
                : null;
        }

        private static float EaseOut(float progress) => Opaque - (Opaque - progress) * (Opaque - progress);

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
        /// Fills the header from the source, one panel per entry, and hides the panels left over from a bigger tooltip
        /// before. A hairline parts every titled panel from what comes before it in the card.
        /// </summary>
        private void ShowEntries(TooltipData data)
        {
            TooltipEntry? source = CollectShownEntries(data);
            bool hasSource = sourcePanel != null
                && source.HasValue;

            if (sourcePanel != null)
                sourcePanel.gameObject.SetActive(hasSource);

            if (hasSource)
                sourcePanel.Show(source.Value);

            for (int i = 0; i < _shownEntries.Count; i++)
            {
                TooltipEntry entry = _shownEntries[i];
                TooltipPanel panel = PanelAt(i);
                panel.gameObject.SetActive(true);
                panel.Show(entry);
                panel.SetSeparated((hasSource || i > 0)
                    && !string.IsNullOrEmpty(entry.Title));
            }

            for (int i = _shownEntries.Count; i < _panels.Count; i++)
                _panels[i].gameObject.SetActive(false);
        }

        /// <summary>
        /// Lists the entries to show as panels and returns the header. With a header, a lone titled entry names
        /// itself: its title, icon and eyebrow go into the header and only its text stays below.
        /// </summary>
        private TooltipEntry? CollectShownEntries(TooltipData data)
        {
            _shownEntries.Clear();

            if (sourcePanel == null
                || data.Source.HasValue
                || data.Entries.Count != 1
                || string.IsNullOrEmpty(data.Entries[0].Title))
            {
                _shownEntries.AddRange(data.Entries);
                return data.Source;
            }

            TooltipEntry lone = data.Entries[0];
            _shownEntries.Add(new TooltipEntry(lone.Body));

            return new TooltipEntry(lone.Title, null, lone.Icon, lone.IconColor, lone.Eyebrow);
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
        /// Places the tooltip and shows it, after the delay and fade unless <paramref name="isWarm"/>. Moving on to
        /// another tooltip fades the new one in; the same tooltip again, like one rebuilt or pinned, just changes. The
        /// spot is picked once and kept while visible, so the tooltip only moves when it is shown again.
        /// </summary>
        private IEnumerator Appear(bool isWarm, bool isSame)
        {
            if (!isWarm)
                yield return WaitAndFadeIn();
            else if (isSame)
                PlaceTooltip();
            else
                yield return SwapIn();

            _contentGroup.alpha = Opaque;
            _isVisible = true;
            _showRoutine = null;
        }

        // Placed every frame while still invisible, so the tooltip appears where its anchor has settled, for
        // example after a hover pop, and then stays put while it fades in.
        private IEnumerator WaitAndFadeIn()
        {
            _contentGroup.alpha = Transparent;
            float fadeStart = Time.unscaledTime + showDelay;

            do
            {
                PlaceTooltip();
                yield return null;
            }
            while (Time.unscaledTime < fadeStart);

            float slide = _isDocked
                && !IsMotionReduced
                    ? slideDistance
                    : 0f;

            yield return FadeIn(fadeDuration, Transparent, slide);
        }

        // Moving straight on keeps the spot and only brings the new content in, so sweeping across a hand stays calm.
        private IEnumerator SwapIn()
        {
            PlaceTooltip();
            yield return FadeIn(swapDuration, SwapStartAlpha, 0f);
        }

        // Fades from fromAlpha to opaque, sliding in from slide units to the right of the rest spot.
        private IEnumerator FadeIn(float duration, float fromAlpha, float slide)
        {
            float start = Time.unscaledTime;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float progress = elapsed / duration;
                _contentGroup.alpha = Mathf.Lerp(fromAlpha, Opaque, progress);
                tooltipRect.localPosition = _restPosition + Vector3.right * (slide * (Opaque - EaseOut(progress)));
                yield return null;

                elapsed = Time.unscaledTime - start;
            }

            tooltipRect.localPosition = _restPosition;
        }

        /// <summary>
        /// Moves the tooltip to the resolved position for this frame.
        /// </summary>
        private void PlaceTooltip()
        {
            Vector2 size = GetTooltipSize();
            Vector2 pivotScreen;
            Rect anchor = Rect.zero;
            _isDocked = false;

            // A view shown without the service has no dock and no kept-clear areas.
            if (_service == null)
            {
                anchor = _getScreenAnchor.Invoke();
                pivotScreen = ResolvePivotPosition(anchor, size);
            }
            else if (_service.TryGetDock(out Rect dock))
            {
                _isDocked = true;
                pivotScreen = ResolveDockedPosition(dock, size);
            }
            else
            {
                anchor = _service.WidenOverClearAreas(_getScreenAnchor.Invoke());
                pivotScreen = ResolvePivotPosition(anchor, size);
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, pivotScreen, _canvasCamera,
                out Vector2 localPoint);

            _restPosition = localPoint;
            tooltipRect.localPosition = localPoint;
            PlaceTail(anchor, pivotScreen, size);
        }

        /// <summary>
        /// Points the tail at the target from the side the tooltip opened on, at the height of the target's middle as
        /// far as the straight edge allows. Hidden in the dock, at a cursor and when the tooltip had to cover the
        /// target.
        /// </summary>
        private void PlaceTail(Rect anchor, Vector2 topLeft, Vector2 size)
        {
            if (tail == null)
                return;

            bool isRightOfTarget = topLeft.x >= anchor.xMax;
            bool isLeftOfTarget = topLeft.x + size.x <= anchor.xMin;
            bool isShown = !_isDocked
                && anchor.width > 0f
                && (isRightOfTarget || isLeftOfTarget);
            tail.gameObject.SetActive(isShown);

            if (!isShown)
                return;

            float scale = canvas.rootCanvas.scaleFactor;
            float height = size.y / scale;
            float margin = tail.rect.height * Half + tailMargin;
            float fromTop = Mathf.Clamp((topLeft.y - anchor.center.y) / scale, margin,
                Mathf.Max(margin, height - margin));
            float edge = isRightOfTarget
                ? 0f
                : Opaque;
            float direction = isRightOfTarget
                ? Opaque
                : -Opaque;

            tail.anchorMin = new Vector2(edge, Opaque);
            tail.anchorMax = tail.anchorMin;
            tail.anchoredPosition = new Vector2(0f, -fromTop);
            tail.localScale = new Vector3(direction, Opaque, Opaque);
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

            float top = anchor.yMax - offsetY;
            if (top - size.y < edgeMargin)
                top = anchor.yMin + offsetY + size.y;

            return ClampToScreen(new Vector2(left, top), size);
        }

        /// <summary>
        /// Resolves the top left corner in screen space for a docked tooltip: its top right corner in the dock's top
        /// right, so more panels make it grow down and wider text makes it grow to the left. Clamped like any other
        /// spot, so it stays on screen.
        /// </summary>
        private Vector2 ResolveDockedPosition(Rect dock, Vector2 size)
            => ClampToScreen(new Vector2(dock.xMax - size.x, dock.yMax), size);

        /// <summary>
        /// Moves a top left corner in screen space as little as needed to keep a tooltip of <paramref name="size"/>
        /// inside the edge margin.
        /// </summary>
        private Vector2 ClampToScreen(Vector2 topLeft, Vector2 size)
        {
            float maxLeft = Mathf.Max(edgeMargin, Screen.width - edgeMargin - size.x);
            float left = Mathf.Clamp(topLeft.x, edgeMargin, maxLeft);

            float minTop = edgeMargin + size.y;
            float maxTop = Mathf.Max(minTop, Screen.height - edgeMargin);
            float top = Mathf.Clamp(topLeft.y, minTop, maxTop);

            return new Vector2(left, top);
        }
    }
}