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
using LiveSplit.ComponentUtil;

public partial class Main : MainShared
{
    public Main()
    {
        try
        {
            TSaves2.Register("rumii", "uhara");
            UniqueScriptLoadID = TUtils.GenerateRandomString(32);
            TSaves2.Set(UniqueScriptLoadID, "IDs", "UniqueScriptLoadID");

            CheckSetProcessAndValues();
            Vars.Uhara = this;

            Assembly liveSplitAssembly = Assembly.Load("LiveSplit.Core");
            Type extensionMethodsType = liveSplitAssembly.GetType("LiveSplit.ComponentUtil.ExtensionMethods");

            _RefAllocateMemory = extensionMethodsType.GetMethod("AllocateMemory", new Type[] { typeof(Process), typeof(int) });
            _RefReadBytes = extensionMethodsType.GetMethod("ReadBytes", new Type[] { typeof(Process), typeof(IntPtr), typeof(int) });
            _RefWriteBytes = extensionMethodsType.GetMethod("WriteBytes", new Type[] { typeof(Process), typeof(IntPtr), typeof(byte[]) });
            _RefCreateThread = extensionMethodsType.GetMethod("CreateThread", new Type[] { typeof(Process), typeof(IntPtr) });

            if (!File.Exists("SharpDisasm.dll"))
                File.WriteAllBytes("SharpDisasm.dll", AsmBlocks.SharpDisasm);
        }
        catch { }
        DebugMode = false;
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
            foreach (var watcher in MemoryWatchers)
            {
                watcher.Update(ProcessInstance);
                current[watcher.Name] = watcher.Current;
            }
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
            CheckSetProcessAndValues();
            if (ProcessInstance == null) throw new Exception();
            if (!TProcess.WaitTillSecondsOld(ProcessInstance, 3)) throw new Exception();

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