using SharpDisasm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public class Unity2 : UShared
{
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

                QueueItems.Add(new QueueItem(Lion + 0x8, hookOffset, overwriteSize, bytes));
                Cat += 0x8 + (ulong)all.Length;
                return (IntPtr)((Lion += 0x100) - 0x100);
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
            UProcess.WaitForThread(UProcess.CreateRemoteThread(Instance, Dog + 0x8), 30000);

            {
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

                    realCode = FixCmp(funcAddress, realCode);

                    RefWriteBytes(Instance, item.Address, item.Bytes);
                    ulong nextAddress = item.Address + (ulong)item.Bytes.Length;

                    RefWriteBytes(Instance, nextAddress, realCode);
                    nextAddress += (ulong)realCode.Length;

                    UMemory.CreateAbsoluteJump(Instance, nextAddress, funcAddress + (ulong)minimumOverwrite);
                    UMemory.CreateAbsoluteJump(Instance, funcAddress, item.Address);
                }
            }
        }
        catch { }
    }

    private byte[] FixCmp(ulong original, byte[] bytes)
    {
        List<byte[]> chunks = new List<byte[]>();
        Instruction[] instrs = UInstruction.GetInstructions2(bytes);

        byte[] modified = new byte[] { 0x50, 0x48, 0xB8, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            0xFF, 0xFF, 0xFF, 0x80, 0x38, 0x00, 0x58, 0x90 };

        int offset = 0;
        for (int i = 0; i < instrs.Length; i++)
        {
            string insTxt = instrs[i].ToString();
            if (instrs[i].Bytes.Length == 7)
            {
                if (insTxt.StartsWith("cmp byte [rip+0x") && insTxt.EndsWith("], 0x0"))
                {
                    ulong realValue = BitConverter.ToUInt32(instrs[i].Bytes, 2);
                    ulong readAddress = original + realValue + (ulong)(offset + 7);
                    UArray.Insert(modified, BitConverter.GetBytes(readAddress), 3);

                    chunks.Add(modified);
                    offset += modified.Length;
                    continue;
                }
            }

            chunks.Add(instrs[i].Bytes);
            offset += instrs[i].Bytes.Length;
        }

        return UArray.Merge(chunks.ToArray());
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

                byte[] asmDecoded = DecodeAsmBlock(AsmBlocks.UnityCPP_JitSave);
                RefWriteBytes(Instance, Dog, asmDecoded);

                Cat = Dog + 0x2000;
                Lion = Dog + 0x3002;
                QueueItems.Clear();
            }
        }
        catch { }
    }

    private byte[] DecodeAsmBlock(byte[] asmBlock)
    {
        List<byte> decoded = new List<byte>();
        for (int i = 0; i < asmBlock.Length; i++)
            if (i % 2 == 0) decoded.Add(asmBlock[i]);

        return decoded.ToArray();
    }
}