using System;
using System.Collections;
using System.Collections.Generic;
using Base.AttributesPackage;
using Base.ServicesPackage;
using Base.UtilityPackage.Logging;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Base.CorePackage.Tooltip
{
    /// <summary>
    /// Draws the tooltip beside its anchor, the cursor or the hovered object, above the hovered object when its data
    /// asks for <see cref="ETooltipPlacement.AboveTarget"/> and there is room, or in the dock area while the
    /// <see cref="TooltipService"/> holds one, without ever letting it leave the screen. A tooltip waits until its
    /// target has come to rest, like a card at the top of its hover pop, then picks its side and spot once and keeps
    /// them, so it never jumps from one side to another while the target is still on its way. A tooltip beside its
    /// target stays at that spot, so it never wanders when its anchor moves on screen, for example a 3D object under a
    /// camera that follows the pointer. A tooltip above its target moves along with it on the side it picked, so its
    /// tail stays on the target when that slides on or sways. Every <see cref="TooltipEntry"/> gets its own
    /// <see cref="TooltipPanel"/>, copied from the first one, so the content's layout group decides whether they stack
    /// or line up side by side. A tooltip in the dock slides in from the right as it fades in, and one beside or above
    /// its target shows a small tail pointing at it, like a popover. A tooltip fades in after a short delay, so
    /// sweeping the pointer across the screen stays calm, but moving straight on to the next tooltip shows it at once.
    /// The tooltip never takes the pointer, so it cannot flicker by covering its anchor. Registers itself with the
    /// <see cref="TooltipService"/> on start.
    /// </summary>
    [DisallowMultipleComponent]
    public class TooltipView : MonoBehaviour
    {
        private const int BottomLeftCorner = 0;
        private const int CornerCount = 4;
        private const float Half = 0.5f;
        private const float NoDelay = 0f;
        private const float Opaque = 1f;
        private const float QuarterTurn = 90f;

        // Looks in a row the target must stay still for to count as resting. One is not enough: a target often starts
        // to move only on the frame after the hover that asked for the tooltip, like a card whose pop eases in.
        private const int RestLooks = 2;
        private const float SwapStartAlpha = 0.35f;

        // Share of the target's size the tail may land from its middle and still point into it, like a chip in a
        // screen corner that the tail can't quite center on.
        private const float TargetShare = 0.25f;
        private const int TopRightCorner = 2;
        private const float Transparent = 0f;

        private static readonly Vector2 TopLeftPivot = new(0f, 1f);

        [Title("Settings")]
        [Tooltip("Distance in pixels between the anchor and the tooltip.")]
        [SerializeField] private Vector2 screenOffset = new(15f, 15f);

        [Tooltip("Distance in pixels the tooltip keeps away from the screen edge.")]
        [Min(0f)] [SerializeField] private float edgeMargin = 8f;

        [Min(0f)]
        [FormerlySerializedAs("aboveGap")]
        [Tooltip("Space between the target and the card's edge while the tail points at it, beside or above, in "
            + "canvas units. Leaves room for the tail. Beside a target, Screen Offset wins when it is larger.")]
        [SerializeField] private float targetGap = 30f;

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

        [Min(0f)]
        [Tooltip("Speed below which the target counts as resting, in canvas units per second. A tooltip waits for its "
            + "target to rest before it picks its spot, so it never switches sides while the target still moves, like "
            + "a card rising on hover.")]
        [SerializeField] private float settleSpeed = 150f;

        [Min(0f)]
        [Tooltip("Longest a tooltip waits for its target to rest, in seconds, after the show delay. A target that "
            + "never stops, like one under a swaying camera, still gets its tooltip.")]
        [SerializeField] private float maxSettleWait = 0.5f;

        [Title("References")]
        [Tooltip("Content GameObject that holds the tooltip visuals.")]
        [Required] [SerializeField] private GameObject content;

        [Tooltip("First panel inside the content. Further entries get copies of it, next to it.")]
        [Required] [SerializeField] private TooltipPanel panelTemplate;

        [Tooltip("Header of the card, filled from the tooltip's source: what the panels explain. A tooltip with one "
            + "titled entry and no source shows that title here and only its text below. Optional: without it every "
            + "entry is a panel of its own.")]
        [SerializeField] private TooltipPanel sourcePanel;

        [Tooltip("Second card beside the main one for entries marked Is Aside, like the terms a pinned tooltip "
            + "mentions; shown only while it holds some. Its panels are copies of the panel template. Optional: "
            + "without it those entries join the main card.")]
        [SerializeField] private Transform asideCard;

        [Tooltip("RectTransform used for positioning, the rect of the content object.")]
        [Required] [SerializeField] private RectTransform tooltipRect;

        [Tooltip("Small pointer on the tooltip's edge toward its target while it opens beside or above it, like on a "
            + "popover. Authored on the left edge pointing left, pivot on its base; flipped when the tooltip opens "
            + "left of the target, turned to point down from the card's bottom edge when it opens above, hidden in "
            + "the dock and at the cursor. Optional.")]
        [SerializeField] private RectTransform tail;

        [Min(0f)]
        [Tooltip("Distance the tail keeps from the tooltip's edges, in canvas units, so it never sits on a rounded "
            + "corner: at least the card's corner radius. A short target gets the tooltip raised until the tail meets "
            + "its middle.")]
        [SerializeField] private float tailMargin = 22f;

        [Tooltip("Canvas the tooltip lives under. Auto-assigned from the parents when empty.")]
        [GetComponentInParent] [SerializeField] private Canvas canvas;

        private readonly Vector3[] _corners = new Vector3[CornerCount];
        private readonly List<TooltipPanel> _panels = new();
        private readonly List<TooltipPanel> _asidePanels = new();
        private readonly List<TooltipEntry> _asideEntries = new();
        private readonly List<TooltipEntry> _shownEntries = new();

        private TooltipService _service;
        private Camera _canvasCamera;
        private RectTransform _canvasRect;
        private RectTransform _card;
        private CanvasGroup _contentGroup;
        private Coroutine _showRoutine;
        private Func<Rect> _getScreenAnchor;
        private float _hiddenAt = float.NegativeInfinity;
        private bool _isVisible;
        private bool _isPlaced;
        private bool _isDocked;
        private bool _isAbove;
        private bool _hasSideTail;
        private bool _isRightOfTarget;
        private bool _hasLastTarget;
        private int _stillLooks;
        private Rect _lastTarget;
        private Vector2 _followOffset;
        private ETooltipPlacement _placement;
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
            _card = (RectTransform)panelTemplate.transform.parent;
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

        // A tooltip above its target moves along with it once placed, so the tail stays on the target when it slides
        // on, like a card moving to a new slot in the hand, or the camera sways.
        private void LateUpdate()
        {
            if (!_isPlaced
                || _isDocked
                || _placement != ETooltipPlacement.AboveTarget)
                return;

            FollowTarget();
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
            _placement = data.Placement;
            _isPlaced = false;
            _hasLastTarget = false;
            _stillLooks = 0;
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
            _isPlaced = false;
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

        // The tail goes as close to the target's middle as the straight edge between lowest and highest allows; it
        // points into the target while that spot stays within a quarter of the target's size from its middle.
        private static bool IsTailOnTarget(float middle, float targetSize, float lowest, float highest)
        {
            float spot = Mathf.Clamp(middle, lowest, Mathf.Max(lowest, highest));

            return Mathf.Abs(spot - middle) <= targetSize * TargetShare;
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
        /// Fills the header from the source, one panel per entry in the main card and one per aside entry in the
        /// second card, and hides the panels left over from a bigger tooltip before.
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

            ShowPanels(_shownEntries, _panels, panelTemplate.transform.parent, hasSource);

            if (asideCard == null)
                return;

            asideCard.gameObject.SetActive(_asideEntries.Count > 0);
            ShowPanels(_asideEntries, _asidePanels, asideCard, false);
        }

        /// <summary>
        /// Shows one panel of <paramref name="pool"/> per entry and hides the rest. A hairline parts every titled panel
        /// from what comes before it in its card.
        /// </summary>
        private void ShowPanels(List<TooltipEntry> entries, List<TooltipPanel> pool, Transform card, bool hasHeader)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                TooltipEntry entry = entries[i];
                TooltipPanel panel = PanelAt(pool, card, i);
                panel.gameObject.SetActive(true);
                panel.Show(entry);
                panel.SetSeparated((hasHeader || i > 0)
                    && !string.IsNullOrEmpty(entry.Title));
            }

            for (int i = entries.Count; i < pool.Count; i++)
                pool[i].gameObject.SetActive(false);
        }

        /// <summary>
        /// Lists the entries to show as panels and returns the header. With a header, a lone titled entry names
        /// itself: its title, icon and eyebrow go into the header and only its text stays below.
        /// </summary>
        private TooltipEntry? CollectShownEntries(TooltipData data)
        {
            _shownEntries.Clear();
            _asideEntries.Clear();

            foreach (TooltipEntry entry in data.Entries)
            {
                if (entry.IsAside
                    && asideCard != null)
                    _asideEntries.Add(entry);
                else
                    _shownEntries.Add(entry);
            }

            if (sourcePanel == null
                || data.Source.HasValue
                || _shownEntries.Count != 1
                || string.IsNullOrEmpty(_shownEntries[0].Title))
                return data.Source;

            TooltipEntry lone = _shownEntries[0];
            _shownEntries[0] = new TooltipEntry(lone.Body);

            return new TooltipEntry(lone.Title, null, lone.Icon, lone.IconColor, lone.Eyebrow,
                isIconFramed: lone.IsIconFramed);
        }

        /// <summary>
        /// The panel of <paramref name="pool"/> for the entry at <paramref name="index"/>, copied from the template
        /// into <paramref name="card"/> the first time it is needed and reused after that.
        /// </summary>
        private TooltipPanel PanelAt(List<TooltipPanel> pool, Transform card, int index)
        {
            while (pool.Count <= index)
                pool.Add(Instantiate(panelTemplate, card));

            return pool[index];
        }

        /// <summary>
        /// Places the tooltip and shows it, after the delay and fade unless <paramref name="isWarm"/>. Moving on to
        /// another tooltip fades the new one in; the same tooltip again, like one rebuilt or pinned, just changes. The
        /// spot is picked once and kept while visible, so the tooltip only moves when it is shown again or follows its
        /// target.
        /// </summary>
        private IEnumerator Appear(bool isWarm, bool isSame)
        {
            if (isSame)
                PlaceTooltip();
            else
                yield return WaitAndFadeIn(isWarm);

            _contentGroup.alpha = Opaque;
            _isVisible = true;
            _showRoutine = null;
        }

        // Invisible until the target rests, so the tooltip picks its spot once, where the target ends up, and keeps it
        // while it fades in. Moving straight on from another tooltip skips the show delay and only brings the new
        // content in, so sweeping across a hand stays calm.
        private IEnumerator WaitAndFadeIn(bool isWarm)
        {
            _contentGroup.alpha = Transparent;
            float earliest = isWarm
                ? Time.unscaledTime
                : Time.unscaledTime + showDelay;

            yield return WaitForRest(earliest);

            PlaceTooltip();

            if (isWarm)
                yield return FadeIn(swapDuration, SwapStartAlpha, 0f);
            else
                yield return FadeIn(fadeDuration, Transparent, GetSlide());
        }

        // Only a tooltip in the dock slides in, and only while motion is not reduced.
        private float GetSlide() => _isDocked
            && !IsMotionReduced
                ? slideDistance
                : 0f;

        /// <summary>
        /// Waits until <paramref name="earliest"/> and until the target has come to rest, so the tooltip picks its
        /// side for where the target ends up, like a card at the top of its hover pop, and never has to switch while
        /// the target is on its way. Gives up waiting for the rest after the max settle wait.
        /// </summary>
        private IEnumerator WaitForRest(float earliest)
        {
            float latest = earliest + maxSettleWait;

            while ((!IsTargetResting() && Time.unscaledTime < latest)
                || Time.unscaledTime < earliest)
                yield return null;
        }

        /// <summary>
        /// Whether the target moved slower than the settle speed for the last few looks in a row. A dock and a cursor
        /// always count as resting. The first look only takes the measure, since one frame shows no motion.
        /// </summary>
        private bool IsTargetResting()
        {
            if (_service != null
                && _service.TryGetDock(out _))
                return true;

            Rect target = _getScreenAnchor.Invoke();
            Rect last = _lastTarget;
            bool hasLast = _hasLastTarget;
            _lastTarget = target;
            _hasLastTarget = true;

            if (target.width <= 0f)
                return true;

            if (!hasLast)
                return false;

            float moved = Mathf.Max(Vector2.Distance(target.min, last.min), Vector2.Distance(target.max, last.max));
            _stillLooks = moved <= settleSpeed * GetPixelsPerUnit() * Time.unscaledDeltaTime
                ? _stillLooks + 1
                : 0;

            return _stillLooks >= RestLooks;
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
        /// Picks the tooltip's side, spot and tail for where its target is now and moves it there. Runs once per show,
        /// after the target came to rest; from then on <see cref="FollowTarget"/> only carries the spot along.
        /// </summary>
        private void PlaceTooltip()
        {
            Vector2 size = GetTooltipSize();
            Vector2 topLeft;
            Rect target = Rect.zero;
            Rect anchor = Rect.zero;
            _isDocked = false;
            _isAbove = false;

            // A view shown without the service has no dock and no kept-clear or kept-visible areas.
            if (_service == null)
            {
                target = _getScreenAnchor.Invoke();
                anchor = target;
                topLeft = ResolvePivotPosition(anchor, size);
            }
            else if (_service.TryGetDock(out Rect dock))
            {
                _isDocked = true;
                topLeft = ResolveDockedPosition(dock, size);
            }
            else
            {
                target = _getScreenAnchor.Invoke();
                anchor = target;

                if (_placement == ETooltipPlacement.AboveTarget
                    && TryResolveAbovePosition(target, size, out Vector2 abovePosition))
                {
                    _isAbove = true;
                    topLeft = abovePosition;
                }
                else
                {
                    anchor = _service.WidenOverClearAreas(target, GetBesideGap(target));
                    topLeft = ResolvePivotPosition(anchor, size);

                    if (_placement == ETooltipPlacement.AboveTarget)
                        topLeft = ClampToScreen(new Vector2(topLeft.x, RaiseBeside(target, topLeft.y, size)), size);

                    // Close to the screen's bottom or top the tail may not reach the target's middle from the side;
                    // above the target it does.
                    if (!CanTailReachBeside(anchor, topLeft, size)
                        && TryResolveAbovePosition(target, size, out Vector2 fallback))
                    {
                        _isAbove = true;
                        anchor = target;
                        topLeft = fallback;
                    }
                }
            }

            PickTailSide(anchor, topLeft, size);
            _followOffset = topLeft - GetFollowPoint(target);
            _isPlaced = true;
            MoveTo(target, topLeft, size);
        }

        /// <summary>
        /// Carries the tooltip along with its target on the side and at the spot it picked, as if pinned to it, so it
        /// never switches sides while the target moves. Only clamps it back onto the screen.
        /// </summary>
        private void FollowTarget()
        {
            Rect target = _getScreenAnchor.Invoke();
            Vector2 size = GetTooltipSize();

            MoveTo(target, ClampToScreen(GetFollowPoint(target) + _followOffset, size), size);
        }

        // The middle of the target's edge that faces the tooltip, so a target that grows, like a card swelling on
        // hover, keeps its gap to the tooltip instead of pushing into it. Its middle where no edge faces the tooltip.
        private Vector2 GetFollowPoint(Rect target)
        {
            if (_isAbove)
                return new Vector2(target.center.x, target.yMax);

            if (!_hasSideTail)
                return target.center;

            return _isRightOfTarget
                ? new Vector2(target.xMax, target.center.y)
                : new Vector2(target.xMin, target.center.y);
        }

        // Moves the tooltip's top left corner to topLeft, in screen pixels, and points the tail at the target.
        private void MoveTo(Rect target, Vector2 topLeft, Vector2 size)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, topLeft, _canvasCamera,
                out Vector2 localPoint);

            _restPosition = localPoint;
            tooltipRect.localPosition = localPoint;
            PlaceTail(target, topLeft, size);
        }

        /// <summary>
        /// Decides which edge the tail sits on for a tooltip beside <paramref name="anchor"/>: the one facing it. No
        /// side tail above the target, in the dock, at a cursor or when the tooltip had to cover the target.
        /// </summary>
        private void PickTailSide(Rect anchor, Vector2 topLeft, Vector2 size)
        {
            _isRightOfTarget = topLeft.x >= anchor.xMax;
            _hasSideTail = !_isAbove
                && !_isDocked
                && anchor.width > 0f
                && (_isRightOfTarget || topLeft.x + size.x <= anchor.xMin);
        }

        /// <summary>
        /// Points the tail at the target from the side the tooltip opened on, at the height of the target's middle as
        /// far as the straight edge allows, or down from the card's bottom edge when it opened above. Hidden in the
        /// dock, at a cursor and when the tooltip had to cover the target.
        /// </summary>
        private void PlaceTail(Rect target, Vector2 topLeft, Vector2 size)
        {
            if (tail == null)
                return;

            tail.gameObject.SetActive(_isAbove || _hasSideTail);

            if (_isAbove)
                PlaceTailBelow(target, topLeft);
            else if (_hasSideTail)
                PlaceTailBeside(target, topLeft, size);
        }

        // The tail on the edge facing the target, at the height of the target's middle as far as the corners allow.
        private void PlaceTailBeside(Rect target, Vector2 topLeft, Vector2 size)
        {
            float scale = GetPixelsPerUnit();
            float height = size.y / scale;
            float margin = tail.rect.height * Half + tailMargin;
            float fromTop = Mathf.Clamp((topLeft.y - target.center.y) / scale, margin,
                Mathf.Max(margin, height - margin));
            float edge = _isRightOfTarget
                ? 0f
                : Opaque;
            float direction = _isRightOfTarget
                ? Opaque
                : -Opaque;

            tail.anchorMin = new Vector2(edge, Opaque);
            tail.anchorMax = tail.anchorMin;
            tail.anchoredPosition = new Vector2(0f, -fromTop);
            tail.localScale = new Vector3(direction, Opaque, Opaque);
            tail.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Points the tail down at the target's middle from the card's bottom edge, as far as the straight edge allows,
        /// so it never sits on a rounded corner.
        /// </summary>
        private void PlaceTailBelow(Rect anchor, Vector2 topLeft)
        {
            float scale = GetPixelsPerUnit();
            Rect card = GetScreenRect(_card);
            float margin = tail.rect.height * Half + tailMargin;
            float cardFromLeft = (card.xMin - topLeft.x) / scale;
            float cardRight = cardFromLeft + card.width / scale;
            float fromLeft = Mathf.Clamp((anchor.center.x - topLeft.x) / scale, cardFromLeft + margin,
                Mathf.Max(cardFromLeft + margin, cardRight - margin));

            tail.anchorMin = TopLeftPivot;
            tail.anchorMax = TopLeftPivot;
            tail.anchoredPosition = new Vector2(fromLeft, -(topLeft.y - card.yMin) / scale);
            tail.localScale = Vector3.one;
            tail.localRotation = Quaternion.Euler(0f, 0f, QuarterTurn);
        }

        /// <summary>
        /// Measures the tooltip in screen pixels, using the layout as it stands this frame.
        /// </summary>
        private Vector2 GetTooltipSize() => GetScreenRect(tooltipRect).size;

        /// <summary>
        /// Whether a tooltip beside <paramref name="anchor"/> at <paramref name="topLeft"/> lets its tail point into
        /// the target without touching a rounded corner. Always true without a tail or at a cursor.
        /// </summary>
        private bool CanTailReachBeside(Rect anchor, Vector2 topLeft, Vector2 size)
        {
            if (tail == null
                || anchor.width <= 0f)
                return true;

            float clearance = GetTailClearance();

            return IsTailOnTarget(anchor.center.y, anchor.height, topLeft.y - size.y + clearance,
                topLeft.y - clearance);
        }


        /// <summary>
        /// Screen pixels from the tooltip's edge to the middle of the tail at its closest to a corner: half the tail
        /// plus the tail margin.
        /// </summary>
        private float GetTailClearance() => (tail.rect.height * Half + tailMargin) * GetPixelsPerUnit();

        /// <summary>
        /// Screen pixels per canvas unit of the tooltip, scaled parents included, so distances in canvas units land
        /// where they are meant to on screen. The canvas scale factor alone misses a scaled parent, like a text size
        /// zoom.
        /// </summary>
        private float GetPixelsPerUnit()
        {
            float width = tooltipRect.rect.width;

            return width > 0f
                ? GetScreenRect(tooltipRect).width / width
                : canvas.rootCanvas.scaleFactor;
        }

        /// <summary>
        /// The screen rect of <paramref name="target"/>, a part of the tooltip, using the layout as it stands this
        /// frame.
        /// </summary>
        private Rect GetScreenRect(RectTransform target)
        {
            target.GetWorldCorners(_corners);
            Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(_canvasCamera, _corners[BottomLeftCorner]);
            Vector2 topRight = RectTransformUtility.WorldToScreenPoint(_canvasCamera, _corners[TopRightCorner]);

            return Rect.MinMaxRect(Mathf.Min(bottomLeft.x, topRight.x), Mathf.Min(bottomLeft.y, topRight.y),
                Mathf.Max(bottomLeft.x, topRight.x), Mathf.Max(bottomLeft.y, topRight.y));
        }

        /// <summary>
        /// Resolves the top left corner in screen space. Prefers right of the anchor with the tops lined up, flips
        /// to the left or to the bottoms lined up when that would overflow, and clamps as a last resort so it stays
        /// on screen. A cursor is a zero-size anchor, so the tooltip sits below right of it.
        /// </summary>
        private Vector2 ResolvePivotPosition(Rect anchor, Vector2 size)
        {
            float tailGap = GetTailGap(anchor);
            float offsetX = GetBesideGap(anchor);
            float offsetY = Mathf.Abs(screenOffset.y);

            float left = anchor.xMax + offsetX;
            if (left + size.x > Screen.width - edgeMargin)
                left = anchor.xMin - offsetX - size.x;

            // A short target, like a row of a list, sits higher than the tail may go below the rounded corner, so the
            // tooltip rises until its tail meets the target's middle.
            float top = anchor.yMax - offsetY;
            if (tailGap > 0f)
                top = Mathf.Max(top, anchor.center.y + GetTailClearance());

            // With no room below, the bottoms line up, but never so high that the tail misses the target's middle.
            if (top - size.y < edgeMargin)
                top = tailGap > 0f
                    ? Mathf.Min(anchor.yMin + offsetY + size.y, anchor.center.y - GetTailClearance() + size.y)
                    : anchor.yMin + offsetY + size.y;

            return ClampToScreen(new Vector2(left, top), size);
        }

        /// <summary>
        /// Screen pixels between <paramref name="anchor"/> and the side of a tooltip beside it: the screen offset, or
        /// the room the tail needs when that is larger.
        /// </summary>
        private float GetBesideGap(Rect anchor) => Mathf.Max(Mathf.Abs(screenOffset.x), GetTailGap(anchor));

        /// <summary>
        /// Screen pixels the tail needs beside <paramref name="anchor"/>, at every resolution. Zero without a tail or
        /// at a cursor, which gets none.
        /// </summary>
        private float GetTailGap(Rect anchor) => tail != null
            && anchor.width > 0f
                ? targetGap * GetPixelsPerUnit()
                : 0f;

        /// <summary>
        /// Lifts the top of a tooltip that asked to open above its target but had to open beside it as high as the
        /// screen and the tail allow, so it covers as little as it can of the row beside the target, like the other
        /// cards of a hand. Never lowers it.
        /// </summary>
        private float RaiseBeside(Rect target, float top, Vector2 size)
        {
            float highest = Screen.height - edgeMargin;
            if (GetTailGap(target) > 0f)
                highest = Mathf.Min(highest, target.center.y - GetTailClearance() + size.y);

            return Mathf.Max(top, highest);
        }

        /// <summary>
        /// Resolves the top left corner in screen space for a tooltip above its anchor: the card's bottom edge one gap
        /// above the anchor's top, and its edge lined up with the anchor's edge on the side away from the screen's
        /// middle, so it grows outward and leaves what lies beside the anchor readable. Grows toward the middle instead
        /// when only that keeps an area kept visible clear. Fails when the tooltip would leave the top of the screen or
        /// move so far that the tail no longer reaches the anchor's middle, never for an area kept visible: covering a
        /// panel beats opening beside the anchor over what lies next to it.
        /// </summary>
        private bool TryResolveAbovePosition(Rect anchor, Vector2 size, out Vector2 topLeft)
        {
            bool isGrowingRight = anchor.center.x >= Screen.width * Half;
            if (!TryPlaceAbove(anchor, size, isGrowingRight, out topLeft))
                return false;

            if (IsCoveringVisibleArea(topLeft, size)
                && TryPlaceAbove(anchor, size, !isGrowingRight, out Vector2 inward)
                && !IsCoveringVisibleArea(inward, size))
                topLeft = inward;

            return true;
        }

        // Whether a tooltip of size with its top left corner at topLeft, in screen pixels, covers an area kept visible.
        private bool IsCoveringVisibleArea(Vector2 topLeft, Vector2 size)
            => _service.IsCoveringVisibleArea(new Rect(topLeft.x, topLeft.y - size.y, size.x, size.y));

        /// <summary>
        /// Resolves the top left corner in screen space for a tooltip above <paramref name="anchor"/>, with the card's
        /// left edge lined up with the anchor's left edge when <paramref name="isGrowingRight"/>, else its right edge
        /// with the anchor's right edge. Fails when the tooltip would leave the top of the screen or move so far that
        /// the tail no longer reaches the anchor's middle.
        /// </summary>
        private bool TryPlaceAbove(Rect anchor, Vector2 size, bool isGrowingRight, out Vector2 topLeft)
        {
            float scale = GetPixelsPerUnit();
            Rect tooltip = GetScreenRect(tooltipRect);
            Rect card = GetScreenRect(_card);
            float cardFromLeft = card.xMin - tooltip.xMin;
            float cardBottomFromTop = tooltip.yMax - card.yMin;

            float tailClearance = tail == null
                ? 0f
                : GetTailClearance();
            float cardLeft = isGrowingRight
                ? anchor.xMin
                : anchor.xMax - card.width;

            // A narrow target would leave the tail no room next to the edge it lines up with: the card moves over.
            cardLeft = Mathf.Clamp(cardLeft, anchor.center.x - card.width + tailClearance,
                anchor.center.x - tailClearance);
            float top = anchor.yMax + targetGap * scale + cardBottomFromTop;
            topLeft = ClampToScreen(new Vector2(cardLeft - cardFromLeft, top), size);

            float shownCardLeft = topLeft.x + cardFromLeft;
            bool canTailReach = IsTailOnTarget(anchor.center.x, anchor.width, shownCardLeft + tailClearance,
                shownCardLeft + card.width - tailClearance);

            return top <= Screen.height - edgeMargin
                && canTailReach;
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