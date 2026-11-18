using TMPro;

namespace Base.TextPackage
{
    /// <summary>
    /// Finds words a text broke across two lines. Reading "Hoffnungslo" and "s romantisch" stops the eye, so the
    /// <see cref="LineBalancer"/> keeps away from such layouts and the text fit check reports them.
    /// </summary>
    public static class BrokenWords
    {
        private const char Hyphen = '-';
        private const char SoftHyphen = '\u00AD';

        /// <summary>Whether a word of <paramref name="info"/> spans two lines without a hyphen at the break.</summary>
        public static bool TryFind(TMP_TextInfo info, out TMP_WordInfo broken)
        {
            for (int i = 0; i < info.wordCount; i++)
            {
                TMP_WordInfo word = info.wordInfo[i];

                if (!IsBroken(info, word))
                    continue;

                broken = word;
                return true;
            }

            broken = default;
            return false;
        }

        private static bool IsBroken(TMP_TextInfo info, TMP_WordInfo word)
        {
            for (int c = word.firstCharacterIndex; c < word.lastCharacterIndex; c++)
            {
                TMP_CharacterInfo character = info.characterInfo[c];

                if (character.lineNumber != info.characterInfo[c + 1].lineNumber
                    && character.character != Hyphen
                    && character.character != SoftHyphen)
                    return true;
            }

            return false;
        }
    }
}