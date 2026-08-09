using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Base.ToolPackage.Editor.CodebaseGraph.Scanning
{
    /// <summary>
    /// Decoding table for raw IL. Maps opcode bytes to <see cref="OpCode"/> values and reports how many
    /// operand bytes follow, so a method body can be walked instruction by instruction.
    /// </summary>
    public static class IlOpCodeTable
    {
        private const int ByteOperandSize = 1;
        private const int IntOperandSize = 4;
        private const int LongOperandSize = 8;
        private const byte MultiBytePrefix = 0xFE;
        private const int ShortOperandSize = 2;
        private const int SwitchCaseSize = 4;
        private const int SwitchCountSize = 4;
        private const int TableSize = 256;

        private static readonly OpCode[] MultiByteCodes = new OpCode[TableSize];
        private static readonly OpCode[] SingleByteCodes = new OpCode[TableSize];

        static IlOpCodeTable()
        {
            foreach (FieldInfo field in typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (field.FieldType != typeof(OpCode))
                    continue;

                OpCode code = (OpCode)field.GetValue(null);

                if (code.Size == 1)
                    SingleByteCodes[code.Value & 0xFF] = code;
                else
                    MultiByteCodes[code.Value & 0xFF] = code;
            }
        }

        /// <summary>Reads the opcode at the current position and advances past it.</summary>
        /// <param name="il">Raw method body bytes.</param>
        /// <param name="position">Read cursor, advanced past the opcode.</param>
        /// <param name="code">The decoded opcode.</param>
        /// <returns>True when an opcode could be read.</returns>
        public static bool TryRead(byte[] il, ref int position, out OpCode code)
        {
            code = default;
            if (position >= il.Length)
                return false;

            byte first = il[position];
            position++;

            if (first != MultiBytePrefix)
            {
                code = SingleByteCodes[first];

                // An unmapped byte means the walk has lost alignment. Reading on would decode whatever
                // follows as instructions and quietly invent usages, so the walk stops here instead.
                return code.Size != 0;
            }

            if (position >= il.Length)
                return false;

            code = MultiByteCodes[il[position]];
            position++;
            return code.Size != 0;
        }

        /// <summary>Returns how many operand bytes follow the given opcode.</summary>
        /// <param name="code">The opcode that was just read.</param>
        /// <param name="il">Raw method body bytes.</param>
        /// <param name="position">Position of the first operand byte.</param>
        /// <returns>The operand size in bytes.</returns>
        public static int GetOperandSize(OpCode code, byte[] il, int position)
        {
            switch (code.OperandType)
            {
                case OperandType.InlineNone:
                    return 0;

                case OperandType.ShortInlineBrTarget:
                case OperandType.ShortInlineI:
                case OperandType.ShortInlineVar:
                    return ByteOperandSize;

                case OperandType.InlineVar:
                    return ShortOperandSize;

                case OperandType.InlineI8:
                case OperandType.InlineR:
                    return LongOperandSize;

                case OperandType.InlineSwitch:
                    return GetSwitchSize(il, position);

                default:
                    return IntOperandSize;
            }
        }

        /// <summary>True when the operand of this opcode is a metadata token.</summary>
        /// <param name="code">The opcode that was just read.</param>
        /// <returns>True for token carrying opcodes.</returns>
        public static bool HasMetadataToken(OpCode code)
            => code.OperandType == OperandType.InlineField
                || code.OperandType == OperandType.InlineMethod
                || code.OperandType == OperandType.InlineTok
                || code.OperandType == OperandType.InlineType;

        /// <summary>Reads the four byte metadata token at the given position.</summary>
        /// <param name="il">Raw method body bytes.</param>
        /// <param name="position">Position of the first operand byte.</param>
        /// <returns>The metadata token.</returns>
        public static int ReadToken(byte[] il, int position) => BitConverter.ToInt32(il, position);

        private static int GetSwitchSize(byte[] il, int position)
        {
            if (position + SwitchCountSize > il.Length)
                return 0;

            int caseCount = BitConverter.ToInt32(il, position);
            return SwitchCountSize + caseCount * SwitchCaseSize;
        }
    }
}
