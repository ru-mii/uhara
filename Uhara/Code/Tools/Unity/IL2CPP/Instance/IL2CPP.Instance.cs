using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using static Tools.Unity.IL2CPP.Instance.InstanceCreation;

public partial class Tools
{
    public partial class Unity
    {
        public partial class IL2CPP
        {
            public partial class Instance
            {
                #region PUBLIC_API
                public void SetDefaultNames(string imageName, string namespaceName = null, string className = null)
                {
                    try
                    {
                        instanceCreation.DefaultImage = imageName;
                        if (namespaceName != null) instanceCreation.DefaultNamespace = namespaceName;
                        if (className != null) instanceCreation.DefaultClass = className;
                    }
                    catch { }
                }

                public void Watch<T>(string watcherName, string fullName, params string[] fieldsNames) where T : unmanaged
                {
                    try
                    {
                        InstanceWatcherBuild watcherBuild = instanceCreation.AddArgument(ArgTypes.Instance, 1, fullName, fieldsNames);
                        new PtrResolver().Watch<T>(watcherName, watcherBuild.Base, watcherBuild.Offsets);
                    }
                    catch { }
                }

                public InstanceWatcherBuild Get(string fullName, params string[] fieldsNames)
                {
                    try
                    {
                        return instanceCreation.AddArgument(ArgTypes.Instance, 1, fullName, fieldsNames);
                    }
                    catch { }
                    return null;
                }

                public void Watch<T>(string watcherName, short instances, string fullName, params string[] fieldsNames) where T : unmanaged
                {
                    try
                    {
                        do
                        {
                            PtrResolver ptrResolver = new PtrResolver();
                            InstanceWatcherBuildMultiple watcherBuildMultiple = instanceCreation.AddArgumentMultiple(ArgTypes.Instance, instances, fullName, fieldsNames);

                            for (int i = 0; i < watcherBuildMultiple.Base.Length; i++)
                                ptrResolver.Watch<T>(watcherName + i.ToString(), watcherBuildMultiple.Base[i], watcherBuildMultiple.Offsets);
                        }
                        while (false);
                    }
                    catch { }
                }

                public InstanceWatcherBuildMultiple Get(short instances, string fullName, params string[] fieldsNames)
                {
                    try
                    {
                        do
                        {
                            return instanceCreation.AddArgumentMultiple(ArgTypes.Instance, instances, fullName, fieldsNames);
                        }
                        while (false);
                    }
                    catch { }
                    return null;
                }

                public void WatchFlag(string watcherName, string fullName)
                {
                    try
                    {
                        InstanceWatcherBuild watcherBuild = instanceCreation.AddArgument(ArgTypes.Flag, 1, fullName);
                        new PtrResolver().Watch<ulong>(watcherName, watcherBuild.Base, watcherBuild.Offsets);
                    }
                    catch { }
                }

                public InstanceWatcherBuild Flag(string fullName)
                {
                    try
                    {
                        return instanceCreation.AddArgument(ArgTypes.Flag, 1, fullName);
                    }
                    catch { }
                    return null;
                }
                #endregion

                internal static string DebugClass = "Instance";
                internal static string ToolUniqueID = "RVOfYiobVYdbMkDJ";

                internal static InstanceCreation instanceCreation = null;
                internal static GetInstances getInstances = null;
                internal static InstanceDestroy instanceDestroy = null;
                internal static OffsetResolver offsetResolver = null;

                enum Result
                {
                    None = 0,
                    Success = 1,
                    Failed = 2
                }

                public Instance()
                {
                    try
                    {
                        while (Main.ProcessInstance.MainWindowHandle == IntPtr.Zero)
                        {
                            if (!Main.ReloadProcess()) throw new Exception();
                            Thread.Sleep(100);
                        }

                        bool success = false;
                        while (!success)
                        {
                            do
                            {
                                if (!Main.ReloadProcess()) throw new Exception();
                                if (TProcess.GetModuleBase(Main.ProcessInstance, "GameAssembly.dll") == 0) break;
                                if (TProcess.GetModuleBase(Main.ProcessInstance, "UnityPlayer.dll") == 0) break;
                                if (TProcess.GetModuleBase(Main.ProcessInstance, "kernel32.dll") == 0) break;
                                success = true;
                            }
                            while (false);
                            Thread.Sleep(100);
                        }
                    }
                    catch { return; }

                    MemoryManager.ClearMemory(ToolUniqueID);

                    instanceDestroy = new InstanceDestroy();
                    getInstances = new GetInstances();
                    offsetResolver = new OffsetResolver();
                    instanceCreation = new InstanceCreation();
                }
            }
        }
    }
}