using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Base.ToolPackage.Editor.CodebaseGraph.Model;

namespace Base.ToolPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>
    /// Reads the compiled body of every method and reports which members it touches. Working off IL
    /// instead of source text means overloads, generic instantiations and extension methods all resolve
    /// to the exact member the compiler bound to.
    /// </summary>
    public static class IlUsageScanner
    {
        private const BindingFlags DeclaredMembers = BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.DeclaredOnly;

        private const int TokenSize = 4;

        /// <summary>Maps the token carrying opcodes to the usage they express.</summary>
        private static readonly Dictionary<short, EUsageKind> UsageByOpCode = BuildUsageTable();

        /// <summary>Scans every method body declared on the type and reports the usages it finds.</summary>
        /// <param name="type">Type whose bodies should be read.</param>
        /// <param name="registry">Registry used to fold accessors and lambdas onto their owner.</param>
        /// <param name="cache">Cache of tokens already resolved during this scan.</param>
        /// <param name="sink">Receiver for the usages.</param>
        public static void ScanType(Type type, MemberRegistry registry, TokenResolutionCache cache, IUsageSink sink)
        {
            foreach (MethodInfo method in type.GetMethods(DeclaredMembers))
                ScanMethod(method, type, registry, cache, sink);

            foreach (ConstructorInfo constructor in type.GetConstructors(DeclaredMembers))
                ScanMethod(constructor, type, registry, cache, sink);
        }

        private static Dictionary<short, EUsageKind> BuildUsageTable()
        {
            Dictionary<short, EUsageKind> table = new()
            {
                [OpCodes.Ldfld.Value] = EUsageKind.FieldRead,
                [OpCodes.Ldflda.Value] = EUsageKind.FieldRead,
                [OpCodes.Ldsfld.Value] = EUsageKind.FieldRead,
                [OpCodes.Ldsflda.Value] = EUsageKind.FieldRead,
                [OpCodes.Stfld.Value] = EUsageKind.FieldWrite,
                [OpCodes.Stsfld.Value] = EUsageKind.FieldWrite,
                [OpCodes.Newobj.Value] = EUsageKind.Construct,
                [OpCodes.Call.Value] = EUsageKind.Call,
                [OpCodes.Calli.Value] = EUsageKind.Call,
                [OpCodes.Callvirt.Value] = EUsageKind.VirtualCall,
                [OpCodes.Ldftn.Value] = EUsageKind.DelegateReference,
                [OpCodes.Ldvirtftn.Value] = EUsageKind.DelegateReference
            };

            return table;
        }

        private static void ScanMethod(MethodBase method,
            Type type,
            MemberRegistry registry,
            TokenResolutionCache cache,
            IUsageSink sink)
        {
            byte[] il = ReadIl(method);
            if (il == null || il.Length == 0)
                return;

            if (!KeyFactory.TryForMember(method, out MemberKey rawSource))
                return;

            MemberKey sourceKey = registry.Resolve(rawSource);
            if (registry.Find(sourceKey) == null)
                return;

            sink.AddIlSize(sourceKey, il.Length);

            Type[] typeArguments = SafeGetGenericArguments(type);
            Type[] methodArguments = SafeGetGenericArguments(method);
            Module module = method.Module;

            int position = 0;

            while (IlOpCodeTable.TryRead(il, ref position, out OpCode code))
            {
                int operandSize = IlOpCodeTable.GetOperandSize(code, il, position);

                if (IlOpCodeTable.HasMetadataToken(code) && position + TokenSize <= il.Length)
                {
                    int token = IlOpCodeTable.ReadToken(il, position);
                    Handle(code, token, module, typeArguments, methodArguments, sourceKey, cache, sink);
                }

                position += operandSize;
                if (operandSize == 0 && code.OperandType != OperandType.InlineNone)
                    break;
            }
        }

        private static void Handle(OpCode code,
            int token,
            Module module,
            Type[] typeArguments,
            Type[] methodArguments,
            MemberKey sourceKey,
            TokenResolutionCache cache,
            IUsageSink sink)
        {
            if (!cache.TryGet(module, token, out TokenResolution resolution))
            {
                resolution = Resolve(code, token, module, typeArguments, methodArguments);
                cache.Store(module, token, resolution);
            }

            if (!resolution.IsResolved)
            {
                sink.ReportUnresolvedToken();
                return;
            }

            if (resolution.Member.IsValid)
                sink.AddMemberUsage(sourceKey, resolution.Member, ResolveKind(code));

            if (resolution.Type.IsValid)
                sink.AddTypeUsage(sourceKey, resolution.Type);
        }

        private static TokenResolution Resolve(OpCode code,
            int token,
            Module module,
            Type[] typeArguments,
            Type[] methodArguments)
        {
            try
            {
                switch (code.OperandType)
                {
                    case OperandType.InlineField:
                        return BuildMemberResolution(module.ResolveField(token, typeArguments, methodArguments));

                    case OperandType.InlineMethod:
                        return BuildMemberResolution(module.ResolveMethod(token, typeArguments, methodArguments));

                    case OperandType.InlineType:
                        return BuildTypeResolution(module.ResolveType(token, typeArguments, methodArguments));

                    case OperandType.InlineTok:
                        return BuildUnknownResolution(module.ResolveMember(token,
                            typeArguments,
                            methodArguments));

                    default:
                        return new TokenResolution(default, default, true);
                }
            }
            catch (Exception)
            {
                // Tokens from generic contexts the runtime cannot rebuild are counted, not logged.
                return new TokenResolution(default, default, false);
            }
        }

        private static TokenResolution BuildUnknownResolution(MemberInfo member)
            => member is Type type
                ? BuildTypeResolution(type)
                : BuildMemberResolution(member);

        private static TokenResolution BuildMemberResolution(MemberInfo member)
        {
            if (member == null)
                return new TokenResolution(default, default, true);

            KeyFactory.TryForMember(member, out MemberKey memberKey);

            TypeKey typeKey = default;
            if (member.DeclaringType != null)
                KeyFactory.TryForType(member.DeclaringType, out typeKey);

            return new TokenResolution(memberKey, typeKey, true);
        }

        private static TokenResolution BuildTypeResolution(Type type)
        {
            if (type == null)
                return new TokenResolution(default, default, true);

            KeyFactory.TryForType(type, out TypeKey typeKey);
            return new TokenResolution(default, typeKey, true);
        }

        private static EUsageKind ResolveKind(OpCode code)
            => UsageByOpCode.TryGetValue(code.Value, out EUsageKind mapped)
                ? mapped
                : EUsageKind.DelegateReference;

        private static byte[] ReadIl(MethodBase method)
        {
            try
            {
                return method.GetMethodBody()?.GetILAsByteArray();
            }
            catch (Exception)
            {
                // Abstract, extern and native methods have no readable body.
                return null;
            }
        }

        private static Type[] SafeGetGenericArguments(Type type)
        {
            try
            {
                return type.IsGenericType
                    ? type.GetGenericArguments()
                    : Type.EmptyTypes;
            }
            catch (Exception)
            {
                return Type.EmptyTypes;
            }
        }

        private static Type[] SafeGetGenericArguments(MethodBase method)
        {
            try
            {
                return method.IsGenericMethod
                    ? method.GetGenericArguments()
                    : Type.EmptyTypes;
            }
            catch (Exception)
            {
                return Type.EmptyTypes;
            }
        }
    }
}
