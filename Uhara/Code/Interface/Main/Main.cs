﻿using LiveSplit.ComponentUtil;
using LiveSplit.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class Main : MainShared
{
    public ScriptSettings Settings = new ScriptSettings();

    public Main()
    {
        try
        {
            TSaves2.Register("rumii", "uhara");
            UniqueScriptLoadID = TUtils.GenerateRandomString(32);
            TSaves2.Set(UniqueScriptLoadID, "IDs", "UniqueScriptLoadID");

            CheckSetProcessAndValues();

            // ---
            Vars.Uhara = this;
            Vars.Resolver = new PtrResolver();

            // ---
            Assembly liveSplitAssembly = Assembly.Load("LiveSplit.Core");
            Type extensionMethodsType = liveSplitAssembly.GetType("LiveSplit.ComponentUtil.ExtensionMethods");

            _RefAllocateMemory = extensionMethodsType.GetMethod("AllocateMemory", new Type[] { typeof(Process), typeof(int) });
            _RefReadBytes = extensionMethodsType.GetMethod("ReadBytes", new Type[] { typeof(Process), typeof(IntPtr), typeof(int) });
            _RefWriteBytes = extensionMethodsType.GetMethod("WriteBytes", new Type[] { typeof(Process), typeof(IntPtr), typeof(byte[]) });
            _RefCreateThread = extensionMethodsType.GetMethod("CreateThread", new Type[] { typeof(Process), typeof(IntPtr) });

            // ---
            if (!File.Exists("SharpDisasm.dll"))
                File.WriteAllBytes("SharpDisasm.dll", AsmBlocks.SharpDisasm);
        }
        catch { }
        DebugMode = false;
    }

    public void Log(string message)
    {
        try
        {
            if (!string.IsNullOrEmpty(message))
                TImports.OutputDebugString("[UHARA] " + message);
        }
        catch { }
    }

    public void AlertLoadless()
    {
        try
        {
            if (CurrentState.CurrentTimingMethod == TimingMethod.RealTime)
            {
                if (MessageBox.Show("This autosplitter recommends using GameTime, you're currently using RealTime comparison method, do you want to switch to GameTime?", "LiveSplit",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    CurrentState.CurrentTimingMethod = TimingMethod.GameTime;
                }
            }
        }
        catch { }
    }

    public void EnableDebug()
    {
        try
        {
            DebugMode = true;
        }
        catch { }
    }

    public void Update()
    {
        UpdateCounter++;

        try
        {
            do
            {
                if (ProcessInstance != null && !ProcessInstance.HasExited && ProcessInstance.Handle != IntPtr.Zero)
                {
                    foreach (var watcher in MemoryWatchers)
                    {
                        watcher.Update(ProcessInstance);
                        current[watcher.Name] = watcher.Current;
                    }

                    foreach (var watcher in StringWatchers)
                    {
                        watcher.Update(ProcessInstance);
                        current[watcher.Name] = watcher.Current;
                    }

                    foreach (var watcher in ListWatchers)
                    {
                        watcher.memoryWatcher.Update(ProcessInstance);
                        if ((IntPtr)watcher.memoryWatcher.Current == IntPtr.Zero) continue;

                        ulong address = (ulong)watcher.memoryWatcher.Current;
                        Type type = watcher.type;

                        int size = TMemory.ReadMemory<ushort>(ProcessInstance, address + 0x18);

                        address = TMemory.ReadMemory<ulong>(ProcessInstance, address + 0x10);
                        if (address == 0) continue;

                        int count = TMemory.ReadMemory<ushort>(ProcessInstance, address + 0x18);
                        if (count > size) continue;

                        // ---
                        if (type == typeof(IntPtr))
                        {
                            List<IntPtr> list = new List<IntPtr>();
                            int itemSize = Marshal.SizeOf(typeof(IntPtr));

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<IntPtr>(ProcessInstance, address + (ulong)i));
                            }
                        }

                        else if (type == typeof(bool))
                        {
                            List<bool> list = new List<bool>();
                            int itemSize = sizeof(bool);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<bool>(ProcessInstance, address + (ulong)i));
                            }
                        }

                        else if (type == typeof(byte))
                        {
                            List<byte> list = new List<byte>();
                            int itemSize = sizeof(byte);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<byte>(ProcessInstance, address + (ulong)i));
                            }
                        }

                        else if (type == typeof(sbyte))
                        {
                            List<sbyte> list = new List<sbyte>();
                            int itemSize = sizeof(sbyte);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<sbyte>(ProcessInstance, address + (ulong)i));
                            }
                        }

                        else if (type == typeof(char))
                        {
                            List<char> list = new List<char>();
                            int itemSize = sizeof(char);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<char>(ProcessInstance, address + (ulong)i));
                            }
                        }

                        else if (type == typeof(short))
                        {
                            List<short> list = new List<short>();
                            int itemSize = sizeof(short);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<short>(ProcessInstance, address + (ulong)i));
                            }
                        }

                        else if (type == typeof(ushort))
                        {
                            List<ushort> list = new List<ushort>();
                            int itemSize = sizeof(ushort);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<ushort>(ProcessInstance, address + (ulong)i));
                            }
                        }

                        else if (type == typeof(int))
                        {
                            List<int> list = new List<int>();
                            int itemSize = sizeof(int);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<int>(ProcessInstance, address + (ulong)i));
                            }
                        }

                        else if (type == typeof(uint))
                        {
                            List<uint> list = new List<uint>();
                            int itemSize = sizeof(uint);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<uint>(ProcessInstance, address + (ulong)i));
                            }
                        }

                        else if (type == typeof(long))
                        {
                            List<long> list = new List<long>();
                            int itemSize = sizeof(long);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<long>(ProcessInstance, address + (ulong)i));
                            }
                        }

                        else if (type == typeof(ulong))
                        {
                            List<ulong> list = new List<ulong>();
                            int itemSize = sizeof(ulong);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<ulong>(ProcessInstance, address + (ulong)i));
                            }
                        }

                        else if (type == typeof(float))
                        {
                            List<float> list = new List<float>();
                            int itemSize = sizeof(float);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<float>(ProcessInstance, address + (ulong)i));
                            }
                        }

                        else if (type == typeof(double))
                        {
                            List<double> list = new List<double>();
                            int itemSize = sizeof(double);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<double>(ProcessInstance, address + (ulong)i));
                            }
                        }

                        else if (type == typeof(decimal))
                        {
                            List<decimal> list = new List<decimal>();
                            int itemSize = sizeof(decimal);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<decimal>(ProcessInstance, address + (ulong)i));
                            }
                        }
                    }
                }
            }
            while (false);
        }
        catch { }
    }

    public dynamic CreateTool(string engine, string tool)
    {
        try
        {
            return CreateTool(engine, "default", tool);
        }
        catch { }
        return null;
    }

    public dynamic CreateTool(string engine, string type, string tool)
    {
        try
        {
            TProcess.WaitTillSecondsOld(ProcessInstance, 3);
            if (!ReloadProcess()) throw new Exception();

            engine = engine.ToLower();
            type = type.ToLower();
            tool = tool.ToLower();

            if (ToolsShared.ToolNames.Unity.Data.Contains(engine))
            {
                if (ToolsShared.ToolNames.Unity.DotNet.Data.Contains(type))
                {
                    if (ToolsShared.ToolNames.Unity.DotNet.JitSave.Data.Contains(tool))
                    {
                        return new Tools.Unity.DotNet.JitSave();
                    }

                    if (ToolsShared.ToolNames.Unity.DotNet.Instance.Data.Contains(tool))
                    {
                        return new Tools.Unity.DotNet.Instance();
                    }
                }

                if (ToolsShared.ToolNames.Unity.Il2Cpp.Data.Contains(type))
                {
                    if (ToolsShared.ToolNames.Unity.Il2Cpp.JitSave.Data.Contains(tool))
                    {
                        return new Tools.Unity.IL2CPP.JitSave();
                    }

                    if (ToolsShared.ToolNames.Unity.Il2Cpp.Instance.Data.Contains(tool))
                    {
                        return new Tools.Unity.IL2CPP.Instance();
                    }
                }

                if (ToolsShared.ToolNames.Unity.Utils.Data.Contains(tool))
                {
                    return new Tools.Unity.Utilities();
                }
            }

            if (ToolsShared.ToolNames.UnrealEngine.Data.Contains(engine))
            {
                if (ToolsShared.ToolNames.UnrealEngine.Default.Data.Contains(type))
                {
                    if (ToolsShared.ToolNames.UnrealEngine.Default.Events.Data.Contains(tool))
                    {
                        return new Tools.UnrealEngine.Default.Events();
                    }
                }
            }

            if (ToolsShared.ToolNames.UnrealEngine.Data.Contains(engine))
            {
                if (ToolsShared.ToolNames.UnrealEngine.Default.Data.Contains(type))
                {
                    if (ToolsShared.ToolNames.UnrealEngine.Default.CutsceneManager.Data.Contains(tool))
                    {
                        //return new Tools.UnrealEngine.Default.CutsceneManager();
                    }
                }
            }
        }
        catch { Thread.Sleep(500); }
        return null;
    }

    public void SetProcess(Process process)
    {
        try
        {
            ProcessInstance = process;
        }
        catch { }
    }
}