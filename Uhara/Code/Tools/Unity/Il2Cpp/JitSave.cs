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

public partial class Tools : UShared
{
    public partial class Unity
    {
        public partial class Il2Cpp
        {
            public class JitSave
            {
                private int EnvironmentAllocSize = 0x10000;

                private byte[] AsmAdd1RelativeStorage = new byte[] { 0x48, 0x83, 0x05, 0xF0, 0xFF, 0xFF, 0xFF, 0x01 }; // add [rip-8], 1
                private byte[] AsmMovRdiRelativeStorage = new byte[] { 0x48, 0x89, 0x3D, 0xF1, 0xFF, 0xFF, 0xFF, 0x90 }; // mov [rip-8], rdi

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

                            // ---
                            byte[] all = UArray.Merge(
                                BitConverter.GetBytes((short)arg1.Length), arg1,
                                BitConverter.GetBytes((short)arg2.Length), arg2,
                                BitConverter.GetBytes((short)arg3.Length), arg3,
                                BitConverter.GetBytes((short)arg4.Length), arg4,
                                BitConverter.GetBytes((short)arg5.Length), arg5);

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
                                ulong funcAddress = UMemory.ReadMemory<ulong>(Instance, item.HookAddress);
                                if (funcAddress == 0) continue;

                                funcAddress += (ulong)item.HookOffset;

                                RefWriteBytes(Instance, item.HookAddress, BitConverter.GetBytes((ulong)0));

                                int minimumOverwrite = item.OverwriteSize;
                                if (minimumOverwrite == 0) minimumOverwrite =
                                        UInstruction.GetMinimumOverwrite(Instance, funcAddress, 14);

                                byte[] realCode = UMemory.ReadMemoryBytes(Instance, funcAddress, minimumOverwrite);
                                if (realCode == null) continue;

                                realCode = FixCmp(funcAddress, realCode);

                                RefWriteBytes(Instance, item.HookAddress, item.Bytes);
                                ulong nextAddress = item.HookAddress + (ulong)item.Bytes.Length;

                                RefWriteBytes(Instance, nextAddress, realCode);
                                nextAddress += (ulong)realCode.Length;

                                UMemory.CreateAbsoluteJump(Instance, nextAddress, funcAddress + (ulong)minimumOverwrite);

                                byte[] jumpIn = UMemory.GetAbsoluteJumpBytes(item.HookAddress);
                                MemoryCleaner.AddOverwrite(funcAddress, jumpIn, realCode);
                                UMemory.CreateAbsoluteJump(Instance, funcAddress, item.HookAddress);

                                hooked++;
                            }

                            UProgram.Print(hooked.ToString() + "/" + QueueItems.Count + " functions hooked successfuly");
                        }
                    }
                    catch { }
                }

                private byte[] FixCmp(ulong original, byte[] bytes)
                {
                    List<byte[]> chunks = new List<byte[]>();
                    Instruction[] instrs = UInstruction.GetInstructions2(bytes);
                    
                    // mov rax, address
                    // cmp byte ptr [rax], 0
                    byte[] modified = new byte[] { 0x48, 0xB8, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                        0xFF, 0xFF, 0xFF, 0x80, 0x38, 0x00, 0x90 };

                    int offset = 0;
                    for (int i = 0; i < instrs.Length; i++)
                    {
                        string insTxt = instrs[i].ToString();
                        int requiredInstructionLength = 7; // cmp byte [rip+n], 0

                        if (instrs[i].Bytes.Length == requiredInstructionLength)
                        {
                            if (insTxt.StartsWith("cmp byte [rip+0x") && insTxt.EndsWith("], 0x0"))
                            {
                                ulong realValue = BitConverter.ToUInt32(instrs[i].Bytes, 2);
                                ulong readAddress = original + realValue + (ulong)(offset + instrs[i].Bytes.Length);
                                UArray.Insert(modified, BitConverter.GetBytes(readAddress), 2);

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

                public JitSave()
                {
                    try
                    {
                        Allocated = RefAllocateMemory(Instance, EnvironmentAllocSize);
                        if (Allocated != 0)
                        {
                            MemoryCleaner.AddAllocate(Allocated, EnvironmentAllocSize);

                            byte[] asmDecoded = DecodeAsmBlock(AsmBlocks.UnityCPP_JitSave);
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
        }
    }
}