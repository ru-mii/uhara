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
            Vars.Uhara = this;

            Assembly liveSplitAssembly = Assembly.Load("LiveSplit.Core");
            Type extensionMethodsType = liveSplitAssembly.GetType("LiveSplit.ComponentUtil.ExtensionMethods");

            _RefAllocateMemory = extensionMethodsType.GetMethod("AllocateMemory", new Type[] { typeof(Process), typeof(int) });
            _RefWriteBytes = extensionMethodsType.GetMethod("WriteBytes", new Type[] { typeof(Process), typeof(IntPtr), typeof(byte[]) });
            _RefCreateThread = extensionMethodsType.GetMethod("CreateThread", new Type[] { typeof(Process), typeof(IntPtr) });
        }
        catch { }
    }

    public dynamic CreateTool(string name, string module)
    {
        try
        {
            if (CheckSetProcessAndValues())
            {
                if (!File.Exists("SharpDisasm.dll"))
                    File.WriteAllBytes("SharpDisasm.dll", AsmBlocks.SharpDisasm);

                name = name.ToLower();
                module = module.ToLower();

                if (ToolNames.Unity.UnityCS.Contains(name))
                {
                    if (ToolNames.Unity.Modules.JitSave.Contains(module))
                    {
                        return new Unity1();
                    }
                }

                else if (ToolNames.Unity.UnityCPP.Contains(name))
                {
                    if (ToolNames.Unity.Modules.JitSave.Contains(module))
                    {
                        return new Unity2();
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