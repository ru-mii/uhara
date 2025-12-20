using SharpDisasm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public partial class Tools
{
    public partial class Unity
    {
        public partial class Utilities
        {
            private static string ToolUniqueID = "LMYpsRecShieLHhD";

            SceneManager sceneManager = null;

            internal static bool LegacyVersion = false;

            #region CONSTRUCTOR
            public Utilities()
            {
                try
                {
                    do
                    {
                        if (!Main.ReloadProcess()) throw new Exception();
                        Thread.Sleep(100);
                    }
                    while (Main.ProcessInstance.MainWindowHandle == IntPtr.Zero);

                    bool success = false;
                    while (!success)
                    {
                        do
                        {
                            if (!Main.ReloadProcess()) throw new Exception();
                            if (TProcess.GetModuleBase(Main.ProcessInstance, "mono-2.0-bdwgc.dll") != 0)
                            {
                                if (TProcess.GetModuleBase(Main.ProcessInstance, "UnityPlayer.dll") == 0) break;
                            }
                            else if (TProcess.GetModuleBase(Main.ProcessInstance, "mono.dll") == 0) break;
                            else LegacyVersion = true;

                            success = true;
                        }
                        while (false);
                        Thread.Sleep(300);
                    }

                    if (!Main.ReloadProcess()) throw new Exception();
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