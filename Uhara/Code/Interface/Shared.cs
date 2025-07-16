using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class UShared
{
    internal static string ProcessName = null;
    internal static Process Instance = null;
    internal static dynamic Vars;

    internal static string ToolName = "";
    internal static string ToolCategory = "";

    internal static bool CheckSetProcessAndValues()
    {
        try
        {
            if (UProcess.IsAlive(Instance)) return true;

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

                if (!UProcess.IsAlive(Instance))
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
        }
        catch { }
        return UProcess.IsAlive(Instance);
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