using System;
using Base.UtilityPackage.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Base.TextPackage
{
    /// <summary>
    /// Evens out the lines of a wrapped text, like CSS <c>text-wrap: balance</c>: the text gets the narrowest width
    /// that keeps its line count and size and breaks no word, so no line ends far short of the others and no word
    /// sits alone on the last line. Justified text was the other option, but narrow boxes like cards open big gaps
    /// between words. A paragraph in a box of fixed width, like a tooltip, asks for <see cref="ELineFit.Pretty"/>
    /// instead, which keeps the text as wide as it can and only cares about the last line.
    /// </summary>
    /// <remarks>
    /// The width is narrowed through the margins, so the box and its layout stay as they are. The cut is serialized
    /// next to them, because a copy of a live text (like a second tooltip panel) copies the narrowed margins too and
    /// has to know which part of them was authored. A text only balances again when its characters, width or size
    /// change, since color tweens regenerate the mesh every frame.
    /// </remarks>
    [Serializable]
    internal sealed class LineBalancer
    {
        private const int HashFactor = 31;
        private const char LineBreak = '\n';
        private const string LoopWarning = "Stopped balancing a text whose layout keeps changing its width or size.";
        private const int MaxBalancedFrames = 10;
        private const float MinLastLineShare = 1f / 3f;
        private const int MinLastLineWords = 2;
        private const int MinLines = 2;
        private const float NoWidth = -1f;
        private const int QuietFrames = 2;
        private const int SearchSteps = 6;
        private const float SizeTolerance = 0.01f;

        [Tooltip("How far the margins are moved in right now, left and right. Runtime state, zero in saved assets.")]
        [SerializeField] private Vector2 cut;

        private TMP_Text _text;
        private Vector4 _baseMargin;
        private bool _isBalancing;
        private bool _hasSignature;
        private int _signature;
        private int _balancedCharacters;
        private int _balancedFrames;
        private int _lastBalanceFrame;

        /// <summary>Forgets the last layout, so a new style balances the text again.</summary>
        public void Invalidate()
        {
            _hasSignature = false;
            _balancedFrames = 0;
        }

        /// <summary>
        /// Gives <paramref name="text"/> the width <paramref name="fit"/> asks for within its authored margins. Call it
        /// after the text was regenerated; it does nothing while the text looks the same as last time.
        /// </summary>
        public void Balance(TMP_Text text, ELineFit fit)
        {
            _text = text;

            if (_isBalancing
                || !IsWrapping())
                return;

            int signature = CalculateSignature();

            if (_hasSignature
                && signature == _signature)
                return;

            TrackBalance();

            if (IsLooping())
                return;

            Vector4 margin = _text.margin;

            // Its own mesh updates raise the text changed event again.
            _isBalancing = true;
            Narrow(fit);
            _isBalancing = false;

            // TMP keeps its measured size over a margin change. A box that showed a balanced text and then gets a
            // shorter one was measured at the old, narrow width, so a title that fits on one line kept the height
            // of two and left an empty line under it.
            if (_text.margin != margin)
                ForgetMeasuredSize();

            // A layout group sized the box before the text balanced. Not the text's own SetLayoutDirty: raised from
            // inside its mesh update, its dirty flag never clears again and the text then skips every culling
            // check, so a text that was outside a scroll view stays hidden once scrolled in.
            LayoutRebuilder.MarkLayoutForRebuild(_text.rectTransform);
        }

        private bool IsWrapping() => _text.textWrappingMode == TextWrappingModes.Normal
            || _text.textWrappingMode == TextWrappingModes.PreserveWhitespace;

        // Balancing moves the margins and a layout that sizes the text by them can answer with a new width or size.
        // Frames in a row of the same text mean the two keep undoing each other, so the balancer gives way: with the
        // margins left alone the layout settles and uneven lines beat a text that jitters. A window being resized
        // can trip it too, which only leaves the lines a little uneven until the text next changes.
        private void TrackBalance()
        {
            int characters = CalculateCharacterHash();
            int frame = Time.frameCount;

            if (characters != _balancedCharacters
                || frame - _lastBalanceFrame > QuietFrames)
                _balancedFrames = 0;

            if (frame != _lastBalanceFrame
                || _balancedFrames == 0)
                _balancedFrames++;

            _balancedCharacters = characters;
            _lastBalanceFrame = frame;

            if (_balancedFrames == MaxBalancedFrames + 1)
                CustomLogger.LogWarning(LoopWarning, _text);
        }

        private bool IsLooping() => _balancedFrames > MaxBalancedFrames;

        private int CalculateSignature()
        {
            int hash = Mathf.RoundToInt(_text.rectTransform.rect.width);

            // The size the layout asks for, not the shrunk one. A squeezed layout group sizes an auto-sized text by
            // its margins and the text's size by its box, so balancing on every shrink flips between two layouts
            // each frame and the whole column jitters.
            float size = _text.enableAutoSizing
                ? _text.fontSizeMax
                : _text.fontSize;
            hash = hash * HashFactor + Mathf.RoundToInt(size / SizeTolerance);
            return hash * HashFactor + CalculateCharacterHash();
        }

        private int CalculateCharacterHash()
        {
            TMP_TextInfo info = _text.textInfo;
            int hash = 0;

            for (int i = 0; i < info.characterCount; i++)
                hash = hash * HashFactor + info.characterInfo[i].character;

            return hash;
        }

        private void Narrow(ELineFit fit)
        {
            Vector4 margin = _text.margin;
            _baseMargin = new Vector4(margin.x - cut.x, margin.y, margin.z - cut.y, margin.w);
            SetCut(0f);
            _signature = CalculateSignature();
            _hasSignature = true;

            if (!HasSoftWraps())
                return;

            int lines = _text.textInfo.lineCount;
            float size = _text.fontSize;
            bool isTruncated = _text.isTextTruncated;
            bool isWordBroken = BrokenWords.TryFind(_text.textInfo, out _);
            float fullWidth = _text.rectTransform.rect.width - _baseMargin.x - _baseMargin.z;

            float best = fit == ELineFit.Pretty
                ? FindPrettyWidth(fullWidth, lines, size, isTruncated, isWordBroken)
                : FindBalancedWidth(fullWidth, lines, size, isTruncated, isWordBroken);

            SetCut(fullWidth - best);
        }

        private float FindBalancedWidth(float fullWidth, int lines, float size, bool isTruncated, bool isWordBroken)
        {
            // No layout of the same lines can be narrower than their average length.
            float fits = fullWidth;
            float fails = CalculateAverageLength();

            for (int i = 0; i < SearchSteps; i++)
            {
                float width = (fits + fails) / 2f;
                SetCut(fullWidth - width);

                if (IsSameLayout(lines, size, isTruncated, isWordBroken))
                    fits = width;
                else
                    fails = width;
            }

            return fits;
        }

        // The widest layout of the same lines whose last line reads as a line. Going narrower only hands the box more
        // empty room. A last line no width can fix, like one after a hard break, leaves the text at full width.
        private float FindPrettyWidth(float fullWidth, int lines, float size, bool isTruncated, bool isWordBroken)
        {
            if (!IsLastLineShort(fullWidth))
                return fullWidth;

            float tooWide = fullWidth;
            float tooNarrow = CalculateAverageLength();
            float best = NoWidth;

            for (int i = 0; i < SearchSteps; i++)
            {
                float width = (tooWide + tooNarrow) / 2f;
                SetCut(fullWidth - width);

                if (!IsSameLayout(lines, size, isTruncated, isWordBroken))
                {
                    tooNarrow = width;
                }
                else if (IsLastLineShort(width))
                {
                    tooWide = width;
                }
                else
                {
                    best = width;
                    tooNarrow = width;
                }
            }

            return best == NoWidth
                ? fullWidth
                : best;
        }

        private bool IsLastLineShort(float width)
        {
            TMP_TextInfo info = _text.textInfo;
            TMP_LineInfo last = info.lineInfo[info.lineCount - 1];

            return last.wordCount < MinLastLineWords
                || last.length < width * MinLastLineShare;
        }

        // Only lines the box broke can get even; a text of hard breaks alone keeps its width.
        private bool HasSoftWraps()
        {
            TMP_TextInfo info = _text.textInfo;

            if (info.lineCount < MinLines)
                return false;

            for (int i = 0; i < info.lineCount - 1; i++)
            {
                int last = info.lineInfo[i].lastCharacterIndex;

                if (info.characterInfo[last].character != LineBreak)
                    return true;
            }

            return false;
        }

        private float CalculateAverageLength()
        {
            TMP_TextInfo info = _text.textInfo;
            float total = 0f;

            for (int i = 0; i < info.lineCount; i++)
                total += info.lineInfo[i].length;

            return total / info.lineCount;
        }

        // A word already broken at full width or a language without spaces, has no better layout to protect.
        private bool IsSameLayout(int lines, float size, bool isTruncated, bool isWordBroken) =>
            _text.textInfo.lineCount == lines
            && Mathf.Abs(_text.fontSize - size) < SizeTolerance
            && _text.isTextTruncated == isTruncated
            && (isWordBroken || !BrokenWords.TryFind(_text.textInfo, out _));

        // Takes the cut from the side the text does not align to, so its anchor edge or center stays put.
        private void SetCut(float amount)
        {
            float left = _text.horizontalAlignment switch
            {
                HorizontalAlignmentOptions.Right => amount,
                HorizontalAlignmentOptions.Center or HorizontalAlignmentOptions.Geometry => amount / 2f,
                _ => 0f
            };

            cut = new Vector2(left, amount - left);
            _text.margin = new Vector4(_baseMargin.x + cut.x, _baseMargin.y, _baseMargin.z + cut.y, _baseMargin.w);
            _text.ForceMeshUpdate();
        }

        // Asking TMP for its preferred values marks its cached ones stale, the only way to do that without
        // SetLayoutDirty (see Balance), so the next layout pass measures the text at its new width.
        private void ForgetMeasuredSize() => _text.GetPreferredValues();
    }
}