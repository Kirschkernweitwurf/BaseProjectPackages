using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Base.ToolPackage.Editor.Tests
{
    /// <summary>
    /// Checks that an assembly compiled out behind a define constraint can be compiled back in.
    /// <para>
    /// The bridge assemblies that join two packages, such as the Settings components shipped by
    /// Localization, Save System and Controller Support, are gated on the package they bridge to and
    /// on nothing else. They reference more than that package, so the gate is only honest as long as
    /// installing the named package also brings the rest along. That holds today, but it holds
    /// through the reference lists of other packages, which nothing stops from changing.
    /// </para>
    /// <para>
    /// A constraint is not the fix for that. Gating on every referenced package would drop the
    /// components from a project silently instead of reporting the package that is missing. So the
    /// guarantee is asserted here instead, where breaking it turns red and names both ends.
    /// </para>
    /// </summary>
    public sealed class AsmdefGuardTests
    {
        private const string AsmdefFilter = "t:AssemblyDefinitionAsset";
        private const string BasePackagePrefix = "com.baseprojectpackages.";
        private const string GuidPrefix = "GUID:";
        private const string TestConstraint = "UNITY_INCLUDE_TESTS";

        // Only assemblies that always compile can carry a guarantee. A gated one may itself be absent,
        // so an edge it declares proves nothing about what installing a package brings with it.
        private readonly Dictionary<string, HashSet<string>> _packageEdges = new(StringComparer.Ordinal);

        private readonly Dictionary<string, string> _packageByAssembly = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _assemblyByGuid = new(StringComparer.Ordinal);
        private readonly HashSet<string> _presentPackages = new(StringComparer.Ordinal);
        private readonly List<RawAsmdef> _gated = new();

        /// <summary>Reads every assembly definition in the project once for the whole suite.</summary>
        [OneTimeSetUp]
        public void LoadAsmdefs()
        {
            List<RawAsmdef> all = new();

            foreach (string guid in AssetDatabase.FindAssets(AsmdefFilter))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                RawAsmdef data = Read(path);

                if (data == null || string.IsNullOrEmpty(data.name))
                    continue;

                string package = PackageOf(path);

                if (string.IsNullOrEmpty(package))
                    continue;

                all.Add(data);

                _assemblyByGuid[guid] = data.name;
                _packageByAssembly[data.name] = package;
                _presentPackages.Add(package);
            }

            foreach (RawAsmdef data in all)
            {
                if (IsGated(data))
                {
                    _gated.Add(data);
                    continue;
                }

                AddPackageEdges(data);
            }
        }

        /// <summary>
        /// The suite is only meaningful while the project actually holds gated assemblies, so a
        /// scan that found none is reported rather than passing quietly.
        /// </summary>
        [Test]
        public void TheProjectHoldsGatedAssemblies() => Assert.That(_gated, Is.Not.Empty,
            "no assembly in this project is behind a define constraint, so nothing below means anything");

        /// <summary>
        /// Every base package an assembly references has to be reachable from the packages its own
        /// gates name, otherwise the constraint lets it compile in a project that cannot build it.
        /// </summary>
        [Test]
        public void EveryGatedAssemblyNamesEnoughPackagesToCompile()
        {
            List<string> failures = new();

            foreach (RawAsmdef data in _gated)
                Inspect(data, failures);

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        private static RawAsmdef Read(string path)
        {
            AssemblyDefinitionAsset asset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(path);

            if (asset == null)
                return null;

            return JsonUtility.FromJson<RawAsmdef>(asset.text);
        }

        private static string PackageOf(string path)
        {
            PackageInfo info = PackageInfo.FindForAssetPath(path);

            if (info == null)
                return null;

            return info.name;
        }

        private static bool IsBasePackage(string package) => !string.IsNullOrEmpty(package)
            && package.StartsWith(BasePackagePrefix, StringComparison.Ordinal);

        // The test constraint is on every test assembly in the project and gates nothing about
        // packages, so an assembly carrying only that one is not treated as gated.
        private static bool IsGated(RawAsmdef data)
        {
            if (data.defineConstraints == null)
                return false;

            foreach (string constraint in data.defineConstraints)
            {
                if (!string.Equals(constraint, TestConstraint, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static bool Names(RawAsmdef data, string define)
        {
            foreach (string constraint in data.defineConstraints)
            {
                if (string.Equals(constraint, define, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private string ResolveReference(string token)
        {
            if (string.IsNullOrEmpty(token)
                || !token.StartsWith(GuidPrefix, StringComparison.Ordinal))
                return token;

            return _assemblyByGuid.GetValueOrDefault(token[GuidPrefix.Length..]);
        }

        private void AddPackageEdges(RawAsmdef data)
        {
            if (data.references == null)
                return;

            string from = _packageByAssembly[data.name];

            foreach (string token in data.references)
            {
                string target = TargetPackage(token);

                if (string.IsNullOrEmpty(target)
                    || string.Equals(target, from, StringComparison.Ordinal))
                    continue;

                if (!_packageEdges.TryGetValue(from, out HashSet<string> targets))
                {
                    targets = new HashSet<string>(StringComparer.Ordinal);
                    _packageEdges[from] = targets;
                }

                targets.Add(target);
            }
        }

        private string TargetPackage(string token)
        {
            string assembly = ResolveReference(token);

            if (string.IsNullOrEmpty(assembly))
                return null;

            return _packageByAssembly.GetValueOrDefault(assembly);
        }

        // The packages the gates promise: the one the assembly ships in, which is always there, plus
        // every package a version define names that the constraints actually require.
        private HashSet<string> Guaranteed(RawAsmdef data)
        {
            HashSet<string> packages = new(StringComparer.Ordinal)
            {
                _packageByAssembly[data.name]
            };

            if (data.versionDefines == null)
                return packages;

            foreach (RawVersionDefine versionDefine in data.versionDefines)
            {
                if (Names(data, versionDefine.define))
                    packages.Add(versionDefine.name);
            }

            return packages;
        }

        private HashSet<string> Closure(IEnumerable<string> seed)
        {
            HashSet<string> reached = new(seed, StringComparer.Ordinal);
            Stack<string> pending = new(reached);

            while (pending.Count > 0)
            {
                if (!_packageEdges.TryGetValue(pending.Pop(), out HashSet<string> targets))
                    continue;

                foreach (string target in targets)
                {
                    if (reached.Add(target))
                        pending.Push(target);
                }
            }

            return reached;
        }

        // A gate naming a package this project does not hold cannot be followed, so the assembly is
        // passed over rather than reported. This is the normal case in a project that installed only
        // part of the stack, and reporting it there would make the suite fail for being incomplete.
        private bool CanFollow(IEnumerable<string> packages)
        {
            foreach (string package in packages)
            {
                if (IsBasePackage(package) && !_presentPackages.Contains(package))
                    return false;
            }

            return true;
        }

        private void Inspect(RawAsmdef data, ICollection<string> failures)
        {
            if (data.references == null)
                return;

            HashSet<string> guaranteed = Guaranteed(data);

            if (!CanFollow(guaranteed))
                return;

            HashSet<string> reachable = Closure(guaranteed);

            foreach (string token in data.references)
            {
                string target = TargetPackage(token);

                if (!IsBasePackage(target) || reachable.Contains(target))
                    continue;

                failures.Add($"{data.name} references {ResolveReference(token)} from {target}, which none "
                    + $"of its gates ({string.Join(", ", data.defineConstraints)}) guarantee. Either gate on "
                    + $"{target} as well, or drop the reference.");
            }
        }

        [Serializable]
        private sealed class RawAsmdef
        {
            public string name;
            public string[] references;
            public string[] defineConstraints;
            public RawVersionDefine[] versionDefines;
        }

        [Serializable]
        private sealed class RawVersionDefine
        {
            public string name;
            public string define;
        }
    }
}