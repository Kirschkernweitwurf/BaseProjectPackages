using System;
using System.Collections;
using Systems.Services;
using TMPro;
using UnityEngine;

namespace Systems.Tooltip
{
    /// <summary>
    /// Manages the visual representation of tooltips on the screen.
    /// Responsible for showing, hiding, and positioning the tooltip based on provided data.
    /// </summary>
    [DisallowMultipleComponent]
    public class TooltipView : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Offset from the target screen position to display the tooltip.")]
        [SerializeField] private Vector2 screenOffset = new(15f, -15f);

        [Tooltip("Preferred starting pivot of the tooltip (e.g., (0,0) = top-right of cursor).")]
        [SerializeField] private Vector2 defaultPivot = new(0f, 0f);

        [Tooltip("Distance in pixels from screen edge before flipping to avoid flicker.")]
        [SerializeField] private float edgeMargin = 8f;

        [Header("References")]
        [Tooltip("Canvas that the tooltip is rendered on.")]
        [SerializeField] private Canvas canvas;

        [Tooltip("Content GameObject that contains the tooltip visuals.")]
        [SerializeField] private GameObject content;

        [Tooltip("Text element to display the tooltip message.")]
        [SerializeField] private TextMeshProUGUI textElement;

        [Tooltip("RectTransform of the tooltip for positioning.")]
        [SerializeField] private RectTransform tooltipRect;

        private Func<Vector2> _getScreenPosition;
        private Coroutine _followRoutine;

        private void Start()
        {
            ServiceLocator.Get<TooltipService>()?.SetView(this);
            Hide();
        }

        /// <summary>
        /// Shows the tooltip with the specified data.
        /// </summary>
        public void Show(TooltipData data)
        {
            _getScreenPosition = data.GetScreenPosition;
            textElement.text = data.Message;
            content.SetActive(true);

            if (_followRoutine != null)
                StopCoroutine(_followRoutine);

            _followRoutine = StartCoroutine(FollowPosition());
        }

        /// <summary>
        /// Hides the tooltip.
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
        /// Coroutine that continuously updates tooltip position while visible.
        /// Prefers showing the tooltip above/right of the cursor,
        /// but flips horizontally/vertically if it would go out of view.
        /// </summary>
        private IEnumerator FollowPosition()
        {
            RectTransform canvasRect = canvas.transform as RectTransform;
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            Vector2 pivot = defaultPivot;
            tooltipRect.pivot = pivot;

            while (content.activeSelf && _getScreenPosition != null)
            {
                Vector2 mousePos = _getScreenPosition.Invoke();
                Vector2 offset = new(
                    Mathf.Approximately(pivot.x, 1f) ? -Mathf.Abs(screenOffset.x) : Mathf.Abs(screenOffset.x),
                    Mathf.Approximately(pivot.y, 1f) ? Mathf.Abs(screenOffset.y) : -Mathf.Abs(screenOffset.y)
                );

                Vector2 screenPos = mousePos + offset;

                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, cam, out Vector2 localPoint);
                tooltipRect.localPosition = localPoint;

                Vector3[] corners = new Vector3[4];
                tooltipRect.GetWorldCorners(corners);

                bool flipX = false;
                bool flipY = false;

                foreach (Vector3 c in corners)
                {
                    Vector2 point = RectTransformUtility.WorldToScreenPoint(cam, c);

                    if (point.x < edgeMargin)
                        flipX = true;

                    if (point.x > Screen.width - edgeMargin)
                        flipX = true;

                    if (point.y < edgeMargin)
                        flipY = true;

                    if (point.y > Screen.height - edgeMargin)
                        flipY = true;
                }

                if (flipX || flipY)
                {
                    Vector2 newPivot = pivot;

                    if (flipX)
                        newPivot.x = 1f - pivot.x;

                    if (flipY)
                        newPivot.y = 1f - pivot.y;

                    if (newPivot != pivot)
                    {
                        pivot = newPivot;
                        tooltipRect.pivot = pivot;
                    }
                }

                yield return null;
            }

            _followRoutine = null;
        }
    }
}