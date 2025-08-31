using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public class MainShared
{
    internal static string ProcessName = null;
    internal static Process Instance = null;
    internal static dynamic Vars;

    public static bool DebugMode = true;

    internal static bool CleanedMemory = false;
    internal static string UniqueScriptLoadID = null;

    internal static bool CheckSetProcessAndValues()
    {
        try
        {
            dynamic currState = UReflection.GetValue(UReflection.GetValue(Application.OpenForms["TimerForm"],
                    "<CurrentState>k__BackingField"));

            if (currState != null)
            {
                dynamic script = UReflection.GetValue(UReflection.GetValue(currState,
                    "<Run>k__BackingField",
                    "<AutoSplitter>k__BackingField",
                    "<Component>k__BackingField",
                    "<Script>k__BackingField"));

                if (script != null)
                {
                    Instance = UReflection.GetValue(script, "_game");
                    Vars = UReflection.GetValue(script, "<Vars>k__BackingField");
                }

                if (!TProcess.IsAlive(Instance))
                {
                    dynamic value = UReflection.GetValue(UReflection.GetValue(currState,
                    "<Layout>k__BackingField",
                    "<LayoutComponents>k__BackingField",
                    "_items"));

                    if (value != null)
                    {
                        int elements = value.Length;
                        for (int i = 0; i < elements; i++)
                        {
                            dynamic index = value.GetValue(i);
                            if (index == null) break;
                            else
                            {
                                dynamic script2 = UReflection.GetValue(index,
                                    "<Component>k__BackingField",
                                    "<Script>k__BackingField");

                                if (script2 != null)
                                {
                                    Instance = UReflection.GetValue(script2, "_game");
                                    Vars = UReflection.GetValue(script2, "<Vars>k__BackingField");
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (TProcess.IsAlive(Instance))
            {
                MemoryManager.ClearMemory();
                return true;
            }
        }
        catch { }
        return false;
    }

    internal static bool SetProcessCache(string id, string name, string data)
    {
        try
        {
            string token = TProcess.GetToken(Instance);
            if (token == null) return false;

            TSaves2.Set(data, "ProcessCache", id, name);
            TSaves2.Set(token, "ProcessCache", id, name, "Token");
        }
        catch { }
        return false;
    }

    internal static string GetProcessCache(string id, string name)
    {
        try
        {
            string token = TProcess.GetToken(Instance);
            if (token == null) return null;

            string data = TSaves2.Get("ProcessCache", id, name);
            if (string.IsNullOrEmpty(data)) return null;

            string dataToken = TSaves2.Get("ProcessCache", id, name, "Token");
            if (string.IsNullOrEmpty(dataToken)) return null;

            if (token == dataToken) return data;
        }
        catch { }
        return null;
    }

    internal static MethodInfo _RefAllocateMemory;
    internal static ulong RefAllocateMemory(Process process, int size)
    {
        try
        {
            IntPtr toReturn = (IntPtr)_RefAllocateMemory.Invoke(null, new object[] { process, size });
            return (ulong)toReturn;
        }
        catch { }
        return 0;
    }

    internal static MethodInfo _RefReadBytes;
    internal static byte[] RefReadBytes(Process process, ulong address, int count)
    {
        try
        {
            return (byte[])_RefReadBytes.Invoke(null, new object[] { process, (IntPtr)address, count });
        }
        catch { }
        return null;
    }

    internal static MethodInfo _RefWriteBytes;
    internal static void RefWriteBytes(Process process, ulong address, byte[] bytes)
    {
        try
        {
            _RefWriteBytes.Invoke(null, new object[] { process, (IntPtr)address, bytes });
        }
        catch { }
    }

    internal static MethodInfo _RefCreateThread;
    internal static void RefCreateThread(Process process, ulong address)
    {
        try
        {
            _RefCreateThread.Invoke(null, new object[] { process, (IntPtr)address });
        }
        catch { }
    }
}