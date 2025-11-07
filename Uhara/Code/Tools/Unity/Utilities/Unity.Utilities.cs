using SharpDisasm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public partial class Tools : MainShared
{
    public partial class Unity
    {
        public partial class Utilities
        {
            private static string ToolUniqueID = "LMYpsRecShieLHhD";

            SceneManager sceneManager;

            #region CONSTRUCTOR
            public Utilities()
            {
                try
                {
                    do
                    {
                        if (!ReloadProcess()) throw new Exception();
                        Thread.Sleep(100);
                    }
                    while (ProcessInstance.MainWindowHandle == IntPtr.Zero);

                    bool success = false;
                    while (!success)
                    {
                        do
                        {
                            if (!ReloadProcess()) throw new Exception();
                            if (TProcess.GetModuleBase(ProcessInstance, "mono-2.0-bdwgc.dll") != 0)
                            {
                                if (TProcess.GetModuleBase(ProcessInstance, "UnityPlayer.dll") == 0) break;
                            }
                            else if (TProcess.GetModuleBase(ProcessInstance, "mono.dll") == 0) break;
                            success = true;
                        }
                        while (false);
                        Thread.Sleep(300);
                    }

                    if (!ReloadProcess()) throw new Exception();
                    MemoryManager.ClearMemory(ToolUniqueID);

                    // ---
                    sceneManager = new SceneManager();
                }
                catch { }
            }
            #endregion
        }
    }
}