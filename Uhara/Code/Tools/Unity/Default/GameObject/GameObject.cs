using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public partial class Tools
{
    public partial class Unity
    {
        public partial class Default
        {
            public partial class GameObject
            {
                #region PUBLIC_API
                public Converter.GameObjectResolvable InstanceToGameObjectResolvable(IntPtr instanceAddress, bool isInsideAPointer)
                {
                    return converter.InstanceToGameObjectResolvable((ulong)instanceAddress, isInsideAPointer);
                }

                public Converter.GameObjectResolvable InstanceToGameObjectResolvable(ulong instanceAddress, bool isInsideAPointer)
                {
                    return converter.InstanceToGameObjectResolvable(instanceAddress, isInsideAPointer);
                }

                public Converter.GameObjectResolvable GameObjectToGameObjectResolvable(IntPtr instanceAddress, bool isInsideAPointer)
                {
                    return converter.GameObjectToGameObjectResolvable((ulong)instanceAddress, isInsideAPointer);
                }

                public Converter.GameObjectResolvable GameObjectToGameObjectResolvable(ulong instanceAddress, bool isInsideAPointer)
                {
                    return converter.GameObjectToGameObjectResolvable(instanceAddress, isInsideAPointer);
                }
                #endregion

                #region VARIABLES
                internal static string ToolUniqueID = "BhuPdeQvHKvIDKiP";
                internal static Converter converter;
                #endregion

                #region CONSTRUCTOR
                public GameObject()
                {
                    try
                    {
                        try
                        {
                            while (true)
                            {
                                if (!Main.ReloadProcess()) throw new Exception();

                                if (Main.ProcessInstance.MainWindowHandle != IntPtr.Zero)
                                    break;

                                throw new Exception();
                            }

                            bool success = false;
                            while (!success)
                            {
                                do
                                {
                                    if (!Main.ReloadProcess()) throw new Exception();
                                    try
                                    {
                                        if (Main.ProcessInstance == null) break;

                                        if (TProcess.GetModuleBase(Main.ProcessInstance, "mono-2.0-bdwgc.dll") != 0)
                                        {
                                            if (TProcess.GetModuleBase(Main.ProcessInstance, "UnityPlayer.dll") == 0) break;
                                            byte[] modBytes = TProcess.GetModuleBytes(Main.ProcessInstance, "UnityPlayer.dll");
                                            if (modBytes == null || modBytes.Length == 0) break;
                                        }
                                        else break;

                                        if (TProcess.GetModuleBase(Main.ProcessInstance, "kernel32.dll") == 0) break;
                                    }
                                    catch { }
                                    success = true;
                                }
                                while (false);
                                if (!success) throw new Exception();
                            }
                        }
                        catch { return; }

                        MemoryManager.ClearMemory(ToolUniqueID);
                    }
                    catch { return; }

                    // ---
                    converter = new Converter();
                    if (!converter.Initiate()) throw new Exception();
                }
                #endregion
            }
        }
    }
}