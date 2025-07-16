using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class Main
{
    public int GetMinimumHKOverwrite(IntPtr address, int required = 14)
    {
        try
        {
            return UInstruction.GetMinimumOverwrite(Instance, (ulong)address, required);
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
            return CodeHK(address, overwriteSize, UMemory.GetByteArray(customCode));
        }
        catch { }
        return IntPtr.Zero;
    }

    public IntPtr CodeHK(IntPtr address, int overwriteSize, byte[] customCode)
    {
        try
        {
            byte[] jmpConfirmBytes = new byte[] { 0xFF, 0x25, 0x00, 0x00, 0x00, 0x00 };
            byte[] readConfirmBytes = UMemory.ReadMemoryBytes(Instance, address, 0x6);

            if (readConfirmBytes == null) return IntPtr.Zero;

            if (jmpConfirmBytes.SequenceEqual(readConfirmBytes))
            {
                ulong oldAllocated = UMemory.ReadMemory<ulong>(Instance, address + 0x6);

                if (oldAllocated == 0) return IntPtr.Zero;
                else return (IntPtr)(oldAllocated - 0x8);
            }
            else
            {
                ulong allocated = RefAllocateMemory(Instance, 0x100);
                if (allocated == 0) return IntPtr.Zero;

                byte[] stolen = UMemory.ReadMemoryBytes(Instance, address, overwriteSize);
                if (stolen == null) return IntPtr.Zero;

                byte[] e1 = customCode;
                byte[] e2 = stolen;
                byte[] e3 = { 0xFF, 0x25, 0x00, 0x00, 0x00, 0x00 };
                byte[] e4 = BitConverter.GetBytes((ulong)address + (ulong)overwriteSize);
                byte[] end = UArray.Merge(e1, e2, e3, e4);

                byte[] s1 = { 0xFF, 0x25, 0x00, 0x00, 0x00, 0x00 };
                byte[] s2 = BitConverter.GetBytes(allocated + 0x8);
                byte[] start = UArray.Merge(s1, s2);

                RefWriteBytes(Instance, allocated + 0x8, end);
                RefWriteBytes(Instance, (ulong)address, start);

                return (IntPtr)allocated;
            }
        }
        catch { }
        return IntPtr.Zero;
    }

    public IntPtr ScanSingle(string signature)
    {
        try
        {
            if (CheckSetProcessAndValues())
                return (IntPtr)UMemory.ScanSingle(signature);
        }
        catch { }
        return IntPtr.Zero;
    }

    public IntPtr ScanRel(int offset, string signature)
    {
        try
        {
            if (CheckSetProcessAndValues())
                return (IntPtr)UMemory.ScanRel(offset, signature);
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

    readonly Dictionary<string, byte[]> SaveRegBytes  = new Dictionary<string, byte[]>
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
