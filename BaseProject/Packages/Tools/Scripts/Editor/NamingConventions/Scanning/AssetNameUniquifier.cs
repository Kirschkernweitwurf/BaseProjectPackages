using System.Globalization;
using System.IO;
using UnityEditor;

namespace Base.ToolPackage.Editor.NamingConventions.Scanning
{
    /// <summary>
    /// Keeps a suggested file name free of collisions. Renaming onto an existing asset fails, so
    /// the number at the end is bumped until the folder has room, and the user sees the final name
    /// in the suggestion instead of running into an error.
    /// </summary>
    public static class AssetNameUniquifier
    {
        private const int DefaultDigits = 2;
        private const string DigitFormatPrefix = "D";
        private const int MaxAttempts = 999;
        private const char NumberSeparator = '_';

        /// <summary>Returns the suggestion, bumped until no other asset in the folder uses it.</summary>
        public static string MakeUnique(string assetPath, string suggestion, int digits)
        {
            if (string.IsNullOrEmpty(suggestion))
                return suggestion;

            string folder = FolderOf(assetPath);
            string extension = Path.GetExtension(assetPath);
            string candidate = suggestion;

            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                if (IsFree(folder, candidate, extension, assetPath))
                    return candidate;

                candidate = Bump(candidate, digits);
            }

            return suggestion;
        }

        private static string FolderOf(string assetPath)
        {
            string folder = Path.GetDirectoryName(assetPath);

            return string.IsNullOrEmpty(folder)
                ? string.Empty
                : folder.Replace('\\', '/');
        }

        private static bool IsFree(string folder, string candidate, string extension, string assetPath)
        {
            string path = folder.Length > 0
                ? folder + "/" + candidate + extension
                : candidate + extension;

            // The asset itself is not a collision, otherwise a name that only changes its casing
            // could never be suggested.
            if (path == assetPath)
                return true;

            return AssetDatabase.LoadMainAssetAtPath(path) == null;
        }

        /// <summary>Raises the trailing number by one, or appends one when there is none yet.</summary>
        private static string Bump(string candidate, int digits)
        {
            int length = digits > 0
                ? digits
                : DefaultDigits;

            if (!AssetNameEvaluator.TrySplitEnumeration(candidate, out string core, out string number))
                return candidate
                    + NumberSeparator
                    + 1.ToString(DigitFormatPrefix + length,
                        CultureInfo.InvariantCulture);

            if (!int.TryParse(number, NumberStyles.None, CultureInfo.InvariantCulture, out int value))
                return candidate
                    + NumberSeparator
                    + 1.ToString(DigitFormatPrefix + length,
                        CultureInfo.InvariantCulture);

            int width = digits > 0
                ? digits
                : number.Length;

            return core
                + NumberSeparator
                + (value + 1).ToString(DigitFormatPrefix + width,
                    CultureInfo.InvariantCulture);
        }
    }
}