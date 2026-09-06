using System;
using System.Collections.Generic;
using System.Reflection;
using Base.ToolsPackage.Editor.MenuManagerWindows;
using Base.ToolsPackage.Editor.PlayModeApplier;
using NUnit.Framework;
using UnityEditor;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Base.ToolsPackage.Editor.Tests
{
    /// <summary>
    /// Guards the rule that menu entries go through the Menu Manager rather than Unity's own
    /// <see cref="MenuItem"/>, and the three places that cannot follow it.
    /// <para>
    /// The reason those three are exceptions was never written down anywhere, which makes them look
    /// like oversights and invites a cleanup that breaks something. Both reasons are in the files now,
    /// and this is what keeps a fourth from joining them quietly.
    /// </para>
    /// <para>
    /// Only the Tools package is checked. It is the package the Menu Manager ships in, so it is the one
    /// place the rule always applies. A package installed without Tools has no Menu Manager to use, and
    /// the installer deliberately depends on neither, which it says so in its own two exceptions.
    /// </para>
    /// </summary>
    public sealed class MenuItemConventionTests
    {
        private static readonly HashSet<Type> AllowedTypes = new()
        {
            typeof(CreateAssetMenuManagerWindow),
            typeof(MenuItemManagerWindow),
            typeof(PlayModeCapturer)
        };

        private readonly Dictionary<Assembly, string> _packageByAssembly = new();
        private readonly HashSet<Type> _declaringTypes = new();

        private string _toolsPackage;

        /// <summary>Collects every type in this package that declares a Unity menu item.</summary>
        [OneTimeSetUp]
        public void CollectMenuItems()
        {
            _toolsPackage = PackageOf(typeof(MenuItemConventionTests).Assembly);

            if (string.IsNullOrEmpty(_toolsPackage))
                return;

            foreach (MethodInfo method in TypeCache.GetMethodsWithAttribute<MenuItem>())
            {
                Type owner = method.DeclaringType;

                if (owner == null)
                    continue;

                if (string.Equals(PackageOf(owner.Assembly), _toolsPackage, StringComparison.Ordinal))
                    _declaringTypes.Add(owner);
            }
        }

        /// <summary>
        /// The scope is resolved from the assembly this test compiles into, so a package layout that
        /// stops answering to that lookup would leave everything below checking nothing.
        /// </summary>
        [Test]
        public void TheToolsPackageResolves() => Assert.That(_toolsPackage, Is.Not.Empty,
            "this test assembly reports no owning package, so nothing below means anything");

        /// <summary>
        /// A fourth exception is either a mistake or a reason nobody wrote down. Both are worth stopping
        /// at, since an entry the Menu Manager cannot see is one the window cannot move or rename.
        /// </summary>
        [Test]
        public void OnlyTheDocumentedTypesUseUnityMenuItem()
        {
            List<string> unexpected = new();

            foreach (Type owner in _declaringTypes)
            {
                if (!AllowedTypes.Contains(owner))
                    unexpected.Add($"{owner.FullName} declares a Unity menu item. Use the Menu Manager, or "
                        + "record here why it cannot.");
            }

            Assert.That(unexpected, Is.Empty, string.Join(Environment.NewLine, unexpected));
        }

        /// <summary>
        /// The other half of the rule, and the one a cleanup would break. Both manager windows are how a
        /// broken registration gets fixed, so each has to stay reachable through a menu entry that does
        /// not go through the registration it fixes.
        /// </summary>
        [Test]
        public void TheManagerWindowsStayReachableWithoutTheMenuManager()
        {
            Assert.That(_declaringTypes, Has.Member(typeof(MenuItemManagerWindow)));
            Assert.That(_declaringTypes, Has.Member(typeof(CreateAssetMenuManagerWindow)));
        }

        /// <summary>
        /// Resolves the package an assembly ships in, remembering the answer because the scan asks about
        /// the same handful of assemblies hundreds of times.
        /// </summary>
        /// <param name="assembly">The assembly to look up.</param>
        /// <returns>The package name, or an empty string when the assembly ships in none.</returns>
        private string PackageOf(Assembly assembly)
        {
            if (_packageByAssembly.TryGetValue(assembly, out string cached))
                return cached;

            PackageInfo info = PackageInfo.FindForAssembly(assembly);
            string package = info == null
                ? string.Empty
                : info.name;

            _packageByAssembly[assembly] = package;

            return package;
        }
    }
}