using System.Collections.Generic;
using UnityEngine;

namespace Base.TextPackage
{
    /// <summary>
    /// Makes text boxes of the same kind below it share one size, so frames that sit side by side look even: when one
    /// text has to shrink to fit, every box with the same style shrinks with it. Used where frames form a set, like
    /// a row of signs or the cards in a hand.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TextSizeGroup : MonoBehaviour
    {
        private readonly List<LocalizedTextBox> _members = new();
        private readonly Dictionary<TextStyle, float> _smallest = new();

        private bool _isDirty;

#region Unity Callbacks
        // Late, so every box that changed this frame has its new text and style before the sizes are compared.
        private void LateUpdate()
        {
            if (!_isDirty)
                return;

            _isDirty = false;
            Even();
        }
#endregion

        /// <summary>Compares the sizes again at the end of the frame, after a box got a new text or style.</summary>
        public void MarkDirty() => _isDirty = true;

        private void Even()
        {
            GetComponentsInChildren(false, _members);
            _smallest.Clear();

            foreach (LocalizedTextBox member in _members)
            {
                TextStyle style = member.AppliedStyle;
                if (style == null)
                    continue;

                float size = member.FitAlone();
                if (!_smallest.TryGetValue(style, out float smallest)
                    || size < smallest)
                    _smallest[style] = size;
            }

            foreach (LocalizedTextBox member in _members)
            {
                TextStyle style = member.AppliedStyle;
                if (style != null
                    && _smallest.TryGetValue(style, out float size))
                    member.LockSize(size);
            }
        }
    }
}