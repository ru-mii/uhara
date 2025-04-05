using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
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

    public dynamic CreateTool(string grand, string sub)
    {
        try
        {
            if (CheckSetProcessAndValues())
            {
                grand = grand.ToLower();
                sub = sub.ToLower();

                if (grand == "unitycs")
                {
                    if (sub == "jitsave")
                    {
                        return new Unity1();
                    }
                }

                else if (grand == "unitycpp")
                {
                    if (sub == "jitsave")
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