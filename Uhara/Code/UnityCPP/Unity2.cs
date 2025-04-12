using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class Unity2 : UShared
{
    ulong Dog = 0; // Allocated
    ulong Cat = 0; // Arguments
    ulong Lion = 0; // Output

    private string DefAssembly = "Assembly-CSharp.dll";
    private string DefNamespace = "";

    public void SetOuter(string _assembly, string _namespace = "")
    {
        DefAssembly = _assembly;
        DefNamespace = _namespace;
    }

    public IntPtr AddFlag(string _class)
    {
        return Add(DefAssembly, DefNamespace, _class, "Start", 0, 1, 14,
            new byte[] { 0x48, 0x83, 0x05, 0xF0, 0xFF, 0xFF, 0xFF, 0x01 });
    }

    public IntPtr AddFlag(string _class, string _method)
    {
        return Add(DefAssembly, DefNamespace, _class, _method, 0, 1, 14,
            new byte[] { 0x48, 0x83, 0x05, 0xF0, 0xFF, 0xFF, 0xFF, 0x01 });
    }

    public IntPtr AddFlag(string _class, string _method, short overwriteSize)
    {
        return Add(DefAssembly, DefNamespace, _class, _method, 0, 1, overwriteSize,
            new byte[] { 0x48, 0x83, 0x05, 0xF0, 0xFF, 0xFF, 0xFF, 0x01 });
    }

    public IntPtr AddFlag(string _class, string _method, short paramCount, short hookOffset, short overwriteSize)
    {
        return Add(DefAssembly, DefNamespace, _class, _method, paramCount, hookOffset, overwriteSize,
            new byte[] { 0x48, 0x83, 0x05, 0xF0, 0xFF, 0xFF, 0xFF, 0x01 });
    }

    public IntPtr AddInst(string _class)
    {
        return Add(DefAssembly, DefNamespace, _class, "Update", 0, 1, 14,
            new byte[] { 0x48, 0x89, 0x3D, 0xF1, 0xFF, 0xFF, 0xFF, 0x90 });
    }

    public IntPtr AddInst(string _class, string _method)
    {
        return Add(DefAssembly, DefNamespace, _class, _method, 0, 1, 14,
            new byte[] { 0x48, 0x89, 0x3D, 0xF1, 0xFF, 0xFF, 0xFF, 0x90 });
    }

    public IntPtr AddInst(string _class, string _method, short overwriteSize)
    {
        return Add(DefAssembly, DefNamespace, _class, _method, 0, 1, overwriteSize,
            new byte[] { 0x48, 0x89, 0x3D, 0xF1, 0xFF, 0xFF, 0xFF, 0x90 });
    }

    public IntPtr AddInst(string _namespace, string _class, string _method, short overwriteSize)
    {
        return Add(DefAssembly, _namespace, _class, _method, 0, 1, overwriteSize,
            new byte[] { 0x48, 0x89, 0x3D, 0xF1, 0xFF, 0xFF, 0xFF, 0x90 });
    }

    public IntPtr Add(string _class, string _method, short paramCount, short hookOffset, short overwriteSize, byte[] bytes)
    {
        return Add(DefAssembly, DefNamespace, _class, _method, paramCount, hookOffset, overwriteSize, bytes);
    }

    public IntPtr Add(string _assembly, string _namespace, string _class, string _method, short paramCount,
    short hookOffset, short overwriteSize, byte[] bytes)
    {
        try
        {
            if (CheckSetProcessAndValues())
            {
                // ---
                if (!_assembly.EndsWith(".dll")) _assembly += ".dll";
                string exeDir = Path.GetDirectoryName(Instance.MainModule.FileName);
                string assemblyPath = UPath.FindFile(exeDir, _assembly);
                string assemblyRelativePath = assemblyPath.Replace(exeDir, "");

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

                RefWriteBytes(Instance, Cat + 0x8, all);
                RefWriteBytes(Instance, Cat + 0x2, BitConverter.GetBytes((short)all.Length));

                Cat += 0x8 + (ulong)all.Length;
                return (IntPtr)((Lion += 0x100) - 0x100);
            }
        }
        catch { }
        return IntPtr.Zero;
    }

    public Unity2()
    {
        try
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

            Dog = RefAllocateMemory(Instance, 0x6000);
            if (Dog != 0)
            {
                USaves.Set(instName, Dog.ToString());
                RefWriteBytes(Instance, Dog, AsmBlocks.UnityCPP_JitSave);
                RefCreateThread(Instance, Dog + 0x8);
                Cat = Dog + 0x2000;
                Lion = Dog + 0x3002;
            }
        }
        catch { }
    }

    private void SendMes(string message)
    {
        UProgram.Print(message);
    }
}