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
    /// String literals are read as well. Invoke, SendMessage and StartCoroutine name their target in a
    /// string and no instruction ever points at what they call. So a literal matching a member of the
    /// type it is loaded in is recorded as the weakest kind of usage. The literals of a body are only
    /// applied when that same body calls one of those methods, because otherwise a plain
    /// Debug.Log("Reset") would quietly silence every finding on a member called Reset.
    /// <br/><br/>
    /// A literal is resolved only against the type the calling code sits in, so a SendMessage aimed at
    /// another object is not covered. That is deliberate rather than a gap left open: the receiver of
    /// such a call is a GameObject, not the component that owns the method, so there is nothing
    /// accurate to resolve the name against and any guess would be a guess.
    /// </summary>
    internal static class IlUsageScanner
    {
        private const BindingFlags DeclaredMembers = BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.Static
            | BindingFlags.DeclaredOnly;

        private const int MinimumNameLength = 3;
        private const int TokenSize = 4;
        private const char Underscore = '_';

        /// <summary>Methods that reach code by name, which is what makes a nearby literal meaningful.</summary>
        private static readonly HashSet<string> DispatchNames = new(StringComparer.Ordinal)
        {
            "BroadcastMessage",
            "CancelInvoke",
            "Invoke",
            "InvokeRepeating",
            "SendMessage",
            "SendMessageUpwards",
            "StartCoroutine",
            "StopCoroutine"
        };

        /// <summary>Maps the token carrying opcodes to the usage they express.</summary>
        private static readonly Dictionary<short, EUsageKind> UsageByOpCode = BuildUsageTable();

        /// <summary>Scans every method body declared on the type and reports the usages it finds.</summary>
        /// <param name="type">Type whose bodies should be read.</param>
        /// <param name="registry">Registry used to fold accessors and lambdas onto their owner.</param>
        /// <param name="cache">Cache of tokens already resolved during this scan.</param>
        /// <param name="sink">Receiver for the usages.</param>
        internal static void ScanType(Type type, MemberRegistry registry, TokenResolutionCache cache, IUsageSink sink)
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
                declaringKey = default(TypeKey);

            List<string> literals = null;
            bool hasDispatch = false;
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

                    hasDispatch |= resolution.IsDispatch;
                }
                else if (hasToken && code.OperandType == OperandType.InlineString)
                {
                    int token = IlOpCodeTable.ReadToken(il, position);
                    string literal = ReadCachedLiteral(module, token, cache);

                    if (IsMemberName(literal))
                    {
                        literals ??= new List<string>();
                        literals.Add(literal);
                    }
                }

                position += operandSize;
                if (operandSize == 0 && code.OperandType != OperandType.InlineNone)
                    break;
            }

            if (hasDispatch && literals != null)
                ApplyLiterals(literals, sourceKey, declaringKey, registry, sink);
        }

        /// <summary>
        /// Applies the literals a body loaded, now that the body is known to dispatch by name. Only the
        /// type the calling code sits in is considered, which covers the common shape of a component
        /// invoking one of its own methods and refuses to guess about anything else.
        /// </summary>
        private static void ApplyLiterals(List<string> literals,
            MemberKey sourceKey,
            TypeKey declaringKey,
            MemberRegistry registry,
            IUsageSink sink)
        {
            if (!declaringKey.IsValid)
                return;

            foreach (string literal in literals)
            {
                if (registry.TryFindByName(declaringKey, literal, out MemberKey own))
                    sink.AddMemberUsage(sourceKey, own, EUsageKind.StringReference);
            }
        }

        private static string ReadCachedLiteral(Module module, int token, TokenResolutionCache cache)
        {
            if (cache.TryGetLiteral(module, token, out string literal))
                return literal;

            literal = ReadLiteral(module, token);
            cache.StoreLiteral(module, token, literal);

            return literal;
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
                cache.Store(module,
                    token,
                    resolution,
                    typeArguments.Length == 0 && methodArguments.Length == 0);
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
                        return new TokenResolution(default(MemberKey), default(TypeKey), true, false);
                }
            }
            catch (Exception)
            {
                // Tokens from generic contexts the runtime cannot rebuild are counted, not logged.
                return new TokenResolution(default(MemberKey), default(TypeKey), false, false);
            }
        }

        private static TokenResolution BuildUnknownResolution(MemberInfo member) => member is Type type
            ? BuildTypeResolution(type)
            : BuildMemberResolution(member);

        private static TokenResolution BuildMemberResolution(MemberInfo member)
        {
            if (member == null)
                return new TokenResolution(default(MemberKey), default(TypeKey), true, false);

            KeyFactory.TryForMember(member, out MemberKey memberKey);

            TypeKey typeKey = default;
            if (member.DeclaringType != null)
                KeyFactory.TryForType(member.DeclaringType, out typeKey);

            return new TokenResolution(memberKey, typeKey, true, DispatchNames.Contains(member.Name));
        }

        private static TokenResolution BuildTypeResolution(Type type)
        {
            if (type == null)
                return new TokenResolution(default(MemberKey), default(TypeKey), true, false);

            KeyFactory.TryForType(type, out TypeKey typeKey);
            return new TokenResolution(default(MemberKey), typeKey, true, false);
        }

        private static EUsageKind ResolveKind(OpCode code)
            => UsageByOpCode.GetValueOrDefault(code.Value, EUsageKind.DelegateReference);

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