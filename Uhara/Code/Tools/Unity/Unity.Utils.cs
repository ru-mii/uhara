using SharpDisasm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public partial class Tools : MainShared
{
    public partial class Unity
    {
        public class Utils
        {
            internal static string ToolUniqueID = "LMYpsRecShieLHhD";

            #region SCENE_MANAGER
            bool _SceneManager = false;
            ulong SceneManager_ManagerPtr = 0;
            ulong SceneManager_NameOffset = 0;
            private void SceneManager()
            {
                try
                {
                    ulong address = 0;

                    if (address == 0)
                    {
                        try
                        {
                            do
                            {
                                ulong result = TMemory.ScanSingle(ProcessInstance,
                                    "48 C7 43 ?? 00 00 80 3F 48 8B 5C 24 30 48 83 C4 20 5F C3", "UnityPlayer.dll", 0x20);

                                if (result == 0) break;
                                result = TMemory.GetFunctionStart(ProcessInstance, result);

                                // ---
                                {
                                    byte[] checkBytes1 = TMemory.ReadMemoryBytes(ProcessInstance, result, 13);
                                    byte[] checkBytes2 = new byte[] { 0x48, 0x89, 0x5C, 0x24, 0x08, 0x57, 0x48, 0x83, 0xEC, 0x20, 0x48, 0x8B, 0xD9 };
                                    if (!checkBytes1.SequenceEqual(checkBytes2)) break;
                                }

                                // ---
                                {
                                    result += 13;
                                    Instruction ins = TInstruction.GetInstruction2(ProcessInstance, result);

                                    if (ins.ToString().Contains(", [") && ins.Bytes.Length == 7)
                                    {
                                        int value = TMemory.ReadMemory<int>(ProcessInstance, result + 3);
                                        SceneManager_ManagerPtr = (ulong)((long)result + value + 7);
                                        SceneManager_NameOffset = 0x38;

                                        // ---
                                        _SceneManager = true;
                                        return;
                                    }

                                    else if (ins.ToString().StartsWith("call") && ins.Length == 5)
                                    {
                                        int value = TMemory.ReadMemory<int>(ProcessInstance, result + 1);
                                        result = (ulong)((long)result + value + 5);

                                        ins = TInstruction.GetInstruction2(ProcessInstance, result);
                                        if (!ins.ToString().Contains(", [") || ins.Bytes.Length != 7) break;

                                        value = TMemory.ReadMemory<int>(ProcessInstance, result + 3);
                                        SceneManager_ManagerPtr = (ulong)((long)result + value + 7);
                                        SceneManager_NameOffset = 0x40;

                                        // ---
                                        _SceneManager = true;
                                        return;
                                    }
                                }
                            }
                            while (false);
                        }
                        catch { }
                    }

                    if (address == 0)
                    {
                        try
                        {
                            do
                            {
                                ulong result = TMemory.ScanSingle(ProcessInstance, "48 8B 05 ?? ?? ?? ?? 48 8B D1 48 83 78 48 00 74 0A 48 8B 40 48", "UnityPlayer.dll", 0x20);
                                if (result == 0) break;

                                // ---
                                int value = TMemory.ReadMemory<int>(ProcessInstance, result + 3);
                                SceneManager_ManagerPtr = (ulong)((long)result + value + 7);
                                SceneManager_NameOffset = 0x38;

                                // ---
                                _SceneManager = true;
                                return;
                            }
                            while (false);
                        }
                        catch { }
                    }

                    _SceneManager = false;
                }
                catch { }
            }

            public string GetActiveSceneName()
            {
                string result = null;
                try
                {
                    do
                    {
                        if (!_SceneManager) break;

                        // ---
                        ulong address = TMemory.ReadMemory<ulong>(ProcessInstance, SceneManager_ManagerPtr);
                        if (address == 0) break;

                        address = TMemory.ReadMemory<ulong>(ProcessInstance, address + 0x48);
                        if (address == 0) break;

                        result = TMemory.ReadMemoryString(ProcessInstance, address + SceneManager_NameOffset, 64);
                    }
                    while (false);
                }
                catch { }
                return result;
            }
            #endregion

            public Utils()
            {
                try
                {
                    do
                    {
                        ProcessInstance = TProcess.RefreshProcess(ProcessInstance);
                        Thread.Sleep(100);
                    }
                    while (ProcessInstance.MainWindowHandle == IntPtr.Zero);

                    bool success = false;
                    while (!success)
                    {
                        do
                        {
                            ProcessInstance = TProcess.RefreshProcess(ProcessInstance);
                            if (TProcess.GetModuleBase(ProcessInstance, "UnityPlayer.dll") == 0) break;
                            success = true;
                        }
                        while (false);
                        Thread.Sleep(100);
                    }

                    MemoryManager.ClearMemory(ToolUniqueID);

                    // ---
                    SceneManager();

                    // ---
                    if (_SceneManager) TUtils.Print("Unity.Utils | SceneManager loaded seccessfuly");
                    else TUtils.Print("Unity.Utils | SceneManager loading failed");
                }
                catch { }
            }
        }
    }
}
