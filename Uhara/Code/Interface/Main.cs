using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class Main : MainShared
{
    public Main()
    {
        try
        {
            CheckSetProcessAndValues();
            TSaves2.Register("rumii", "uhara");
            Vars.Uhara = this;

            Assembly liveSplitAssembly = Assembly.Load("LiveSplit.Core");
            Type extensionMethodsType = liveSplitAssembly.GetType("LiveSplit.ComponentUtil.ExtensionMethods");

            _RefAllocateMemory = extensionMethodsType.GetMethod("AllocateMemory", new Type[] { typeof(Process), typeof(int) });
            _RefReadBytes = extensionMethodsType.GetMethod("ReadBytes", new Type[] { typeof(Process), typeof(IntPtr), typeof(int) });
            _RefWriteBytes = extensionMethodsType.GetMethod("WriteBytes", new Type[] { typeof(Process), typeof(IntPtr), typeof(byte[]) });
            _RefCreateThread = extensionMethodsType.GetMethod("CreateThread", new Type[] { typeof(Process), typeof(IntPtr) });

            UniqueScriptLoadID = TProgram.GenerateRandomString(32);
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
            if (CheckSetProcessAndValues())
            {
                if (!File.Exists("SharpDisasm.dll"))
                    File.WriteAllBytes("SharpDisasm.dll", AsmBlocks.SharpDisasm);

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
                    }

                    else if (ToolsShared.ToolNames.Unity.Il2Cpp.Data.Contains(type))
                    {
                        if (ToolsShared.ToolNames.Unity.Il2Cpp.JitSave.Data.Contains(tool))
                        {
                            return new Tools.Unity.Il2Cpp.JitSave();
                        }
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
        }
        catch { }
        return null;
    }

    public void SetProcess(Process process)
    {
        try
        {
            Instance = process;
        }
        catch { }
    }
}