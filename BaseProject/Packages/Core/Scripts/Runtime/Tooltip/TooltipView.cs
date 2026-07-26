using System;
using System.Collections;
using Base.AttributePackage;
using Base.CorePackage.Services;
using Base.UtilityPackage.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Base.CorePackage.Tooltip
{
    /// <summary>
    /// Draws the tooltip and keeps it next to the cursor without ever letting it leave the screen.
    /// Registers itself with the <see cref="TooltipService"/> on start.
    /// </summary>
    [DisallowMultipleComponent]
    public class TooltipView : MonoBehaviour
    {
        private const int BottomLeftCorner = 0;
        private const int CornerCount = 4;
        private const int TopRightCorner = 2;

        private static readonly Vector2 TopLeftPivot = new(0f, 1f);

        [Header("Settings")]

        [Tooltip("Distance in pixels between the cursor and the tooltip.")]
        [SerializeField] private Vector2 screenOffset = new(15f, 15f);

        [Tooltip("Distance in pixels the tooltip keeps away from the screen edge.")]
        [Min(0f)] [SerializeField] private float edgeMargin = 8f;

        [Header("References")]

        [Tooltip("Content GameObject that holds the tooltip visuals.")]
        [Required] [SerializeField] private GameObject content;

        [Tooltip("Text element that shows the message.")]
        [Required] [SerializeField] private TextMeshProUGUI textElement;

        [Tooltip("RectTransform used for positioning, the rect of the content object.")]
        [Required] [SerializeField] private RectTransform tooltipRect;

        [Tooltip("Canvas the tooltip lives under. Auto-assigned from the parents when empty.")]
        [GetComponentInParent] [SerializeField] private Canvas canvas;

        private readonly Vector3[] _corners = new Vector3[CornerCount];

        private Coroutine _followRoutine;
        private Func<Vector2> _getScreenPosition;

#region Unity Callbacks
        private void Start()
        {
            if (ServiceLocator.TryGet(out TooltipService service))
                service.SetView(this);

            Hide();
        }
#endregion

        /// <summary>
        /// Shows the tooltip and starts following the position supplied by the data.
        /// </summary>
        /// <param name="data">Message and screen position to show.</param>
        public void Show(TooltipData data)
        {
            if (data.GetScreenPosition == null)
            {
                CustomLogger.LogError($"{nameof(Show)} was called without a screen position.", this);
                return;
            }

            _getScreenPosition = data.GetScreenPosition;
            textElement.text = data.Message;
            content.SetActive(true);

            // Apply the size fitter now, so the very first placement already uses the final size.
            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);

            if (_followRoutine != null)
                StopCoroutine(_followRoutine);

            _followRoutine = StartCoroutine(FollowPosition());
        }

        /// <summary>
        /// Hides the tooltip and stops following the cursor.
        /// </summary>
        public void Hide()
        {
            if (_followRoutine != null)
            {
                StopCoroutine(_followRoutine);
                _followRoutine = null;
            }

            content.SetActive(false);
            _getScreenPosition = null;
        }

        /// <summary>
        /// Repositions the tooltip every frame for as long as it stays visible.
        /// </summary>
        private IEnumerator FollowPosition()
        {
            RectTransform canvasRect = canvas.transform as RectTransform;
            Camera canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : canvas.worldCamera;

            tooltipRect.pivot = TopLeftPivot;

            while (content.activeSelf && _getScreenPosition != null)
            {
                PlaceTooltip(canvasRect, canvasCamera);
                yield return null;
            }

            _followRoutine = null;
        }

        /// <summary>
        /// Moves the tooltip to the resolved position for this frame.
        /// </summary>
        private void PlaceTooltip(RectTransform canvasRect, Camera canvasCamera)
        {
            Vector2 mouse = _getScreenPosition.Invoke();
            Vector2 size = GetTooltipSize(canvasCamera);
            Vector2 pivotScreen = ResolvePivotPosition(mouse, size);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, pivotScreen, canvasCamera,
                out Vector2 localPoint);

            tooltipRect.localPosition = localPoint;
        }

        /// <summary>
        /// Measures the tooltip in screen pixels, using the layout as it stands this frame.
        /// </summary>
        private Vector2 GetTooltipSize(Camera canvasCamera)
        {
            tooltipRect.GetWorldCorners(_corners);
            Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(canvasCamera, _corners[BottomLeftCorner]);
            Vector2 topRight = RectTransformUtility.WorldToScreenPoint(canvasCamera, _corners[TopRightCorner]);

            return new Vector2(Mathf.Abs(topRight.x - bottomLeft.x), Mathf.Abs(topRight.y - bottomLeft.y));
        }

        /// <summary>
        /// Resolves the top left corner in screen space. Prefers below right of the cursor, flips to the
        /// other side when that would overflow, and clamps as a last resort so it stays on screen.
        /// </summary>
        private Vector2 ResolvePivotPosition(Vector2 mouse, Vector2 size)
        {
            float offsetX = Mathf.Abs(screenOffset.x);
            float offsetY = Mathf.Abs(screenOffset.y);

            float left = mouse.x + offsetX;
            if (left + size.x > Screen.width - edgeMargin)
                left = mouse.x - offsetX - size.x;

            float maxLeft = Mathf.Max(edgeMargin, Screen.width - edgeMargin - size.x);
            left = Mathf.Clamp(left, edgeMargin, maxLeft);

            float top = mouse.y - offsetY;
            if (top - size.y < edgeMargin)
                top = mouse.y + offsetY + size.y;

            float minTop = edgeMargin + size.y;
            float maxTop = Mathf.Max(minTop, Screen.height - edgeMargin);
            top = Mathf.Clamp(top, minTop, maxTop);

            return new Vector2(left, top);
        }
    }
}