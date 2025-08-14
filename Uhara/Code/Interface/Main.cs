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

public partial class Main : UShared
{
    public Main()
    {
        try
        {
            CheckSetProcessAndValues();
            USaves.Register("rumii", "uhara");
            Vars.Uhara = this;

            Assembly liveSplitAssembly = Assembly.Load("LiveSplit.Core");
            Type extensionMethodsType = liveSplitAssembly.GetType("LiveSplit.ComponentUtil.ExtensionMethods");

            _RefAllocateMemory = extensionMethodsType.GetMethod("AllocateMemory", new Type[] { typeof(Process), typeof(int) });
            _RefReadBytes = extensionMethodsType.GetMethod("ReadBytes", new Type[] { typeof(Process), typeof(IntPtr), typeof(int) });
            _RefWriteBytes = extensionMethodsType.GetMethod("WriteBytes", new Type[] { typeof(Process), typeof(IntPtr), typeof(byte[]) });
            _RefCreateThread = extensionMethodsType.GetMethod("CreateThread", new Type[] { typeof(Process), typeof(IntPtr) });

            UniqueScriptLoadID = UProgram.GenerateRandomString(12);
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
        return CreateTool(engine, "default", tool);
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

                if (ToolNames.Unity.Data.Contains(engine))
                {
                    if (ToolNames.Unity.DotNet.Data.Contains(type))
                    {
                        if (ToolNames.Unity.DotNet.JitSave.Data.Contains(tool))
                        {
                            return new Tools.Unity.DotNet.JitSave();
                        }
                    }

                    else if (ToolNames.Unity.Il2Cpp.Data.Contains(type))
                    {
                        if (ToolNames.Unity.Il2Cpp.JitSave.Data.Contains(tool))
                        {
                            return new Tools.Unity.Il2Cpp.JitSave();
                        }
                    }
                }

                else if (ToolNames.UnrealEngine.Data.Contains(engine))
                {
                    if (ToolNames.UnrealEngine.Default.Data.Contains(type))
                    {
                        if (ToolNames.UnrealEngine.Default.CatchInstance.Data.Contains(tool))
                        {
                            return new Tools.UnrealEngine.Default.CatchInstance();
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
        Instance = process;
    }
}