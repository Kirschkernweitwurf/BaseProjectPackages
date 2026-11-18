using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Base.UIPackage.Effects
{
    /// <summary>
    /// Tilts a piece of UI in 3D with real perspective, like a card turned in the hand. An overlay canvas draws
    /// everything flat, so a rotation around x or y only squashes the UI; this bends the meshes of every graphic and
    /// text below it instead. Each graphic draws only while its side faces the eye, so a card can spin and show a
    /// back (a child with a <see cref="UIPerspectiveGraphic"/> marked as back side). The roll stays on the transform.
    /// A child's Pos Z sets its depth: a positive z sits behind the UI and shifts against it while it tilts, for a
    /// parallax layer and still shows at its laid out size while flat. Graphics spawned later are picked up the next
    /// time the tilt leaves rest.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class UIPerspective : MonoBehaviour
    {
        private static readonly Quaternion BackSideTurn = Quaternion.Euler(0f, 180f, 0f);

        [Min(0.5f)]
        [Tooltip("How far the eye sits in front of the UI, in heights of this rect. Smaller is a stronger perspective; "
            + "around 3 looks like a card held in front of you.")]
        [SerializeField] private float distance = 3f;

        private readonly List<UIPerspectiveGraphic> _graphics = new();
        private readonly List<UIPerspectiveText> _texts = new();
        private readonly List<Graphic> _found = new();

        private RectTransform _rect;
        private Vector2 _tilt;
        private float _twist;
        private bool _isDirty;

        /// <summary>
        /// Pitch around x and yaw around y in degrees. Positive pitch tips the top edge away, positive yaw turns the
        /// right edge away. Zero is flat; a yaw of 180 shows the back.
        /// </summary>
        public Vector2 Tilt
        {
            get => _tilt;
            set
            {
                if (value == _tilt)
                    return;

                if (IsFlat)
                    CollectGraphics();

                _tilt = value;
                _isDirty = true;
            }
        }

        /// <summary>
        /// Degrees the UI turns around its own vertical axis before the tilt, like a card lying tipped back on a table
        /// that turns over or lifts one long edge off it. Positive turns the right edge away, like the yaw; 180 shows
        /// the back. Zero by default.
        /// </summary>
        public float Twist
        {
            get => _twist;
            set
            {
                if (Mathf.Approximately(value, _twist))
                    return;

                if (IsFlat)
                    CollectGraphics();

                _twist = value;
                _isDirty = true;
            }
        }

        /// <summary>Whether the UI lies flat, so its front graphics draw unchanged.</summary>
        public bool IsFlat => _tilt == Vector2.zero
            && _twist == 0f;

        /// <summary>Whether the back of the UI faces the eye, so only the back side draws.</summary>
        public bool IsFacingAway => (Rotation * Vector3.back).z > 0f;

        // Unity turns a positive y angle toward the eye, so the yaw and the twist are flipped to match their
        // descriptions. The twist turns the UI in its own plane's frame first, the tilt then tips it.
        private Quaternion Rotation => Quaternion.Euler(_tilt.x, -_tilt.y, 0f)
            * Quaternion.AngleAxis(-_twist, Vector3.up);

#region Unity Callbacks
        private void Awake()
        {
            _rect = (RectTransform)transform;
            CollectGraphics();
        }

        // Meshes rebuild after LateUpdate, so a tilt set in any Update this frame is drawn this frame.
        private void LateUpdate()
        {
            if (!_isDirty)
                return;

            _isDirty = false;

            foreach (UIPerspectiveGraphic graphic in _graphics)
                graphic.Refresh();

            foreach (UIPerspectiveText text in _texts)
                text.Refresh();
        }
#endregion

        /// <summary>Whether a graphic on the given side is drawn at the current tilt.</summary>
        public bool IsSideShown(bool isBackSide) => isBackSide == IsFacingAway;

        /// <summary>Returns the projection for the vertices of <paramref name="source"/>, a graphic below.</summary>
        public UIPerspectiveProjection GetProjection(Transform source, bool isBackSide)
        {
            Quaternion rotation = isBackSide
                ? Rotation * BackSideTurn
                : Rotation;

            return new UIPerspectiveProjection(_rect, source, rotation, distance * _rect.rect.height);
        }

        private static void AddHook<T>(Graphic graphic, List<T> hooks) where T : Component
        {
            if (!graphic.TryGetComponent(out T hook))
                hook = graphic.gameObject.AddComponent<T>();

            if (!hooks.Contains(hook))
                hooks.Add(hook);
        }

        // Adds the mesh hooks to every graphic below, also ones spawned since the last tilt, like mood badges.
        private void CollectGraphics()
        {
            GetComponentsInChildren(includeInactive: true, _found);

            foreach (Graphic graphic in _found)
            {
                if (graphic is TMP_Text)
                    AddHook(graphic, _texts);
                else
                    AddHook(graphic, _graphics);
            }

            _found.Clear();
        }
    }
}