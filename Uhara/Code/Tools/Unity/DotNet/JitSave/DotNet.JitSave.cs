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

public partial class Tools
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
                private byte[] AsmMovRcxRelativeStorage = new byte[] { 0x48, 0x89, 0x0D, 0xF1, 0xFF, 0xFF, 0xFF, 0x90 };

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
                    return Add(DefAssembly, DefNamespace, _class, "Update", 0, 0, 0, AsmMovRcxRelativeStorage);
                }

                public IntPtr AddInst(string _class, short overwriteSize)
                {
                    return Add(DefAssembly, DefNamespace, _class, "Update", 0, 0, overwriteSize,
                        AsmMovRcxRelativeStorage);
                }

                public IntPtr AddInst(string _class, string _method)
                {
                    return Add(DefAssembly, DefNamespace, _class, _method, -1, 0, 0,
                        AsmMovRcxRelativeStorage);
                }

                public IntPtr AddInst(string _class, string _method, short overwriteSize)
                {
                    return Add(DefAssembly, DefNamespace, _class, _method, -1, 0, overwriteSize,
                        AsmMovRcxRelativeStorage);
                }

                public IntPtr AddInst(string _class, string _method, short paramCount, short hookOffset, short overwriteSize)
                {
                    return Add(DefAssembly, DefNamespace, _class, _method, paramCount, hookOffset, overwriteSize,
                        AsmMovRcxRelativeStorage);
                }

                public IntPtr AddInst(string _namespace, string _class, string _method, short overwriteSize)
                {
                    return Add(DefAssembly, _namespace, _class, _method, -1, 0, overwriteSize,
                        AsmMovRcxRelativeStorage);
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

                        string exeDir = Path.GetDirectoryName(Main.ProcessInstance.MainModule.FileName);
                        string assemblyPath = UPath.FindFile(exeDir, _assembly);

                        if (assemblyPath == "") return IntPtr.Zero;

                        string assemblyRelativePath = assemblyPath.Replace(exeDir, "");
                        assemblyRelativePath = assemblyRelativePath.Substring(1);

                        byte[] arg1 = TUtils.StringToMultibyte(assemblyRelativePath);
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

                        Main.RefWriteBytes(Main.ProcessInstance, InterfaceArguments + 0x8, all);
                        Main.RefWriteBytes(Main.ProcessInstance, InterfaceArguments + 0x2, BitConverter.GetBytes((short)all.Length));

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

                        if (TProcess.WaitForThread(TProcess.CreateRemoteThread(Main.ProcessInstance, NativeCode + 0x8), 30000))
                        {
                            int hooked = 0;
                            foreach (QueueItem item in QueueItems)
                            {
                                ulong funcAddress = TMemory.ReadMemory<ulong>(Main.ProcessInstance, item.HookAddress);
                                if (funcAddress == 0) continue;

                                funcAddress += (ulong)item.HookOffset;

                                Main.RefWriteBytes(Main.ProcessInstance, item.HookAddress, BitConverter.GetBytes((ulong)0));

                                int minimumOverwrite = item.OverwriteSize;
                                if (minimumOverwrite == 0) minimumOverwrite =
                                        TInstruction.GetMinimumOverwrite(Main.ProcessInstance, funcAddress, 14);

                                byte[] realCode = TMemory.ReadMemoryBytes(Main.ProcessInstance, funcAddress, minimumOverwrite);
                                if (realCode == null) continue;

                                Main.RefWriteBytes(Main.ProcessInstance, item.HookAddress, item.Bytes);
                                ulong nextAddress = item.HookAddress + (ulong)item.Bytes.Length;
                                Main.RefWriteBytes(Main.ProcessInstance, nextAddress, realCode);
                                //try { UMemory.FixRelative(Instance, funcAddress, nextAddress, realCode.Length); } catch { }
                                nextAddress += (ulong)realCode.Length;

                                TMemory.CreateAbsoluteJump(Main.ProcessInstance, nextAddress, funcAddress + (ulong)minimumOverwrite);

                                byte[] jumpIn = TMemory.GetAbsoluteJumpBytes(item.HookAddress);
                                MemoryManager.AddOverwrite(funcAddress, realCode);
                                TMemory.CreateAbsoluteJump(Main.ProcessInstance, funcAddress, item.HookAddress);

                                hooked++;
                            }

                            TUtils.Print(hooked.ToString() + "/" + QueueItems.Count + " functions hooked successfuly");
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
                            Main.RefWriteBytes(Main.ProcessInstance, NativeCode, asmDecoded);
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