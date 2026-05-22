using SharpDisasm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class TInstruction
{
    internal static Instruction[] GetInstructionsBackwards(Process process, ulong address, int numBytes)
    {
        do
        {
            List<Instruction> instrs = new List<Instruction>();

            ulong behind = address - (ulong)numBytes;
            byte[] bytes = TMemory.ReadMemoryBytes(process, behind, numBytes);
            if (bytes == null) break;

            instrs = GetInstructions2(bytes, behind).ToList();
            if (instrs == null) break;

            instrs.Reverse();
            return instrs.ToArray();
        }
        while (false);
        return new List<Instruction>().ToArray();
    }

    internal static ulong GetAlignedAddress(Process process, ulong address)
    {
        byte[] plank = TMemory.ReadMemoryBytes(process, address - 60, 100);
        Instruction[] instructions = GetInstructions2(plank);
        ulong toReturn = 0;

        ulong current = address - 60;
        for (int i = 0; i < instructions.Length; i++)
        {
            if (current <= address && current + (ulong)instructions[i].Length > address)
            {
                toReturn = current;
                break;
            }

            current += (ulong)instructions[i].Length;
        }

        return toReturn;
    }

    internal static long ExtractRipValue(Instruction instruction)
    {
        return ExtractRipValue(instruction.ToString());
    }

    internal static long ExtractRipValue(string instruction)
    {
        if (instruction.Contains("rip") && instruction.Contains("]"))
        {
            string converted = instruction.Substring(instruction.IndexOf("rip") + 4);
            converted = converted.Remove(converted.IndexOf("]"));
            return TConvert.Parse<long>(converted);
        }
        return 0;
    }

    internal static Instruction GetInstruction2(Process process, ulong address)
    {
        byte[] bytes = TMemory.ReadMemoryBytes(process, address, 50);
        Instruction[] instructions = new Disassembler(bytes, ArchitectureMode.x86_64,
        address, true).Disassemble().ToArray();

        return instructions[0];
    }

    internal static Instruction GetInstruction2(byte[] bytes, ulong address)
    {
        Instruction[] instructions = new Disassembler(bytes, ArchitectureMode.x86_64,
        address, true).Disassemble().ToArray();

        return instructions[0];
    }

    internal static Instruction[] GetInstructions2(byte[] bytes, ulong address = 0)
    {
        return new Disassembler(bytes, ArchitectureMode.x86_64,
        address, true).Disassemble().ToArray();
    }

    internal static int GetMinimumOverwrite(Process process, ulong address, int required = 5)
    {
        byte[] bytes = TMemory.ReadMemoryBytes(process, (IntPtr)address, 50);
        return GetMinimumOverwrite(bytes, required);
    }

    internal static int GetMinimumOverwrite(string bytes, int required = 5)
    {
        return GetMinimumOverwrite(TSignature.GetBytes(bytes), required);
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
