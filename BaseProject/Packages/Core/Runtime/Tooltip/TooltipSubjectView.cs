using UnityEngine;

namespace Base.CorePackage.Tooltip
{
    /// <summary>
    /// Draws the picture of a <see cref="TooltipLink"/>, like the face of a card a rule deals. The project knows what
    /// its subjects look like, so it derives from this class and puts its component on the picture inside the view's
    /// <see cref="TooltipLinkGroup"/>.
    /// </summary>
    public abstract class TooltipSubjectView : MonoBehaviour
    {
        /// <summary>Shows <paramref name="subject"/>.</summary>
        /// <param name="subject">What the link shows. Never null.</param>
        /// <returns><c>true</c> when it could draw the subject; the picture hides otherwise.</returns>
        public abstract bool TryShow(object subject);
    }
}