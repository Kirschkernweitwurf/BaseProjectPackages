using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Base.ToolPackage.Editor.Tests
{
    /// <summary>
    /// Checks that no base package source holds a <c>GUIStyle</c> in a static without a way to drop it
    /// again.
    /// <para>
    /// A style copies its colors out of <c>EditorStyles</c> at the moment it is built and does not stay
    /// linked to them. Cached in a static it therefore outlives the theme it was built for, because
    /// domain reload is disabled and nothing else clears it. The window then draws the previous theme's
    /// colors until the next recompile, which looks like a rendering glitch rather than a stale cache.
    /// That is why this kept being found by reading files instead of being noticed while using them.
    /// </para>
    /// <para>
    /// The guard is either the shared <c>EditorStyleWatch</c>, or a direct skin comparison for the
    /// packages that sit below EditorUi and cannot reach it. Both leave a mark in the source, which is
    /// what this looks for. A text check is the right shape here: the fault is a missing rebuild path,
    /// and a path that was never written is not something the compiled assembly can be asked about.
    /// </para>
    /// </summary>
    public sealed class StyleCacheGuardTests
    {
        private const string BasePackagePrefix = "com.baseprojectpackages.";
        private const string ScriptExtension = ".cs";
        private const string ScriptFilter = "t:MonoScript";

        // A style assigned into a static field the first time it is asked for. Deliberately loose on
        // the right hand side: the packages build one in place, through a named factory, and through a
        // wrapper that pins its colors, and naming those three would miss the fourth.
        private static readonly Regex CachePattern =
            new(@"_\w+\s*\?\?=[^;]*GUIStyle", RegexOptions.Compiled);

        private static readonly Regex GuardPattern =
            new(@"EditorStyleWatch|EditorStyleSet|isProSkin", RegexOptions.Compiled);

        private readonly List<MonoScript> _scripts = new();

        /// <summary>
        /// The suite only means something while it is actually reading sources, so a scan that reached
        /// nothing is reported rather than passing on an empty set.
        /// </summary>
        [Test]
        public void TheScanReachesBasePackageSources()
        {
            Collect();

            Assert.That(_scripts, Is.Not.Empty,
                "no base package source was read, so nothing below means anything");
        }

        /// <summary>
        /// Every source that caches a style in a static has to name one of the guards, otherwise the
        /// cache survives a theme change it was never rebuilt for.
        /// </summary>
        [Test]
        public void EveryStaticStyleCacheCanBeDropped()
        {
            Collect();

            List<string> failures = new();

            foreach (MonoScript script in _scripts)
                Inspect(script, failures);

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        private static bool IsBasePackageSource(string path)
        {
            if (!path.EndsWith(ScriptExtension, StringComparison.Ordinal))
                return false;

            PackageInfo info = PackageInfo.FindForAssetPath(path);

            if (info == null)
                return false;

            return info.name.StartsWith(BasePackagePrefix, StringComparison.Ordinal);
        }

        private static void Inspect(MonoScript script, ICollection<string> failures)
        {
            string source = script.text;

            if (!CachePattern.IsMatch(source)
                || GuardPattern.IsMatch(source))
                return;

            failures.Add($"{AssetDatabase.GetAssetPath(script)} caches a GUIStyle in a static but names "
                + "no guard, so it keeps the colors of the theme it was built for. Drop the cache from "
                + "an EditorStyleWatch, or compare against EditorGUIUtility.isProSkin where EditorUi is "
                + "out of reach.");
        }

        private void Collect()
        {
            if (_scripts.Count > 0)
                return;

            foreach (string guid in AssetDatabase.FindAssets(ScriptFilter))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (!IsBasePackageSource(path))
                    continue;

                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                if (script != null)
                    _scripts.Add(script);
            }
        }
    }
}