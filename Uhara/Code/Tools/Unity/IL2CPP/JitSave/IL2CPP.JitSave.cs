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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

public partial class Tools : MainShared
{
    public partial class Unity
    {
        public partial class IL2CPP
        {
            public class JitSave
            {
                private ulong AllocSize = 0x10000;

                private byte[] AsmAdd1RelativeStorage = new byte[] { 0x48, 0x83, 0x05, 0xF0, 0xFF, 0xFF, 0xFF, 0x01 }; // add [rip-8], 1
                private byte[] AsmMovRdiRelativeStorage = new byte[] { 0x48, 0x89, 0x3D, 0xF1, 0xFF, 0xFF, 0xFF, 0x90 }; // mov [rip-8], rdi

                public class Offsets
                {
                    public static readonly ulong NativeCode = 0x0;
                    public static readonly ulong NativeData = 0x1000;

                    public static readonly ulong BarrierSeparator = 0x2000;

                    public static readonly ulong InterfaceArguments = 0x2000;
                    public static readonly ulong InterfaceData = 0x3000;
                    public static readonly ulong InterfaceCode = 0x4000;
                    public static readonly ulong GlobalOutput = 0x5000;
                }

                private class QueueItem
                {
                    public ulong HookAddress = 0;
                    public short HookOffset = 0;
                    public short OverwriteSize = 0;
                    public byte[] Bytes = new byte[0];

                    public QueueItem(ulong address, short hookOffset, short overwriteSize, byte[] bytes)
                    {
                        HookAddress = address;
                        HookOffset = hookOffset;
                        OverwriteSize = overwriteSize;
                        Bytes = bytes;
                    }
                }

                List<QueueItem> QueueItems = new List<QueueItem>();

                ulong AllocateStart = 0;
                ulong NativeCode = 0;
                ulong InterfaceArguments = 0;
                ulong GlobalOutput = 0;

                private string DefAssembly = "Assembly-CSharp.dll";
                private string DefNamespace = "";

                public void SetOuter(string _assembly, string _namespace = "")
                {
                    DefAssembly = _assembly;
                    DefNamespace = _namespace;
                }

                public IntPtr AddFlag(string _class)
                {
                    return Add(DefAssembly, DefNamespace, _class, "Start", 0, 0, 0, AsmAdd1RelativeStorage);
                }

                public IntPtr AddFlag(string _class, string _method)
                {
                    return Add(DefAssembly, DefNamespace, _class, _method, -1, 0, 0, AsmAdd1RelativeStorage);
                }

                public IntPtr AddFlag(string _class, string _method, short overwriteSize)
                {
                    return Add(DefAssembly, DefNamespace, _class, _method, -1, 0, overwriteSize,
                        AsmAdd1RelativeStorage);
                }

                public IntPtr AddFlag(string _class, string _method, short paramCount, short hookOffset, short overwriteSize)
                {
                    return Add(DefAssembly, DefNamespace, _class, _method, paramCount, hookOffset,
                        overwriteSize, AsmAdd1RelativeStorage);
                }

                public IntPtr AddInst(string _class)
                {
                    return Add(DefAssembly, DefNamespace, _class, "Update", 0, 0, 0, AsmMovRdiRelativeStorage);
                }

                public IntPtr AddInst(string _class, short overwriteSize)
                {
                    return Add(DefAssembly, DefNamespace, _class, "Update", 0, 0, overwriteSize,
                        AsmMovRdiRelativeStorage);
                }

                public IntPtr AddInst(string _class, string _method)
                {
                    return Add(DefAssembly, DefNamespace, _class, _method, -1, 0, 0,
                        AsmMovRdiRelativeStorage);
                }

                public IntPtr AddInst(string _class, string _method, short overwriteSize)
                {
                    return Add(DefAssembly, DefNamespace, _class, _method, -1, 0, overwriteSize,
                        AsmMovRdiRelativeStorage);
                }

                public IntPtr AddInst(string _class, string _method, short paramCount, short hookOffset, short overwriteSize)
                {
                    return Add(DefAssembly, DefNamespace, _class, _method, paramCount, hookOffset, overwriteSize,
                        AsmMovRdiRelativeStorage);
                }

                public IntPtr AddInst(string _namespace, string _class, string _method, short overwriteSize)
                {
                    return Add(DefAssembly, _namespace, _class, _method, -1, 0, overwriteSize,
                        AsmMovRdiRelativeStorage);
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
                        // ---
                        if (!_assembly.EndsWith(".dll")) _assembly += ".dll";

                        byte[] arg1 = TUtils.StringToMultibyte(_assembly);
                        byte[] arg2 = TUtils.StringToMultibyte(_namespace);
                        byte[] arg3 = TUtils.StringToMultibyte(_class);
                        byte[] arg4 = TUtils.StringToMultibyte(_method);
                        byte[] arg5 = BitConverter.GetBytes(paramCount);

                        byte[] _offset = BitConverter.GetBytes(hookOffset);
                        byte[] _overwriteSize = BitConverter.GetBytes(overwriteSize);
                        byte[] _bytesSize = BitConverter.GetBytes((short)bytes.Length);

                        // ---
                        byte[] all = TArray.Merge(
                            BitConverter.GetBytes((short)arg1.Length), arg1,
                            BitConverter.GetBytes((short)arg2.Length), arg2,
                            BitConverter.GetBytes((short)arg3.Length), arg3,
                            BitConverter.GetBytes((short)arg4.Length), arg4,
                            BitConverter.GetBytes((short)arg5.Length), arg5);

                        RefWriteBytes(Instance, InterfaceArguments + 0x8, all);
                        RefWriteBytes(Instance, InterfaceArguments + 0x2, BitConverter.GetBytes((short)all.Length));

                        QueueItems.Add(new QueueItem(GlobalOutput + 0x8 + 0x2, hookOffset, overwriteSize, bytes));
                        InterfaceArguments += 0x8 + (ulong)all.Length;
                        GlobalOutput += 0x100;

                        return (IntPtr)(GlobalOutput - 0x100 + 0x2);
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
                        TUtils.Print("Waiting for thread to return");

                        if (TProcess.WaitForThread(TProcess.CreateRemoteThread(Instance, NativeCode + 0x8), 30000))
                        {
                            int hooked = 0;
                            foreach (QueueItem item in QueueItems)
                            {
                                ulong funcAddress = TMemory.ReadMemory<ulong>(Instance, item.HookAddress);
                                if (funcAddress == 0) continue;

                                funcAddress += (ulong)item.HookOffset;

                                RefWriteBytes(Instance, item.HookAddress, BitConverter.GetBytes((ulong)0));

                                int minimumOverwrite = item.OverwriteSize;
                                if (minimumOverwrite == 0) minimumOverwrite =
                                        TInstruction.GetMinimumOverwrite(Instance, funcAddress, 14);

                                byte[] realCode = TMemory.ReadMemoryBytes(Instance, funcAddress, minimumOverwrite);
                                if (realCode == null) continue;
                                byte[] toRecover = realCode.ToList().ToArray();

                                realCode = FixCmp(funcAddress, realCode);
                                realCode = FixJump(funcAddress, realCode);

                                RefWriteBytes(Instance, item.HookAddress, item.Bytes);
                                ulong nextAddress = item.HookAddress + (ulong)item.Bytes.Length;

                                RefWriteBytes(Instance, nextAddress, realCode);
                                nextAddress += (ulong)realCode.Length;

                                TMemory.CreateAbsoluteJump(Instance, nextAddress, funcAddress + (ulong)minimumOverwrite);

                                byte[] jumpIn = TMemory.GetAbsoluteJumpBytes(item.HookAddress);
                                MemoryManager.AddOverwrite(funcAddress, toRecover);
                                TMemory.CreateAbsoluteJump(Instance, funcAddress, item.HookAddress);

                                hooked++;
                            }

                            TUtils.Print(hooked.ToString() + "/" + QueueItems.Count + " functions hooked successfuly");
                        }
                    }
                    catch { }
                }

                private byte[] FixJump(ulong original, byte[] bytes)
                {
                    List<byte[]> chunks = new List<byte[]>();
                    Instruction[] instrs = TInstruction.GetInstructions2(bytes);

                    Dictionary<string, byte[]> jumpPairs = new Dictionary<string, byte[]>
                    {
                        { "jz", new byte[]  { 0x74, 0x02, 0xEB, 0x0E } }, // jump short if equal (je)
                        { "jnz", new byte[] { 0x75, 0x02, 0xEB, 0x0E } }, // jump short if not equal (jne)
                        { "jge", new byte[] { 0x7D, 0x02, 0xEB, 0x0E } }, // jump short if not less (greater or equal) (jnl)
                        { "jle", new byte[] { 0x7E, 0x02, 0xEB, 0x0E } }, // jump short if not less (greater or equal) (jle)
                        { "jl", new byte[]  { 0x7C, 0x02, 0xEB, 0x0E } }, // jump short if not less (greater or equal) (jl)
                        { "jg", new byte[]  { 0x7F, 0x02, 0xEB, 0x0E } }, // jump short if not less (greater or equal) (jg)
                        { "jmp", new byte[] { 0xEB, 0x02, 0xEB, 0x0E } }, // jump short (jmp)
                    };

                    byte[] absJump = new byte[] { 0xFF, 0x25, 0x00, 0x00, 0x00, 0x00,
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

                    int offset = 0;
                    for (int i = 0; i < instrs.Length; i++)
                    {
                        string insTxt = instrs[i].ToString();
                        int insLen = instrs[i].Bytes.Length;

                        if (insTxt.StartsWith("j"))
                        {
                            foreach (var jump in jumpPairs)
                            {
                                if (insTxt.StartsWith(jump.Key))
                                {
                                    ulong address = original + (ulong)offset;
                                    int value = 0;

                                    if (insLen == 2) value = instrs[i].Bytes[1];
                                    else if (insLen == 5) value = BitConverter.ToInt32(instrs[i].Bytes, 1);
                                    else break;

                                    address += (ulong)(instrs[i].Bytes.Length + value);

                                    byte[] together = jump.Value.Concat(absJump).ToArray();
                                    TArray.Insert(together, BitConverter.GetBytes(address), 10);

                                    chunks.Add(together);
                                    offset += together.Length;
                                    continue;
                                }
                            }
                        }

                        chunks.Add(instrs[i].Bytes);
                        offset += instrs[i].Bytes.Length;
                    }

                    return TArray.Merge(chunks.ToArray());
                }

                private byte[] FixCmp(ulong original, byte[] bytes)
                {
                    List<byte[]> chunks = new List<byte[]>();
                    Instruction[] instrs = TInstruction.GetInstructions2(bytes);
                    int requiredInstructionLength = 7; // cmp byte [rip+n], 0

                    // mov rax, address
                    // cmp byte ptr [rax], 0
                    byte[] modified = new byte[] { 0x48, 0xB8, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                        0xFF, 0xFF, 0xFF, 0x80, 0x38, 0x00, 0x90 };

                    int offset = 0;
                    for (int i = 0; i < instrs.Length; i++)
                    {
                        string insTxt = instrs[i].ToString();
                        if (instrs[i].Bytes.Length == requiredInstructionLength)
                        {
                            if (insTxt.StartsWith("cmp byte [rip+0x") && insTxt.EndsWith("], 0x0"))
                            {
                                ulong realValue = BitConverter.ToUInt32(instrs[i].Bytes, 2);
                                ulong readAddress = original + realValue + (ulong)(offset + instrs[i].Bytes.Length);
                                TArray.Insert(modified, BitConverter.GetBytes(readAddress), 2);

                                chunks.Add(modified);
                                offset += modified.Length;
                                continue;
                            }
                        }

                        chunks.Add(instrs[i].Bytes);
                        offset += instrs[i].Bytes.Length;
                    }

                    return TArray.Merge(chunks.ToArray());
                }

                public JitSave()
                {
                    try
                    {
                        AllocateStart = MemoryManager.AllocateSafe((int)AllocSize);
                        if (AllocateStart != 0)
                        {
                            NativeCode = AllocateStart + Offsets.NativeCode;
                            InterfaceArguments = AllocateStart + Offsets.InterfaceArguments;
                            GlobalOutput = AllocateStart + Offsets.GlobalOutput;
                            QueueItems.Clear();

                            byte[] asmDecoded = DecodeAsmBlock(AsmBlocks.UnityCPP_JitSave);
                            RefWriteBytes(Instance, NativeCode, asmDecoded);
                        }
                    }
                    catch { TUtils.Print("Creating tool failed"); }
                }

                private byte[] DecodeAsmBlock(byte[] asmBlock)
                {
                    List<byte> decoded = new List<byte>();
                    for (int i = 0; i < asmBlock.Length; i++)
                        if (i % 2 == 0) decoded.Add(asmBlock[i]);

                    return decoded.ToArray();
                }
            }
        }
    }
}