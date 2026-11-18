using System;
using TMPro;
using UnityEngine;

namespace Base.UIPackage.Effects
{
    /// <summary>
    /// Bends a TextMesh Pro text into the tilt of the <see cref="UIPerspective"/> above it and hides it while the
    /// front faces away. TMP builds its mesh itself and skips mesh effects, so this keeps the flat vertices of the
    /// last build and projects a copy of them whenever the tilt changes, without laying the text out again. The
    /// perspective adds it to every text on its own.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Text))]
    public sealed class UIPerspectiveText : MonoBehaviour
    {
        private TMP_Text _text;
        private UIPerspective _perspective;
        private Vector3[][] _flat = Array.Empty<Vector3[]>();
        private bool _hasFlat;

#region Unity Callbacks
        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            _perspective = GetComponentInParent<UIPerspective>(includeInactive: true);
        }

        // A text laid out before the hook existed builds once more, so its flat vertices get kept.
        private void OnEnable()
        {
            _text.OnPreRenderText += OnPreRenderText;
            _text.havePropertiesChanged = true;
            _text.SetVerticesDirty();
        }

        private void OnDisable()
        {
            _text.OnPreRenderText -= OnPreRenderText;
            _hasFlat = false;
        }
#endregion

        /// <summary>Projects the text with the current tilt and uploads it.</summary>
        public void Refresh()
        {
            if (!_hasFlat
                || !isActiveAndEnabled)
                return;

            TMP_TextInfo info = _text.textInfo;
            for (int index = 0; index < info.materialCount; index++)
                Project(_flat[index], info.meshInfo[index].vertices);

            _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }

        // Runs after every layout of the text, before TMP uploads it, so the new flat vertices are kept and bent.
        private void OnPreRenderText(TMP_TextInfo info)
        {
            if (_flat.Length < info.materialCount)
                Array.Resize(ref _flat, info.materialCount);

            for (int index = 0; index < info.materialCount; index++)
            {
                Vector3[] vertices = info.meshInfo[index].vertices;
                if (vertices == null)
                    continue;

                if (_flat[index] == null
                    || _flat[index].Length != vertices.Length)
                    _flat[index] = new Vector3[vertices.Length];

                // TMP clears the unused tail of a sub mesh (like the bold one of keywords) only after this event. It
                // can still hold glyphs of a longer earlier layout, which a later refresh would draw again.
                int used = info.meshInfo[index].vertexCount;
                Array.Copy(vertices, _flat[index], used);
                Array.Clear(_flat[index], used, vertices.Length - used);
                Project(_flat[index], vertices);
            }

            _hasFlat = true;
        }

        // A text facing away collapses to a point, which draws nothing.
        private void Project(Vector3[] flat, Vector3[] target)
        {
            if (flat == null
                || target == null
                || flat.Length != target.Length
                || _perspective == null)
                return;

            if (!_perspective.IsSideShown(isBackSide: false))
            {
                Array.Clear(target, 0, target.Length);
                return;
            }

            if (_perspective.IsFlat)
            {
                Array.Copy(flat, target, flat.Length);
                return;
            }

            UIPerspectiveProjection projection = _perspective.GetProjection(transform, isBackSide: false);
            for (int index = 0; index < flat.Length; index++)
                target[index] = projection.Project(flat[index]);
        }
    }
}