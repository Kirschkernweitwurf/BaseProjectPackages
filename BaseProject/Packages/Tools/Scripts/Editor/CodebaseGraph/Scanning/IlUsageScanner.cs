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
    /// <br/><br/>
    /// String literals are read as well. Invoke, SendMessage, StartCoroutine and animation events all
    /// name their target in a string, and no instruction ever points at what they call, so a literal
    /// that matches a member of the type it is loaded in is recorded as the weakest kind of usage. It
    /// can only ever silence a finding, never raise one.
    /// </summary>
    public static class IlUsageScanner
    {
        private const BindingFlags DeclaredMembers = BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.DeclaredOnly;

        private const int MinimumNameLength = 3;
        private const int TokenSize = 4;
        private const char Underscore = '_';

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

            if (!KeyFactory.TryForType(type, out TypeKey declaringKey))
                declaringKey = default;

            TypeKey receiverKey = default;
            int position = 0;

            while (IlOpCodeTable.TryRead(il, ref position, out OpCode code))
            {
                int operandSize = IlOpCodeTable.GetOperandSize(code, il, position);
                bool hasToken = position + TokenSize <= il.Length;

                if (hasToken && IlOpCodeTable.HasMetadataToken(code))
                {
                    int token = IlOpCodeTable.ReadToken(il, position);
                    TokenResolution resolution = Handle(code,
                        token,
                        module,
                        typeArguments,
                        methodArguments,
                        sourceKey,
                        cache,
                        sink);

                    if (resolution.Type.IsValid)
                        receiverKey = resolution.Type;
                }
                else if (hasToken && code.OperandType == OperandType.InlineString)
                {
                    int token = IlOpCodeTable.ReadToken(il, position);
                    HandleLiteral(token, module, sourceKey, declaringKey, receiverKey, registry, cache, sink);
                }

                position += operandSize;
                if (operandSize == 0 && code.OperandType != OperandType.InlineNone)
                    break;
            }
        }

        /// <summary>
        /// Matches a string literal against member names. Only the type the code sits in and the type
        /// most recently touched are considered, so the guess stays tight enough to be worth trusting.
        /// </summary>
        private static void HandleLiteral(int token,
            Module module,
            MemberKey sourceKey,
            TypeKey declaringKey,
            TypeKey receiverKey,
            MemberRegistry registry,
            TokenResolutionCache cache,
            IUsageSink sink)
        {
            if (!cache.TryGetLiteral(module, token, out string literal))
            {
                literal = ReadLiteral(module, token);
                cache.StoreLiteral(module, token, literal);
            }

            if (!IsMemberName(literal))
                return;

            if (declaringKey.IsValid && registry.TryFindByName(declaringKey, literal, out MemberKey own))
            {
                sink.AddMemberUsage(sourceKey, own, EUsageKind.StringReference);
                return;
            }

            if (receiverKey.IsValid && registry.TryFindByName(receiverKey, literal, out MemberKey target))
                sink.AddMemberUsage(sourceKey, target, EUsageKind.StringReference);
        }

        private static string ReadLiteral(Module module, int token)
        {
            try
            {
                return module.ResolveString(token);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static bool IsMemberName(string literal)
        {
            if (string.IsNullOrEmpty(literal) || literal.Length < MinimumNameLength)
                return false;

            if (!char.IsLetter(literal[0]) && literal[0] != Underscore)
                return false;

            foreach (char value in literal)
            {
                if (!char.IsLetterOrDigit(value) && value != Underscore)
                    return false;
            }

            return true;
        }

        private static TokenResolution Handle(OpCode code,
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
                return resolution;
            }

            if (resolution.Member.IsValid)
                sink.AddMemberUsage(sourceKey, resolution.Member, ResolveKind(code));

            if (resolution.Type.IsValid)
                sink.AddTypeUsage(sourceKey, resolution.Type);

            return resolution;
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
