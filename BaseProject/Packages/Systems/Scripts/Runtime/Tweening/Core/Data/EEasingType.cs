namespace Systems.Tweening.Core.Data
{
    /// <summary>
    /// Common easing types for tweens.
    /// See https://easings.net/ for visualizations.
    /// </summary>
    public enum EEasingType : byte
    {
        Linear = 0,
        EaseInQuad = 1,
        EaseOutQuad = 2,
        EaseInOutQuad = 3,
        EaseOutBack = 4,
        EaseInExpo = 5,
        EaseOutExpo = 6,
        EaseInOut = 7,
        EaseInOutCubic = 8,
        EaseInOutExpo = 9,
        EaseInBounce = 10,
        EaseOutBounce = 11,
        EaseInElastic = 12,
        EaseOutElastic = 13
    }
}