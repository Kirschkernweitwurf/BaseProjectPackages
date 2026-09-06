using System.IO;

namespace Base.UIPackage.Utility
{
    /// <summary>
    /// The version file the build pipeline writes and the label reads back.
    /// <para>
    /// Split out of <see cref="BuildVersion"/> because the reading, the counting and the formatting are
    /// the only parts of it that can be wrong, and none of them could be reached while they sat behind
    /// a MonoBehaviour pointed at one fixed path.
    /// </para>
    /// <para>
    /// The file is three lines. The first is left alone, so anything a project keeps up there survives
    /// a build. The second is the application version and the third is the build number.
    /// </para>
    /// </summary>
    internal static class BuildVersionFile
    {
        /// <summary>Line the build number sits on.</summary>
        internal const int BuildNumberLineIndex = 2;

        /// <summary>Number the first build of a project gets, when nothing was counted before.</summary>
        internal const int FirstBuildNumber = 1;

        /// <summary>How many lines the file always has, however many the file on disk holds.</summary>
        internal const int LineCount = 3;

        /// <summary>Line the application version sits on.</summary>
        internal const int VersionLineIndex = 1;

        /// <summary>
        /// Reads the file into exactly <see cref="LineCount"/> entries, whether it is missing, short or
        /// long. Every caller indexes into the result, so a shorter array would be an exception waiting
        /// for the first project that has not built yet.
        /// </summary>
        /// <param name="path">Absolute path of the version file.</param>
        /// <returns>The lines, padded with nulls where the file had none.</returns>
        internal static string[] Read(string path)
        {
            string[] versionInfo = new string[LineCount];

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return versionInfo;

            string[] lines = File.ReadAllLines(path);

            for (int i = 0; i < versionInfo.Length && i < lines.Length; i++)
                versionInfo[i] = lines[i];

            return versionInfo;
        }

        /// <summary>
        /// Writes the file, creating the folder it goes in. The streaming assets folder does not exist
        /// in a project that has never put anything in it.
        /// </summary>
        /// <param name="path">Absolute path of the version file.</param>
        /// <param name="versionInfo">The lines to write.</param>
        internal static void Write(string path, string[] versionInfo)
        {
            string directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllLines(path, versionInfo);
        }

        /// <summary>
        /// Records the version of the build being made and counts it. The stored number is read first,
        /// so a file that was never written or that somebody edited by hand starts the count over
        /// rather than stopping the build.
        /// </summary>
        /// <param name="versionInfo">The lines read from the file.</param>
        /// <param name="version">The application version of the build being made.</param>
        /// <returns>The same array, with the version and the build number filled in.</returns>
        internal static string[] Advance(string[] versionInfo, string version)
        {
            int buildNumber = int.TryParse(versionInfo[BuildNumberLineIndex], out int storedBuildNumber)
                ? storedBuildNumber + 1
                : FirstBuildNumber;

            versionInfo[VersionLineIndex] = version;
            versionInfo[BuildNumberLineIndex] = buildNumber.ToString();

            return versionInfo;
        }

        /// <summary>
        /// Builds the label. A project that has never built has nothing to say, so it says nothing
        /// rather than showing an empty pair of brackets on screen.
        /// </summary>
        /// <param name="versionInfo">The lines read from the file.</param>
        /// <returns>The label, or an empty string when the file holds neither value.</returns>
        internal static string Format(string[] versionInfo)
        {
            string version = versionInfo[VersionLineIndex];
            string buildNumber = versionInfo[BuildNumberLineIndex];

            return string.IsNullOrEmpty(version) && string.IsNullOrEmpty(buildNumber)
                ? string.Empty
                : $"{version} [{buildNumber}]";
        }
    }
}