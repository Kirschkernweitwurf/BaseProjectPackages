using System;
using UnityEngine.Localization;

namespace Base.TextPackage
{
    /// <summary>A <see cref="TextStyle"/> picked per language from the Text Styles asset table.</summary>
    [Serializable]
    public sealed class LocalizedTextStyle : LocalizedAsset<TextStyle>
    {
    }
}