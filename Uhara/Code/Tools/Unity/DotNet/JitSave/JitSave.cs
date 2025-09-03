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

public partial class Tools : MainShared
{
    public partial class Unity
    {
        public partial class DotNet
        {
            public class JitSave
            {
                private ulong AllocSize = 0x10000;

                // add [rip-8], 1
                private byte[] AsmAdd1RelativeStorage = new byte[] { 0x48, 0x83, 0x05, 0xF0, 0xFF, 0xFF, 0xFF, 0x01 };

                // mov [rip-8], rdi
                private byte[] AsmMovRdiRelativeStorage = new byte[] { 0x48, 0x89, 0x3D, 0xF1, 0xFF, 0xFF, 0xFF, 0x90 };

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

                private ulong AllocateStart = 0;
                private ulong NativeCode = 0;
                private ulong NativeData = 0;
                private ulong InterfaceArguments = 0;
                private ulong InterfaceData = 0;
                private ulong InterfaceCode = 0;
                private ulong GlobalOutput = 0;

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

                        string exeDir = Path.GetDirectoryName(Instance.MainModule.FileName);
                        string assemblyPath = UPath.FindFile(exeDir, _assembly);

                        if (assemblyPath == "") return IntPtr.Zero;

                        string assemblyRelativePath = assemblyPath.Replace(exeDir, "");
                        assemblyRelativePath = assemblyRelativePath.Substring(1);

                        byte[] arg1 = TProgram.StringToMultibyte(assemblyRelativePath);
                        byte[] arg2 = TProgram.StringToMultibyte(_namespace);
                        byte[] arg3 = TProgram.StringToMultibyte(_class);
                        byte[] arg4 = TProgram.StringToMultibyte(_method);
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
                        TProgram.Print("Waiting for thread to return");

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

                                RefWriteBytes(Instance, item.HookAddress, item.Bytes);
                                ulong nextAddress = item.HookAddress + (ulong)item.Bytes.Length;
                                RefWriteBytes(Instance, nextAddress, realCode);
                                //try { UMemory.FixRelative(Instance, funcAddress, nextAddress, realCode.Length); } catch { }
                                nextAddress += (ulong)realCode.Length;

                                TMemory.CreateAbsoluteJump(Instance, nextAddress, funcAddress + (ulong)minimumOverwrite);

                                byte[] jumpIn = TMemory.GetAbsoluteJumpBytes(item.HookAddress);
                                MemoryManager.AddOverwrite(funcAddress, realCode);
                                TMemory.CreateAbsoluteJump(Instance, funcAddress, item.HookAddress);

                                hooked++;
                            }

                            TProgram.Print(hooked.ToString() + "/" + QueueItems.Count + " functions hooked successfuly");
                        }
                    }
                    catch { }
                }

                public JitSave()
                {
                    try
                    {
                        AllocateStart = MemoryManager.AllocateSafe((int)AllocSize);
                        if (AllocateStart != 0)
                        {
                            NativeCode = AllocateStart + Offsets.NativeCode;
                            NativeData = AllocateStart + Offsets.NativeData;
                            InterfaceArguments = AllocateStart + Offsets.InterfaceArguments;
                            InterfaceData = AllocateStart + Offsets.InterfaceData;
                            InterfaceCode = AllocateStart + Offsets.InterfaceCode;
                            GlobalOutput = AllocateStart + Offsets.GlobalOutput;
                            QueueItems.Clear();

                            byte[] asmDecoded = DecodeAsmBlock(AsmBlocks.UnityCS_JitSave);
                            RefWriteBytes(Instance, NativeCode, asmDecoded);
                        }
                    }
                    catch { TProgram.Print("Creating tool failed"); }
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