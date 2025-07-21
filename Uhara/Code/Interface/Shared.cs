using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public class UShared
{
    internal static string ProcessName = null;
    internal static Process Instance = null;
    internal static dynamic Vars;

    public static bool DebugMode = true;

    internal class ToolNames
    {
        internal static readonly string[] _Unity = new string[] { "unity", "unity3d" };

        internal class Unity
        {
            internal static readonly string[] _DotNet = new string[] { "cs", "dotnet", "csharp", "mono" };
            internal static readonly string[] _Il2Cpp = new string[] { "cpp", "il2cpp" };

            internal class Tool
            {
                internal static readonly string[] _JitSave = new string[] { "jitsave" };
            }
        }
    }

    internal class MemoryCleaner
    {
        private static readonly string RegistryName = "MemoryCleaner";
        private static readonly string FreeMemory = "FreeMemory";
        private static readonly string Overwrite = "Overwrite";

        public static void AddAllocate(ulong address, int size)
        {
            try
            {
                if (address == 0 || size == 0) return;

                string token = UProcess.GetToken(Instance);
                string data = "0x" + size.ToString("X");

                USaves.Set(data, RegistryName, token, FreeMemory, "0x" + address.ToString("X"));
            }
            catch { }
        }

        public static void AddOverwrite(ulong address, byte[] validate, byte[] recover)
        {
            try
            {
                if (address == 0 || validate == null || recover == null)
                    return;

                string token = UProcess.GetToken(Instance);
                string data = UMemory.GetSignature(validate, true) + " " +
                    UMemory.GetSignature(recover, true);

                USaves.Set(data, RegistryName, token, Overwrite, "0x" + address.ToString("X"));
            }
            catch { }
        }

        public static void Start()
        {
            if (UProcess.IsAlive(Instance))
            {
                string token = UProcess.GetToken(Instance);
                string[] keys = USaves.GetKeyNames(RegistryName);

                foreach (string key in keys)
                {
                    // overwrite
                    {
                        string[] values = USaves.GetValueNames(RegistryName, key, Overwrite);
                        foreach (string value in values)
                        {
                            ulong address = UConvert.Parse<ulong>(value);
                            if (address == 0) continue;

                            string dataRaw = USaves.Get(RegistryName, key, Overwrite, value);
                            if (dataRaw == null) continue;

                            string[] dataSplit = dataRaw.Split(' ');
                            if (dataSplit.Length != 2) continue;

                            byte[] validate = UMemory.GetByteArray(dataSplit[0]);
                            byte[] recover = UMemory.GetByteArray(dataSplit[1]);

                            if (UMemory.ConfirmBytes(Instance, address, validate))
                            {
                                RefWriteBytes(Instance, address, recover);
                                UProgram.Print("Overwrite at 0x" + address.ToString("X") +
                                    " recovered with " + recover.Length + " bytes");
                            }
                        }
                    }

                    // free memory
                    {
                        string[] values = USaves.GetValueNames(RegistryName, key, FreeMemory);
                        foreach (string value in values)
                        {
                            ulong address = UConvert.Parse<ulong>(value);
                            if (address == 0) continue;

                            string dataRaw = USaves.Get(RegistryName, key, FreeMemory, value);
                            if (dataRaw == null) continue;

                            int size = UConvert.Parse<int>(dataRaw);
                            if (size == 0) continue;

                            try
                            {
                                UMemory.FreeMemory(Instance, address, size);
                                UProgram.Print("Memory freed at 0x" + address.ToString("X"));
                            } catch { }
                        }
                    }
                }

                USaves.DeleteKey(RegistryName);
            }
        }
    }

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