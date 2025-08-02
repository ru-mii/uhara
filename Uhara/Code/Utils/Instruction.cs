using SharpDisasm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class UInstruction
{
    public static Instruction GetInstruction2(byte[] bytes, ulong address)
    {
        Instruction[] instructions = new Disassembler(bytes, ArchitectureMode.x86_64,
        address, true).Disassemble().ToArray();

        return instructions[0];
    }

    public static Instruction[] GetInstructions2(byte[] bytes)
    {
        return new Disassembler(bytes, ArchitectureMode.x86_64,
        0, true).Disassemble().ToArray();
    }

    internal static int GetMinimumOverwrite(Process process, ulong address, int required = 5)
    {
        byte[] bytes = UMemory.ReadMemoryBytes(process, (IntPtr)address, 50);
        return GetMinimumOverwrite(bytes, required);
    }

    internal static int GetMinimumOverwrite(string bytes, int required = 5)
    {
        return GetMinimumOverwrite(UMemory.GetByteArray(bytes), required);
    }

    internal static int GetMinimumOverwrite(byte[] bytes, int required = 5)
    {
        int length = 0;
        Instruction[] instructions = new Disassembler(bytes, ArchitectureMode.x86_64, 0,
            true).Disassemble().ToArray();

        foreach (Instruction instruction in instructions)
        {
            length += instruction.Length;
            if (length >= required) return length;
        }

        return 0;
    }
}
