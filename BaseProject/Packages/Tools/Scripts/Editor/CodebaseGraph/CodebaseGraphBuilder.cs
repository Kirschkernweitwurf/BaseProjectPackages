using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Base.ToolPackage.Editor.CodebaseGraph.Analysis;
using Base.ToolPackage.Editor.CodebaseGraph.Model;
using Base.ToolPackage.Editor.CodebaseGraph.Scanning;

namespace Base.ToolPackage.Editor.CodebaseGraph
{
    /// <summary>Runs the whole scan and returns the finished graph.</summary>
    internal static class CodebaseGraphBuilder
    {
        private const float AnalyzeProgress = 0.96f;
        private const float AssetProgress = 0.88f;
        private const float CollectProgress = 0.1f;

        private const BindingFlags DeclaredMembers = BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.DeclaredOnly;

        private const string EditorFolderMarker = "/Editor/";
        private const string EditorSuffix = ".Editor";
        private const string GeneratedFolderMarker = "/Generated/";
        private const string GeneratedReason = "Generated source";
        private const float MembersProgress = 0.25f;
        private const float PathsProgress = 0.8f;
        private const int ProgressStepInterval = 100;
        private const string SampleReason = "Sample, showcase or test code";
        private const float SourceTextProgress = 0.85f;
        private const string TestAssemblyReason = "Test assembly";
        private const float UsageProgress = 0.35f;
        private const float UsageProgressSpan = 0.45f;

        /// <summary>Path or namespace segments that mark code as fixtures rather than production.</summary>
        private static readonly string[] ExcludedSegments =
        {
            "/Fixtures/",
            "/Samples/",
            "/Showcase/",
            "/Tests/",
            "Samples~/",
            ".Fixtures",
            ".Samples",
            ".Showcase",
            ".Tests"
        };

        /// <summary>Scans the project and returns a fully analyzed graph.</summary>
        /// <param name="onProgress">
        /// Called with a normalized progress value and a status line. Returning false cancels the scan.
        /// </param>
        /// <param name="includeExcludedScopes">
        /// True to analyze generated, sample and test code as well, which only the tool's own tests ask
        /// for.
        /// </param>
        /// <returns>The finished graph, or null when the scan was canceled.</returns>
        public static CodebaseGraphData Build(Func<float, string, bool> onProgress,
            bool includeExcludedScopes = false)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            CodebaseGraphData graph = new();

            if (!Report(onProgress, 0f, "Resolving assemblies"))
                return null;

            HashSet<string> packageAssemblies = new();
            HashSet<string> testAssemblies = new();
            List<Assembly> assemblies = ProjectAssemblyResolver.Resolve(packageAssemblies, testAssemblies);

            foreach (Assembly assembly in assemblies)
                graph.ScannedAssemblies.Add(assembly.GetName().Name);

            graph.PackageAssemblies.UnionWith(packageAssemblies);

            if (!Report(onProgress, CollectProgress, "Collecting types"))
                return null;

            List<Type> declaredTypes = new();
            List<Type> generatedTypes = new();
            TypeCollector.Collect(assemblies, declaredTypes, generatedTypes);

            if (!Report(onProgress, MembersProgress, "Collecting members"))
                return null;

            MemberCollector.ResetCaches();
            MemberRegistry registry = new(graph.Members);
            CollectTypes(declaredTypes, registry, graph, packageAssemblies, testAssemblies);
            RegisterGeneratedRedirects(declaredTypes, generatedTypes, registry);

            GraphUsageSink sink = new(graph, registry);
            if (!ScanUsages(declaredTypes, generatedTypes, registry, sink, onProgress))
                return null;

            if (!Report(onProgress, PathsProgress, "Reading scripts"))
                return null;

            ScriptIndex index = ScriptPathResolver.Build(ScriptPathResolver.CollectPaths(),
                new ScanProgress(onProgress, PathsProgress, SourceTextProgress - PathsProgress));

            if (index == null)
                return null;

            ApplyScriptPaths(graph, index);

            if (!Report(onProgress, SourceTextProgress, "Matching inlined constants"))
                return null;

            SourceTextScanner.Scan(graph, index);
            index.ReleaseSources();

            if (!Report(onProgress, AssetProgress, "Checking prefabs and scenes"))
                return null;

            // The asset pass has to run first. It marks methods and types that only the YAML knows are
            // reachable, and the analyzer reads exactly that when deciding what is dead, so running it
            // afterward would leave every one of those marks with nothing left to affect.
            if (!SerializedFieldAssetScanner.Scan(graph,
                    new ScanProgress(onProgress, AssetProgress, AnalyzeProgress - AssetProgress)))
                return null;

            if (!Report(onProgress, AnalyzeProgress, "Analyzing"))
                return null;

            BuildNamespaceRelations(graph);
            ComputeMetrics(graph);
            CodebaseGraphAnalyzer.Analyze(graph, includeExcludedScopes);

            stopwatch.Stop();
            graph.ScanSeconds = (float)stopwatch.Elapsed.TotalSeconds;
            return graph;
        }

        private static void CollectTypes(List<Type> declaredTypes,
            MemberRegistry registry,
            CodebaseGraphData graph,
            HashSet<string> packageAssemblies,
            HashSet<string> testAssemblies)
        {
            foreach (Type type in declaredTypes)
            {
                TypeNodeInfo node = MemberCollector.Collect(type, registry);
                if (node == null)
                    continue;

                node.DismissalId = GraphIdentity.ForType(node);
                node.IsPackageAssembly = packageAssemblies.Contains(node.AssemblyName);

                if (testAssemblies.Contains(node.AssemblyName))
                {
                    node.IsExcludedFromFindings = true;
                    node.ExclusionReason = TestAssemblyReason;
                }

                graph.Types[node.Key] = node;

                foreach (MemberNodeInfo member in node.Members)
                    member.DismissalId = GraphIdentity.ForMember(node, member);

                if (!graph.Namespaces.TryGetValue(node.Namespace, out NamespaceNodeInfo group))
                {
                    group = new NamespaceNodeInfo(node.Namespace)
                    {
                        DismissalId = GraphIdentity.ForNamespace(node.Namespace)
                    };

                    graph.Namespaces[node.Namespace] = group;
                }

                group.Types.Add(node);
            }
        }

        private static void RegisterGeneratedRedirects(List<Type> declaredTypes,
            List<Type> generatedTypes,
            MemberRegistry registry)
        {
            foreach (Type type in declaredTypes)
                RedirectGeneratedMethods(type, type, registry);

            foreach (Type type in generatedTypes)
            {
                Type owner = TypeCollector.FindDeclaringWrittenType(type);
                if (owner == null)
                    continue;

                RedirectGeneratedMethods(type, owner, registry);
            }
        }

        private static void RedirectGeneratedMethods(Type container, Type owner, MemberRegistry registry)
        {
            if (!KeyFactory.TryForType(owner, out TypeKey ownerKey))
                return;

            CompilerGeneratedNameResolver.TryResolveOwnerName(container.Name, out string typeOwnerName);

            foreach (MethodBase method in EnumerateMethods(container))
            {
                if (!CompilerGeneratedNameResolver.IsGeneratedName(method.Name)
                    && string.IsNullOrEmpty(typeOwnerName))
                    continue;

                if (!KeyFactory.TryForMember(method, out MemberKey key))
                    continue;

                string ownerName =
                    CompilerGeneratedNameResolver.TryResolveOwnerName(method.Name, out string fromMethod)
                        ? fromMethod
                        : typeOwnerName;

                if (TryFindOwner(registry, ownerKey, ownerName, out MemberKey target))
                    registry.Redirect(key, target);
            }
        }

        /// <summary>
        /// Finds the member a piece of machinery belongs to. The name is tried as written first, then
        /// with an accessor prefix stripped, because a lambda inside a property getter names get_Order
        /// as its owner while only Order was ever registered.
        /// <br/><br/>
        /// The order matters and is not merely tidy. A method may be called get_Order by hand, and an
        /// accessor is only left unregistered when a property actually owns it, so a hand written
        /// get_Order is registered under that exact name. Stripping first would redirect its usages to
        /// an Order property that does not exist, and every fixture would still pass, because none of
        /// them has a method named after an accessor it does not own.
        /// </summary>
        private static bool TryFindOwner(MemberRegistry registry,
            TypeKey ownerKey,
            string ownerName,
            out MemberKey target)
        {
            if (registry.TryFindByName(ownerKey, ownerName, out target))
                return true;

            return CompilerGeneratedNameResolver.TryGetAccessorOwner(ownerName, out string accessorOwner)
                && registry.TryFindByName(ownerKey, accessorOwner, out target);
        }

        private static IEnumerable<MethodBase> EnumerateMethods(Type type)
        {
            foreach (MethodInfo method in type.GetMethods(DeclaredMembers))
                yield return method;

            foreach (ConstructorInfo constructor in type.GetConstructors(DeclaredMembers))
                yield return constructor;
        }

        private static bool ScanUsages(List<Type> declaredTypes,
            List<Type> generatedTypes,
            MemberRegistry registry,
            GraphUsageSink sink,
            Func<float, string, bool> onProgress)
        {
            int total = declaredTypes.Count + generatedTypes.Count;
            int done = 0;
            TokenResolutionCache cache = new();

            foreach (Type type in declaredTypes)
            {
                HierarchyScanner.ScanType(type, registry, sink);
                SignatureScanner.ScanType(type, sink);
                AttributeUsageScanner.ScanType(type, registry, sink);
                IlUsageScanner.ScanType(type, registry, cache, sink);
                done++;

                if (!ReportStep(onProgress, done, total))
                    return false;
            }

            foreach (Type type in generatedTypes)
            {
                IlUsageScanner.ScanType(type, registry, cache, sink);
                done++;

                if (!ReportStep(onProgress, done, total))
                    return false;
            }

            return true;
        }

        private static bool ReportStep(Func<float, string, bool> onProgress, int done, int total)
        {
            if (total == 0 || done % ProgressStepInterval != 0)
                return true;

            float progress = UsageProgress + UsageProgressSpan * (done / (float)total);
            return Report(onProgress, progress, $"Reading method bodies {done} / {total}");
        }

        /// <summary>
        /// Marks the namespaces an editor window lives in. A public member of a type sitting beside a
        /// window has no caller outside the project, whatever its access says, because nobody installs
        /// a package to reach the row model of somebody else's window.
        /// </summary>
        private static void MarkWindowNamespaces(CodebaseGraphData graph)
        {
            HashSet<string> windowNamespaces = new(StringComparer.Ordinal);

            foreach (TypeNodeInfo type in graph.Types.Values)
            {
                if (type.IsEditorWindow)
                    windowNamespaces.Add(type.Namespace);
            }

            foreach (TypeNodeInfo type in graph.Types.Values)
                type.IsWindowOwned = windowNamespaces.Contains(type.Namespace);
        }

        private static bool IsDataMember(MemberNodeInfo member)
        {
            if (member.Kind == EMemberKind.Const || member.Kind == EMemberKind.EnumMember)
                return true;

            return member.IsStatic && member.IsReadOnly;
        }

        private static void ComputeMetrics(CodebaseGraphData graph)
        {
            HashSet<MemberKey> scratch = new();

            foreach (MemberNodeInfo member in graph.Members.Values)
                member.RecomputeFanCounts(scratch);

            MarkWindowNamespaces(graph);

            HashSet<string> namespaces = new();

            foreach (TypeNodeInfo type in graph.Types.Values)
                ComputeTypeMetrics(type, graph, namespaces);
        }

        /// <summary>
        /// Works out the numbers the size and coupling rules read. Compiled size and namespace reach say how
        /// much a type really does, where a member count mostly counts backing fields. Abstractness separates
        /// a type everything can safely depend on from one that cannot be touched without consequences.
        /// </summary>
        private static void ComputeTypeMetrics(TypeNodeInfo type,
            CodebaseGraphData graph,
            HashSet<string> namespaces)
        {
            int size = 0;
            int abstractMembers = 0;
            int dataMembers = 0;
            int behaviourMembers = 0;

            foreach (MemberNodeInfo member in type.Members)
            {
                size += member.IlSize;

                if (member.IsAbstract)
                    abstractMembers++;

                if (IsDataMember(member))
                    dataMembers++;

                if (member.Kind == EMemberKind.Method)
                    behaviourMembers++;
            }

            type.IlSize = size;
            type.BehaviourMemberCount = behaviourMembers;
            type.DataMemberShare = type.Members.Count == 0
                ? 0f
                : dataMembers / (float)type.Members.Count;

            if (type.Kind == ETypeKind.Interface)
                abstractMembers = type.Members.Count;

            type.Abstractness = type.Members.Count == 0
                ? 0f
                : abstractMembers / (float)type.Members.Count;

            namespaces.Clear();

            foreach (TypeKey target in type.Outgoing.Keys)
            {
                TypeNodeInfo other = graph.FindType(target);

                if (other != null && other.Namespace != type.Namespace)
                    namespaces.Add(other.Namespace);
            }

            type.NamespaceReach = namespaces.Count;
        }

        private static void ApplyScriptPaths(CodebaseGraphData graph, ScriptIndex index)
        {
            foreach (TypeNodeInfo type in graph.Types.Values)
            {
                string outermost = ReadOutermostName(type.ShortName);

                type.ScriptPath = ResolvePath(type, outermost, index);
                type.IsEditorOnly = IsEditorOnly(type);
                ApplyScopeExclusion(type, outermost, index);
            }

            PropagateExclusion(graph);
        }

        /// <summary>
        /// Carries an exclusion down through every level of nesting. Checking only the immediate parent
        /// makes the result depend on the order the dictionary happens to hand types back, so a type
        /// nested three deep is excluded or not depending on which of its ancestors was seen first.
        /// </summary>
        private static void PropagateExclusion(CodebaseGraphData graph)
        {
            foreach (TypeNodeInfo type in graph.Types.Values)
            {
                if (type.IsExcludedFromFindings)
                    continue;

                TypeNodeInfo excluded = FindExcludedAncestor(graph, type);
                if (excluded == null)
                    continue;

                type.IsExcludedFromFindings = true;
                type.ExclusionReason = excluded.ExclusionReason;
            }
        }

        private static TypeNodeInfo FindExcludedAncestor(CodebaseGraphData graph, TypeNodeInfo type)
        {
            TypeNodeInfo current = type;

            while (current.DeclaringTypeKey.IsValid)
            {
                current = graph.FindType(current.DeclaringTypeKey);
                if (current == null)
                    return null;

                if (current.IsExcludedFromFindings)
                    return current;
            }

            return null;
        }

        private static string ResolvePath(TypeNodeInfo type, string outermost, ScriptIndex index)
        {
            if (index.ByFullName.TryGetValue(type.FullName, out string path))
                return path;

            // A nested type lives in its outer type's file, and a generic never resolves through MonoScript.
            return index.BySimpleName.GetValueOrDefault(outermost);
        }

        private static string ReadOutermostName(string shortName)
        {
            int cut = shortName.Length;

            int dot = shortName.IndexOf('.');
            if (dot >= 0)
                cut = dot;

            int generic = shortName.IndexOf('<');
            if (generic >= 0 && generic < cut)
                cut = generic;

            return shortName[..cut];
        }

        /// <summary>
        /// Marks types whose findings would be noise: generated output nobody edits, sample fixtures that
        /// exist to hold broken code on purpose, and test folders.
        /// </summary>
        private static void ApplyScopeExclusion(TypeNodeInfo type, string outermost, ScriptIndex index)
        {
            if (type.IsExcludedFromFindings)
                return;

            if (index.IsGenerated(type.ScriptPath, outermost) || IsGeneratedFolder(type.ScriptPath))
            {
                type.IsExcludedFromFindings = true;
                type.ExclusionReason = GeneratedReason;
                return;
            }

            if (HasExcludedSegment(type.ScriptPath) || HasExcludedSegment(type.Namespace))
            {
                type.IsExcludedFromFindings = true;
                type.ExclusionReason = SampleReason;
            }
        }

        private static bool IsGeneratedFolder(string scriptPath) => !string.IsNullOrEmpty(scriptPath)
            && scriptPath.Contains(GeneratedFolderMarker, StringComparison.Ordinal);

        private static bool HasExcludedSegment(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            foreach (string segment in ExcludedSegments)
            {
                if (text.Contains(segment, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static bool IsEditorOnly(TypeNodeInfo type)
        {
            if (type.AssemblyName.EndsWith(EditorSuffix, StringComparison.Ordinal))
                return true;

            return type.ScriptPath != null
                && type.ScriptPath.Contains(EditorFolderMarker, StringComparison.Ordinal);
        }

        private static void BuildNamespaceRelations(CodebaseGraphData graph)
        {
            foreach (TypeNodeInfo type in graph.Types.Values)
            {
                NamespaceNodeInfo source = graph.Namespaces[type.Namespace];

                foreach (TypeKey targetKey in type.Outgoing.Keys)
                {
                    TypeNodeInfo target = graph.FindType(targetKey);
                    if (target == null || target.Namespace == type.Namespace)
                        continue;

                    source.AddOutgoing(target.Namespace);
                    graph.Namespaces[target.Namespace].AddIncoming(type.Namespace);
                }
            }
        }

        private static bool Report(Func<float, string, bool> onProgress, float progress, string status)
            => onProgress == null || onProgress(progress, status);
    }
}