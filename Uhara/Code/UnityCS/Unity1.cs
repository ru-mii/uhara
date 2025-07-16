using SharpDisasm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

public class Unity1 : UShared
{
    private int AllocSize = 0x6000;

    public class QueueItem
    {
        public ulong Address = 0;
        public short HookOffset = 0;
        public short OverwriteSize = 0;
        public byte[] Bytes = new byte[0];

        public QueueItem(ulong address, short hookOffset, short overwriteSize, byte[] bytes)
        {
            Address = address;
            HookOffset = hookOffset;
            OverwriteSize = overwriteSize;
            Bytes = bytes;
        }
    }

    List<QueueItem> QueueItems = new List<QueueItem>();

    ulong Allocated = 0;
    ulong Arguments = 0;
    ulong Output = 0;

    private string DefAssembly = "Assembly-CSharp.dll";
    private string DefNamespace = "";

    public void SetOuter(string _assembly, string _namespace = "")
    {
        DefAssembly = _assembly;
        DefNamespace = _namespace;
    }

    public IntPtr AddFlag(string _class)
    {
        return Add(DefAssembly, DefNamespace, _class, "Start", 0, 0, 0,
            new byte[] { 0x48, 0x83, 0x05, 0xF0, 0xFF, 0xFF, 0xFF, 0x01 });
    }

    public IntPtr AddFlag(string _class, string _method)
    {
        return Add(DefAssembly, DefNamespace, _class, _method, -1, 0, 0,
            new byte[] { 0x48, 0x83, 0x05, 0xF0, 0xFF, 0xFF, 0xFF, 0x01 });
    }

    public IntPtr AddFlag(string _class, string _method, short overwriteSize)
    {
        return Add(DefAssembly, DefNamespace, _class, _method, -1, 0, overwriteSize,
            new byte[] { 0x48, 0x83, 0x05, 0xF0, 0xFF, 0xFF, 0xFF, 0x01 });
    }

    public IntPtr AddFlag(string _class, string _method, short paramCount, short hookOffset, short overwriteSize)
    {
        return Add(DefAssembly, DefNamespace, _class, _method, paramCount, hookOffset, overwriteSize,
            new byte[] { 0x48, 0x83, 0x05, 0xF0, 0xFF, 0xFF, 0xFF, 0x01 });
    }

    public IntPtr AddInst(string _class)
    {
        return Add(DefAssembly, DefNamespace, _class, "Update", 0, 0, 0,
            new byte[] { 0x48, 0x89, 0x3D, 0xF1, 0xFF, 0xFF, 0xFF, 0x90 });
    }

    public IntPtr AddInst(string _class, short overwriteSize)
    {
        return Add(DefAssembly, DefNamespace, _class, "Update", 0, 0, overwriteSize,
            new byte[] { 0x48, 0x89, 0x3D, 0xF1, 0xFF, 0xFF, 0xFF, 0x90 });
    }

    public IntPtr AddInst(string _class, string _method)
    {
        return Add(DefAssembly, DefNamespace, _class, _method, -1, 0, 0,
            new byte[] { 0x48, 0x89, 0x3D, 0xF1, 0xFF, 0xFF, 0xFF, 0x90 });
    }

    public IntPtr AddInst(string _class, string _method, short overwriteSize)
    {
        return Add(DefAssembly, DefNamespace, _class, _method, -1, 0, overwriteSize,
            new byte[] { 0x48, 0x89, 0x3D, 0xF1, 0xFF, 0xFF, 0xFF, 0x90 });
    }

    public IntPtr AddInst(string _class, string _method, short paramCount, short hookOffset, short overwriteSize)
    {
        return Add(DefAssembly, DefNamespace, _class, _method, paramCount, hookOffset, overwriteSize,
            new byte[] { 0x48, 0x89, 0x3D, 0xF1, 0xFF, 0xFF, 0xFF, 0x90 });
    }

    public IntPtr AddInst(string _namespace, string _class, string _method, short overwriteSize)
    {
        return Add(DefAssembly, _namespace, _class, _method, -1, 0, overwriteSize,
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

                if (assemblyPath == "") return IntPtr.Zero;

                string assemblyRelativePath = assemblyPath.Replace(exeDir, "");
                assemblyRelativePath = assemblyRelativePath.Substring(1);

                byte[] arg1 = UProgram.StringToMultibyte(assemblyRelativePath);
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

                QueueItems.Add(new QueueItem(Output + 0x8, hookOffset, overwriteSize, bytes));
                Arguments += 0x8 + (ulong)all.Length;
                return (IntPtr)((Output += 0x100) - 0x100);
            }
        }
        catch { }
        return IntPtr.Zero;
    }

    public void ProcessQueue()
    {
        new Thread(_ProcessQueue).Start();
    }

    private void _ProcessQueue()
    {
        try
        {
            UProgram.Print("Waiting for thread to return");

            if (UProcess.WaitForThread(UProcess.CreateRemoteThread(Instance, Allocated + 0x8), 30000))
            {
                int hooked = 0;
                foreach (QueueItem item in QueueItems)
                {
                    ulong funcAddress = UMemory.ReadMemory<ulong>(Instance, item.Address);
                    if (funcAddress == 0) continue;

                    funcAddress += (ulong)item.HookOffset;

                    RefWriteBytes(Instance, item.Address, BitConverter.GetBytes((ulong)0));

                    int minimumOverwrite = item.OverwriteSize;
                    if (minimumOverwrite == 0) minimumOverwrite =
                            UInstruction.GetMinimumOverwrite(Instance, funcAddress, 14);

                    byte[] realCode = UMemory.ReadMemoryBytes(Instance, funcAddress, minimumOverwrite);
                    if (realCode == null) continue;

                    RefWriteBytes(Instance, item.Address, item.Bytes);
                    ulong nextAddress = item.Address + (ulong)item.Bytes.Length;
                    RefWriteBytes(Instance, nextAddress, realCode);
                    //try { UMemory.FixRelative(Instance, funcAddress, nextAddress, realCode.Length); } catch { }
                    nextAddress += (ulong)realCode.Length;

                    UMemory.CreateAbsoluteJump(Instance, nextAddress, funcAddress + (ulong)minimumOverwrite);
                    UMemory.CreateAbsoluteJump(Instance, funcAddress, item.Address);
                    hooked++;
                }

                UProgram.Print(hooked.ToString() + "/" + QueueItems.Count + " functions hooked successfuly");
            }
        }
        catch { }
    }

    public Unity1()
    {
        try
        {
            string instName = ToolNames.Unity.UnityCS[0] + "." + ToolNames.Unity.Modules.JitSave[0];

            ulong lastAddress = 0;
            if (ulong.TryParse(USaves.Get(instName), out lastAddress) && lastAddress != 0)
            {
                byte[] sigTest = UMemory.ReadMemoryBytes(Instance, lastAddress, 0x8);
                if (sigTest != null)
                {
                    byte[] sigCheck = new byte[] { 0x5E, 0x9B, 0xDB, 0x02, 0xF7, 0x39, 0x1C, 0x02 };
                    if (sigCheck.SequenceEqual(sigTest))
                    {
                        try
                        {
                            RefWriteBytes(Instance, lastAddress, BitConverter.GetBytes((ulong)0));
                            UMemory.FreeMemory(Instance, lastAddress, AllocSize);
                        }
                        catch { }
                    }
                }
            }

            Allocated = RefAllocateMemory(Instance, AllocSize);
            if (Allocated != 0)
            {
                USaves.Set(instName, Allocated.ToString());

                byte[] asmDecoded = DecodeAsmBlock(AsmBlocks.UnityCS_JitSave);
                RefWriteBytes(Instance, Allocated, asmDecoded);

                Arguments = Allocated + 0x2000;
                Output = Allocated + 0x3002;
                QueueItems.Clear();
            }
        }
        catch { UProgram.Print("Creating tool failed"); }
    }

    private byte[] DecodeAsmBlock(byte[] asmBlock)
    {
        List<byte> decoded = new List<byte>();
        for (int i = 0; i < asmBlock.Length; i++)
            if (i % 2 == 0) decoded.Add(asmBlock[i]);

        return decoded.ToArray();
    }
}