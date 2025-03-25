using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class UnityCPP_JitSave : UShared
{
    ulong Allocated = 0;
    ulong Arguments = 0;
    ulong Output = 0;

    public IntPtr AddFlag(string _class, string _method, short overwriteSize)
    {
        return Add("Assembly-CSharp.dll", "", _class, _method, 0, 0, overwriteSize,
            new byte[] { 0x48, 0x83, 0x05, 0xF0, 0xFF, 0xFF, 0xFF, 0x01 });
    }

    public IntPtr AddInstance(string _class, string _method)
    {
        try
        {
            return Add("Assembly-CSharp.dll", "", _class, _method, 0, 0, 15,
                new byte[] { 0x48, 0x89, 0x3D, 0xF1, 0xFF, 0xFF, 0xFF, 0x90 });
        }
        catch { }
        return IntPtr.Zero;
    }

    public IntPtr AddInstance(string _class, string _method, short overwriteSize)
    {
        try
        {
            return Add("Assembly-CSharp.dll", "", _class, _method, 0, 0, overwriteSize,
                new byte[] { 0x48, 0x89, 0x3D, 0xF1, 0xFF, 0xFF, 0xFF, 0x90 });
        }
        catch { }
        return IntPtr.Zero;
    }

    public IntPtr AddInstance(string _namespace, string _class, string _method, short overwriteSize)
    {
        try
        {
            return Add("Assembly-CSharp.dll", _namespace, _class, _method, 0, 0, overwriteSize,
                new byte[] { 0x48, 0x89, 0x3D, 0xF1, 0xFF, 0xFF, 0xFF, 0x90 });
        }
        catch { }
        return IntPtr.Zero;
    }

    public IntPtr Add(string _class, string _method, short paramCount, short hookOffset, short overwriteSize, byte[] bytes)
    {
        try
        {
            return Add("Assembly-CSharp.dll", "", _class, _method, paramCount, hookOffset, overwriteSize, bytes);
        }
        catch { }
        return IntPtr.Zero;
    }

    public IntPtr Add(string _assembly, string _namespace, string _class, string _method, short paramCount,
    short hookOffset, short overwriteSize, byte[] bytes)
    {
        try
        {
            if (GetSetValues())
            {
                // ---
                if (!_assembly.EndsWith(".dll")) _assembly += ".dll";
                byte[] arg1 = UProgram.StringToMultibyte(_assembly);
                byte[] arg2 = UProgram.StringToMultibyte(_namespace);
                byte[] arg3 = UProgram.StringToMultibyte(_class);
                byte[] arg4 = UProgram.StringToMultibyte(_method);
                byte[] arg5 = BitConverter.GetBytes(paramCount);

                byte[] _offset = BitConverter.GetBytes(hookOffset);
                byte[] _overwriteSize = BitConverter.GetBytes(overwriteSize);
                byte[] _bytesSize = BitConverter.GetBytes((short)bytes.Length);
                byte[] arg6 = UArray.Merge(_offset, _overwriteSize, _bytesSize, bytes);

                // ---
                byte[] all = UArray.Merge(
                    BitConverter.GetBytes((short)arg1.Length), arg1,
                    BitConverter.GetBytes((short)arg2.Length), arg2,
                    BitConverter.GetBytes((short)arg3.Length), arg3,
                    BitConverter.GetBytes((short)arg4.Length), arg4,
                    BitConverter.GetBytes((short)arg5.Length), arg5,
                    BitConverter.GetBytes((short)arg6.Length), arg6);

                RefWriteBytes(Instance, Arguments + 0x8, all);
                RefWriteBytes(Instance, Arguments + 0x2, BitConverter.GetBytes((short)all.Length));

                Arguments += 0x8 + (ulong)all.Length;
                return (IntPtr)((Output += 0x100) - 0x100);
            }
        }
        catch { }
        return IntPtr.Zero;
    }

    public UnityCPP_JitSave()
    {
        string instName = "UnityCPP.JitSave";

        ulong lastAddress = 0;
        if (ulong.TryParse(USaves.Get(instName), out lastAddress) && lastAddress != 0)
        {
            byte[] sigTest = UMemory.ReadMemoryBytes(Instance, lastAddress, 0x8);
            if (sigTest != null)
            {
                byte[] sigCheck = new byte[] { 0x5E, 0x9B, 0xDB, 0x02, 0xF7, 0x39, 0x1C, 0x02 };
                if (sigCheck.SequenceEqual(sigTest)) RefWriteBytes(Instance, lastAddress, BitConverter.GetBytes((ulong)0));
            }
        }

        Allocated = RefAllocateMemory(Instance, 0x5000);
        if (Allocated != 0)
        {
            USaves.Set(instName, Allocated.ToString());
            RefWriteBytes(Instance, Allocated, AsmBlocks.UnityCS_JitSave);
            RefCreateThread(Instance, Allocated + 0x8);
            Arguments = Allocated + 0x2000;
            Output = Allocated + 0x3002;
        }
    }
}