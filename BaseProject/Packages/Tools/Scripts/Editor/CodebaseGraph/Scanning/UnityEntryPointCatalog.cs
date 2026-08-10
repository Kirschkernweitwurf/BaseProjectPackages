using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Base.ToolPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>
    /// Knows which members are called by something other than the code itself. Without this every Awake,
    /// every menu item and every asset postprocessor in the project would be reported as dead.
    /// <br/><br/>
    /// Unity dispatches by convention in several unrelated families, and only one of them runs on types
    /// deriving from UnityEngine.Object. An AssetPostprocessor is a plain class, so a single guard on
    /// UnityEngine.Object silently rejects a whole category of engine driven code.
    /// </summary>
    public static class UnityEntryPointCatalog
    {
        private const string AssetModificationProcessorBase = "AssetModificationProcessor";
        private const string AssetPostprocessorBase = "AssetPostprocessor";
        private const string AttributeSuffix = "Attribute";
        private const string ConstructorReason = "Called by the runtime";
        private const string EditorWindowBase = "EditorWindow";
        private const string EngineDrivenReason = "Found by Unity through its base type";
        private const string FormerlySerializedAsName = "FormerlySerializedAs";
        private const string SerializeFieldReason = "Written by Unity serialization";
        private const string UnityMessageReason = "Unity message";

        /// <summary>Messages Unity sends to asset modification processors, which are plain classes.</summary>
        private static readonly HashSet<string> AssetModificationMessageNames = new(StringComparer.Ordinal)
        {
            "CanOpenForEdit",
            "FileModeChanged",
            "IsOpenForEdit",
            "MakeEditable",
            "OnWillCreateAsset",
            "OnWillDeleteAsset",
            "OnWillMoveAsset",
            "OnWillSaveAssets"
        };

        /// <summary>Messages Unity sends to asset postprocessors, which are plain classes.</summary>
        private static readonly HashSet<string> AssetPostprocessorMessageNames = new(StringComparer.Ordinal)
        {
            "GetPostprocessOrder",
            "GetVersion",
            "OnAssignMaterialModel",
            "OnGeneratedCSProjectFiles",
            "OnPostprocessAllAssets",
            "OnPostprocessAssetbundleNameChanged",
            "OnPostprocessAudio",
            "OnPostprocessGameObjectWithUserProperties",
            "OnPostprocessModel",
            "OnPostprocessPrefab",
            "OnPostprocessSprites",
            "OnPostprocessTexture",
            "OnPreprocessAnimation",
            "OnPreprocessAsset",
            "OnPreprocessAudio",
            "OnPreprocessModel",
            "OnPreprocessTexture"
        };

        /// <summary>Messages Unity sends to editor windows.</summary>
        private static readonly HashSet<string> EditorWindowMessageNames = new(StringComparer.Ordinal)
        {
            "CreateGUI",
            "ModifierKeysChanged",
            "OnBecameInvisible",
            "OnBecameVisible",
            "OnFocus",
            "OnHierarchyChange",
            "OnInspectorUpdate",
            "OnLostFocus",
            "OnProjectChange",
            "OnSelectionChange",
            "SaveChanges",
            "ShowButton"
        };

        /// <summary>
        /// Base type names Unity discovers by inheritance rather than by reference. A type deriving from
        /// one of these is reachable even though nothing in the codebase ever names it.
        /// </summary>
        private static readonly HashSet<string> EngineDrivenBaseNames = new(StringComparer.Ordinal)
        {
            "AssetModificationProcessor",
            "AssetPostprocessor",
            "AssetsModifiedProcessor",
            "BuildPlayerProcessor",
            "Editor",
            "EditorTool",
            "EditorWindow",
            "AssetImporterEditor",
            "PropertyDrawer",
            "ScriptableRenderPass",
            "ScriptableWizard",
            "ScriptedImporter",
            "SettingsProvider"
        };

        /// <summary>Interfaces Unity discovers by implementation rather than by reference.</summary>
        private static readonly HashSet<string> EngineDrivenInterfaceNames = new(StringComparer.Ordinal)
        {
            "IActiveBuildTargetChanged",
            "IFilterBuildAssemblies",
            "IOrderedCallback",
            "IPostBuildPlayerScriptDLLs",
            "IPostprocessBuild",
            "IPostprocessBuildWithReport",
            "IPreprocessBuild",
            "IPreprocessBuildWithReport",
            "IProcessScene",
            "IProcessSceneWithReport",
            "IUnityLinkerProcessor"
        };

        /// <summary>
        /// Attribute names that mark a member as reachable from outside the code. Compared by short name
        /// with the Attribute suffix already trimmed, so both the runtime and editor variants match and
        /// package attributes this assembly does not reference still resolve.
        /// </summary>
        private static readonly HashSet<string> EntryPointAttributeNames = new(StringComparer.Ordinal)
        {
            "BurstCompile",
            "Button",
            "ClutchShortcut",
            "ContextMenu",
            "ContextMenuItem",
            "CustomEditor",
            "CustomGridBrush",
            "CustomPropertyDrawer",
            "CustomTimelineEditor",
            "DidReloadScripts",
            "DrawGizmo",
            "DynamicCreateAssetMenu",
            "DynamicMenuItem",
            "EditorToolbarElement",
            "ExecuteAlways",
            "ExecuteInEditMode",
            "HeaderButton",
            "InitializeOnEnterPlayMode",
            "InitializeOnLoad",
            "InitializeOnLoadMethod",
            "InlineButton",
            "MenuItem",
            "MonoPInvokeCallback",
            "OnOpenAsset",
            "Overlay",
            "PostProcessBuild",
            "PostProcessScene",
            "Preserve",
            "RuntimeInitializeOnLoadMethod",
            "ScriptedImporter",
            "SetUp",
            "SettingsProvider",
            "SettingsProviderGroup",
            "Shortcut",
            "TearDown",
            "Test",
            "TestCase",
            "UnityTest",
            "UsedImplicitly"
        };

        /// <summary>Attributes that mark a whole type or member as deliberately out of scope.</summary>
        private static readonly HashSet<string> SuppressionAttributeNames = new(StringComparer.Ordinal)
        {
            "CodebaseGraphIgnore",
            "TroubleshootSample"
        };

        /// <summary>Attributes that mark a method as running on entering play mode.</summary>
        private static readonly HashSet<string> ResetAttributeNames = new(StringComparer.Ordinal)
        {
            "InitializeOnEnterPlayMode",
            "RuntimeInitializeOnLoadMethod"
        };

        /// <summary>Method names Unity invokes by convention on components and scriptable objects.</summary>
        private static readonly HashSet<string> UnityMessageNames = new(StringComparer.Ordinal)
        {
            "Awake",
            "FixedUpdate",
            "LateUpdate",
            "OnAnimatorIK",
            "OnAnimatorMove",
            "OnApplicationFocus",
            "OnApplicationPause",
            "OnApplicationQuit",
            "OnAudioFilterRead",
            "OnBecameInvisible",
            "OnBecameVisible",
            "OnCollisionEnter",
            "OnCollisionEnter2D",
            "OnCollisionExit",
            "OnCollisionExit2D",
            "OnCollisionStay",
            "OnCollisionStay2D",
            "OnControllerColliderHit",
            "OnDestroy",
            "OnDisable",
            "OnDrawGizmos",
            "OnDrawGizmosSelected",
            "OnEnable",
            "OnGUI",
            "OnInspectorGUI",
            "OnJointBreak",
            "OnJointBreak2D",
            "OnMouseDown",
            "OnMouseDrag",
            "OnMouseEnter",
            "OnMouseExit",
            "OnMouseOver",
            "OnMouseUp",
            "OnMouseUpAsButton",
            "OnParticleCollision",
            "OnParticleSystemStopped",
            "OnParticleTrigger",
            "OnPostRender",
            "OnPreCull",
            "OnPreRender",
            "OnRenderImage",
            "OnRenderObject",
            "OnSceneGUI",
            "OnTransformChildrenChanged",
            "OnTransformParentChanged",
            "OnTriggerEnter",
            "OnTriggerEnter2D",
            "OnTriggerExit",
            "OnTriggerExit2D",
            "OnTriggerStay",
            "OnTriggerStay2D",
            "OnValidate",
            "OnWillRenderObject",
            "Reset",
            "Start",
            "Update"
        };

        /// <summary>
        /// Answers both attribute questions about a method from a single read. Building the attribute
        /// data for a member is not cheap, and asking twice for every method in the project was.
        /// </summary>
        /// <param name="method">Method to inspect.</param>
        /// <param name="declaringType">Type the method is declared on.</param>
        /// <param name="reason">Why it counts as an entry point, or null.</param>
        /// <param name="isReset">Whether it runs on entering play mode.</param>
        /// <returns>True when the method must never be reported as dead.</returns>
        public static bool Inspect(MethodBase method, Type declaringType, out string reason, out bool isReset)
        {
            reason = null;
            isReset = false;

            if (method == null)
                return false;

            bool isEntryPoint = IsConventionMessage(method.Name, declaringType);
            if (isEntryPoint)
                reason = UnityMessageReason;

            foreach (CustomAttributeData attribute in ReadAttributes(method))
            {
                string name = TrimAttributeSuffix(attribute.AttributeType.Name);

                if (ResetAttributeNames.Contains(name))
                    isReset = true;

                if (isEntryPoint || !EntryPointAttributeNames.Contains(name))
                    continue;

                isEntryPoint = true;
                reason = $"[{name}]";
            }

            return isEntryPoint;
        }

        /// <summary>Checks whether a constructor is called by the runtime rather than by code.</summary>
        /// <param name="isStatic">Whether the constructor is the static one.</param>
        /// <param name="isUnityObject">Whether the declaring type derives from a Unity object.</param>
        /// <param name="reason">Human readable reason, or null.</param>
        /// <returns>True when the constructor must never be reported as dead.</returns>
        public static bool IsRuntimeConstructor(bool isStatic, bool isUnityObject, out string reason)
        {
            reason = isStatic || isUnityObject
                ? ConstructorReason
                : null;

            return reason != null;
        }

        /// <summary>Checks whether Unity finds this type through its base type or an interface.</summary>
        /// <param name="type">Type to test.</param>
        /// <param name="reason">Human readable reason, or null.</param>
        /// <returns>True when the type must never be reported as unreferenced.</returns>
        public static bool IsEngineDriven(Type type, out string reason)
        {
            reason = null;
            if (type == null)
                return false;

            for (Type current = type.BaseType; current != null; current = current.BaseType)
            {
                if (!EngineDrivenBaseNames.Contains(current.Name))
                    continue;

                reason = $"{EngineDrivenReason} ({current.Name})";
                return true;
            }

            foreach (Type contract in type.GetInterfaces())
            {
                if (!EngineDrivenInterfaceNames.Contains(contract.Name))
                    continue;

                reason = $"{EngineDrivenReason} ({contract.Name})";
                return true;
            }

            return false;
        }

        /// <summary>Checks whether a member is marked as deliberately out of scope for findings.</summary>
        /// <param name="member">Member or type to test.</param>
        /// <param name="reason">Human readable reason, or null.</param>
        /// <returns>True when findings on it should be suppressed.</returns>
        public static bool IsSuppressed(MemberInfo member, out string reason)
        {
            reason = null;
            if (member == null)
                return false;

            foreach (CustomAttributeData attribute in ReadAttributes(member))
            {
                string name = TrimAttributeSuffix(attribute.AttributeType.Name);
                if (!SuppressionAttributeNames.Contains(name))
                    continue;

                reason = $"[{name}]";
                return true;
            }

            return false;
        }

        /// <summary>Checks whether a field is written by Unity instead of by code.</summary>
        /// <param name="field">Field to test.</param>
        /// <param name="reason">Human readable reason, or null when the field is not an entry point.</param>
        /// <returns>True when the field is filled in by serialization.</returns>
        public static bool IsSerializedEntryPoint(FieldInfo field, out string reason)
        {
            reason = null;
            if (!IsSerialized(field))
                return false;

            reason = SerializeFieldReason;
            return true;
        }

        /// <summary>True when Unity serializes the field and therefore writes it from outside the code.</summary>
        /// <param name="field">Field to test.</param>
        /// <returns>True for serialized fields.</returns>
        public static bool IsSerialized(FieldInfo field)
        {
            if (field == null || field.IsStatic || field.IsLiteral)
                return false;

            if (field.IsDefined(typeof(NonSerializedAttribute), false))
                return false;

            // SerializeReference persists a field just as SerializeField does, and a readonly field is
            // not serialized at all, so mistaking one for a plain field costs the stored data.
            if (field.IsDefined(typeof(SerializeField), false)
                || field.IsDefined(typeof(SerializeReference), false))
                return true;

            // A public field is only written by Unity when the type it sits on is serialized at all.
            return field.IsPublic && IsSerializableContainer(field.DeclaringType);
        }

        /// <summary>
        /// True when a method could be the target of an animation event. A clip names only the method,
        /// so the signature is the only guard there is: the engine will only call something public that
        /// takes nothing, or one int, float, string, AnimationEvent or Object.
        /// </summary>
        /// <param name="method">Method to test.</param>
        /// <returns>True when the engine could call it from a clip.</returns>
        public static bool IsAnimationEventSignature(MethodInfo method)
        {
            if (method == null || !method.IsPublic || method.IsStatic)
                return false;

            ParameterInfo[] parameters = method.GetParameters();

            if (parameters.Length == 0)
                return true;

            if (parameters.Length > 1)
                return false;

            Type parameter = parameters[0].ParameterType;

            return parameter == typeof(int)
                || parameter == typeof(float)
                || parameter == typeof(string)
                || parameter == typeof(AnimationEvent)
                || typeof(UnityEngine.Object).IsAssignableFrom(parameter);
        }

        /// <summary>Collects the earlier names a field still answers to in existing assets.</summary>
        /// <param name="field">Field to inspect.</param>
        /// <param name="aliases">List that receives the earlier names.</param>
        public static void CollectSerializedAliases(FieldInfo field, List<string> aliases)
        {
            foreach (CustomAttributeData attribute in ReadAttributes(field))
            {
                if (TrimAttributeSuffix(attribute.AttributeType.Name) != FormerlySerializedAsName)
                    continue;

                foreach (CustomAttributeTypedArgument argument in attribute.ConstructorArguments)
                {
                    if (argument.Value is string alias && alias.Length > 0)
                        aliases.Add(alias);
                }
            }
        }

        /// <summary>Checks whether any attribute on the member marks it as an entry point.</summary>
        /// <param name="member">Member to test.</param>
        /// <param name="reason">Human readable reason, or null when nothing matched.</param>
        /// <returns>True when a known entry point attribute is present.</returns>
        public static bool TryGetEntryPointAttribute(MemberInfo member, out string reason)
        {
            reason = null;
            if (member == null)
                return false;

            foreach (CustomAttributeData attribute in ReadAttributes(member))
            {
                string name = TrimAttributeSuffix(attribute.AttributeType.Name);
                if (!EntryPointAttributeNames.Contains(name))
                    continue;

                reason = $"[{name}]";
                return true;
            }

            return false;
        }

        private static bool IsConventionMessage(string methodName, Type declaringType)
        {
            if (declaringType == null)
                return false;

            if (UnityMessageNames.Contains(methodName)
                && typeof(UnityEngine.Object).IsAssignableFrom(declaringType))
                return true;

            if (EditorWindowMessageNames.Contains(methodName) && InheritsFrom(declaringType, EditorWindowBase))
                return true;

            if (AssetPostprocessorMessageNames.Contains(methodName)
                && InheritsFrom(declaringType, AssetPostprocessorBase))
                return true;

            return AssetModificationMessageNames.Contains(methodName)
                && InheritsFrom(declaringType, AssetModificationProcessorBase);
        }

        private static bool InheritsFrom(Type type, string baseName)
        {
            for (Type current = type; current != null; current = current.BaseType)
            {
                if (string.Equals(current.Name, baseName, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static IList<CustomAttributeData> ReadAttributes(MemberInfo member)
        {
            try
            {
                return member.GetCustomAttributesData();
            }
            catch (Exception)
            {
                // An attribute type from a missing optional dependency cannot be loaded. Treat it as absent.
                return Array.Empty<CustomAttributeData>();
            }
        }

        private static bool IsSerializableContainer(Type type)
        {
            if (type == null)
                return false;

            return typeof(UnityEngine.Object).IsAssignableFrom(type)
                || type.IsDefined(typeof(SerializableAttribute), false);
        }

        private static string TrimAttributeSuffix(string name)
            => name.EndsWith(AttributeSuffix, StringComparison.Ordinal)
                ? name[..^AttributeSuffix.Length]
                : name;
    }
}
