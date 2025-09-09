using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiveSplit.ComponentUtil;

public partial class Main : MainShared
{
    public IntPtr CodeHKFlag(string signature)
    {
        try
        {
            do
            {
                ulong address = TMemory.ScanSingle(Instance, signature);
                if (address == 0) break;

                int minimumOverwrite = TInstruction.GetMinimumOverwrite(Instance, address, 14);
                if (minimumOverwrite == 0) break;

                byte[] stolen = TMemory.ReadMemoryBytes(Instance, address, minimumOverwrite);
                if (stolen == null) break;

                MemoryManager.AddOverwrite(address, stolen);

                ulong allocateStart = MemoryManager.AllocateSafe(0x1000);
                if (allocateStart == 0) break;

                ulong allocate = allocateStart;

                // leave 8 bytes for flag
                allocate += 0x8;

                byte[] flagAsm = new byte[] { 0x83, 0x05, 0xF1, 0xFF, 0xFF, 0xFF, 0x01, 0x90 };
                RefWriteBytes(Instance, allocate, flagAsm);
                allocate += (ulong)flagAsm.Length;

                RefWriteBytes(Instance, allocate, stolen);
                allocate += (ulong)stolen.Length;

                TMemory.CreateAbsoluteJump(Instance, allocate, address + (ulong)minimumOverwrite);
                allocate += 14;

                TMemory.CreateAbsoluteJump(Instance, address, allocateStart + 0x8);

                return (IntPtr)allocateStart;
            }
            while (false);
        }
        catch { }
        return IntPtr.Zero;
    }

    public int GetMinimumHKOverwrite(IntPtr address, int required = 14)
    {
        try
        {
            return TInstruction.GetMinimumOverwrite(Instance, (ulong)address, required);
        }
        catch { }
        return 0;
    }

    public IntPtr CatchReg(IntPtr address, string register, int overwriteSize)
    {
        try
        {
            return CodeHK(address, overwriteSize, SaveRegBytes[register]);
        }
        catch { }
        return IntPtr.Zero;
    }

    public IntPtr CodeHK(IntPtr address, int overwriteSize, string customCode)
    {
        try
        {
            return CodeHK(address, overwriteSize, TSignature.GetBytes(customCode));
        }
        catch { }
        return IntPtr.Zero;
    }

    public IntPtr CodeHK(IntPtr address, int overwriteSize, byte[] customCode)
    {
        try
        {
            byte[] jmpConfirmBytes = new byte[] { 0xFF, 0x25, 0x00, 0x00, 0x00, 0x00 };
            byte[] readConfirmBytes = TMemory.ReadMemoryBytes(Instance, address, 0x6);

            if (readConfirmBytes == null) return IntPtr.Zero;

            if (jmpConfirmBytes.SequenceEqual(readConfirmBytes))
            {
                ulong oldAllocated = TMemory.ReadMemory<ulong>(Instance, address + 0x6);

                if (oldAllocated == 0) return IntPtr.Zero;
                else return (IntPtr)(oldAllocated - 0x8);
            }
            else
            {
                ulong allocated = RefAllocateMemory(Instance, 0x100);
                if (allocated == 0) return IntPtr.Zero;

                byte[] stolen = TMemory.ReadMemoryBytes(Instance, address, overwriteSize);
                if (stolen == null) return IntPtr.Zero;

                byte[] e1 = customCode;
                byte[] e2 = stolen;
                byte[] e3 = { 0xFF, 0x25, 0x00, 0x00, 0x00, 0x00 };
                byte[] e4 = BitConverter.GetBytes((ulong)address + (ulong)overwriteSize);
                byte[] end = TArray.Merge(e1, e2, e3, e4);

                byte[] s1 = { 0xFF, 0x25, 0x00, 0x00, 0x00, 0x00 };
                byte[] s2 = BitConverter.GetBytes(allocated + 0x8);
                byte[] start = TArray.Merge(s1, s2);

                RefWriteBytes(Instance, allocated + 0x8, end);
                RefWriteBytes(Instance, (ulong)address, start);

                return (IntPtr)allocated;
            }
        }
        catch { }
        return IntPtr.Zero;
    }

    public IntPtr ScanSingle(string signature, string moduleName = null, int offset = 0)
    {
        try
        {
            return (IntPtr)TMemory.ScanSingle(Instance, signature, moduleName, offset);
        }
        catch { }
        return IntPtr.Zero;
    }

    public IntPtr ScanRel(int offset, string signature)
    {
        try
        {
            return (IntPtr)TMemory.ScanRel(Instance, offset, signature);
        }
        catch { }
        return IntPtr.Zero;
    }

    public IntPtr ScanRel2(string signature, int toInstructionOffset = 0)
    {
        try
        {
            return (IntPtr)TMemory.ScanRel2(Instance, signature, null, toInstructionOffset);
        }
        catch { }
        return IntPtr.Zero;
    }

    public IntPtr ScanRel2(string signature, string moduleName = null, int toInstructionOffset = 0)
    {
        try
        {
            return (IntPtr)TMemory.ScanRel2(Instance, signature, moduleName, toInstructionOffset);
        }
        catch { }
        return IntPtr.Zero;
    }

    public string GetCategoryName()
    {
        try
        {
            string category = UReflection.GetValue(UReflection.GetValue(Application.OpenForms["TimerForm"],
            "<CurrentState>k__BackingField",
            "<Run>k__BackingField",
            "categoryName")).ToString();

            return category ?? "";
        }
        catch { }
        return "";
    }

    public readonly Dictionary<string, byte[]> SaveRegBytes  = new Dictionary<string, byte[]>
    {
        { "rax", new byte[] { 0x48, 0x89, 0x05, 0xF1, 0xFF, 0xFF, 0xFF } },
        { "rbx", new byte[] { 0x48, 0x89, 0x1D, 0xF1, 0xFF, 0xFF, 0xFF } },
        { "rcx", new byte[] { 0x48, 0x89, 0x15, 0xF1, 0xFF, 0xFF, 0xFF } },
        { "rdx", new byte[] { 0x48, 0x89, 0x15, 0xF1, 0xFF, 0xFF, 0xFF } },
        { "rbp", new byte[] { 0x48, 0x89, 0x2D, 0xF1, 0xFF, 0xFF, 0xFF } },
        { "rsp", new byte[] { 0x48, 0x89, 0x25, 0xF1, 0xFF, 0xFF, 0xFF } },
        { "rsi", new byte[] { 0x48, 0x89, 0x35, 0xF1, 0xFF, 0xFF, 0xFF } },
        { "rdi", new byte[] { 0x48, 0x89, 0x3D, 0xF1, 0xFF, 0xFF, 0xFF } },
        { "r8",  new byte[] { 0x4C, 0x89, 0x05, 0xF1, 0xFF, 0xFF, 0xFF } },
        { "r9",  new byte[] { 0x4C, 0x89, 0x0D, 0xF1, 0xFF, 0xFF, 0xFF } },
        { "r10", new byte[] { 0x4C, 0x89, 0x15, 0xF1, 0xFF, 0xFF, 0xFF } },
        { "r11", new byte[] { 0x4C, 0x89, 0x1D, 0xF1, 0xFF, 0xFF, 0xFF } },
        { "r12", new byte[] { 0x4C, 0x89, 0x25, 0xF1, 0xFF, 0xFF, 0xFF } },
        { "r13", new byte[] { 0x4C, 0x89, 0x2D, 0xF1, 0xFF, 0xFF, 0xFF } },
        { "r14", new byte[] { 0x4C, 0x89, 0x35, 0xF1, 0xFF, 0xFF, 0xFF } },
        { "r15", new byte[] { 0x4C, 0x89, 0x3D, 0xF1, 0xFF, 0xFF, 0xFF } },
    };
}
