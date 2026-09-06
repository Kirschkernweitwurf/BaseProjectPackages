using Base.AttributesPackage.Editor.Core;
using NUnit.Framework;
using UnityEngine;

namespace Base.AttributesPackage.Tests
{
    /// <summary>
    /// Covers the paths the folder and file drawers hand back. A picker returns a Windows path with
    /// backslashes and an absolute location, and what gets serialized has to be neither, or the field
    /// stops resolving the moment the project is opened on another machine.
    /// </summary>
    public sealed class PathUtilityTests
    {
        private const string Assets = "Assets";

        /// <summary>Unity speaks in forward slashes wherever the picker came from.</summary>
        [Test]
        public void BackslashesBecomeForwardSlashes() => Assert.That(PathUtility.Normalize(@"Assets\Art\Textures"),
            Is.EqualTo("Assets/Art/Textures"));

        /// <summary>Nothing in means nothing out, not a crash.</summary>
        [Test]
        public void AMissingPathNormalizesToNothing() => Assert.That(PathUtility.Normalize(null), Is.Null);

        /// <summary>
        /// An absolute path inside the project becomes a project path, which is the only form that
        /// survives being committed and opened somewhere else.
        /// </summary>
        [Test]
        public void APathInsideTheProjectBecomesProjectRelative() => Assert.That(
            PathUtility.ToProjectRelative(Application.dataPath + "/Art/Textures"),
            Is.EqualTo("Assets/Art/Textures"));

        /// <summary>The project folder itself is the root, not an empty string.</summary>
        [Test]
        public void TheProjectFolderItselfBecomesTheRoot()
            => Assert.That(PathUtility.ToProjectRelative(Application.dataPath), Is.EqualTo(Assets));

        /// <summary>
        /// A path outside the project cannot be made relative to it, so it comes back as it is rather
        /// than as a project path that points nowhere.
        /// </summary>
        [Test]
        public void APathOutsideTheProjectStaysAbsolute()
            => Assert.That(PathUtility.ToProjectRelative("D:/Elsewhere/Art"), Is.EqualTo("D:/Elsewhere/Art"));

        /// <summary>
        /// What Resources.Load wants is the part after the folder and without the extension, which is
        /// neither what the picker returns nor what the inspector shows.
        /// </summary>
        [Test]
        public void AResourcesPathDropsTheFolderAndTheExtension() => Assert.That(
            PathUtility.ToResourcesPath("Assets/Art/Resources/Icons/Play.png"),
            Is.EqualTo("Icons/Play"));

        /// <summary>
        /// Only the last Resources folder counts, since that is the one Unity loads relative to when
        /// they are nested.
        /// </summary>
        [Test]
        public void TheLastResourcesFolderIsTheOneThatCounts() => Assert.That(
            PathUtility.ToResourcesPath("Assets/Resources/Packs/Resources/Play.png"),
            Is.EqualTo("Play"));

        /// <summary>An asset outside a Resources folder cannot be loaded that way, so there is no path.</summary>
        [Test]
        public void AnAssetOutsideResourcesHasNoResourcesPath()
            => Assert.That(PathUtility.ToResourcesPath("Assets/Art/Icons/Play.png"), Is.Null);

        /// <summary>A path with no extension is already what Resources.Load wants.</summary>
        [Test]
        public void APathWithoutAnExtensionIsLeftAlone()
            => Assert.That(PathUtility.ToResourcesPath("Assets/Resources/Icons/Play"), Is.EqualTo("Icons/Play"));

        /// <summary>Nothing in means nothing out here too.</summary>
        [Test]
        public void AMissingAssetPathHasNoResourcesPath()
        {
            Assert.That(PathUtility.ToResourcesPath(null), Is.Null);
            Assert.That(PathUtility.ToResourcesPath(string.Empty), Is.Null);
        }
    }
}