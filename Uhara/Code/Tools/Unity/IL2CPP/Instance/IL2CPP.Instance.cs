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

public partial class Tools : MainShared
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

                public InstanceWatcherBuild Get(string fullName, params string[] fieldsNames)
                {
                    try
                    {
                        return instanceCreation.AddArgument(ArgTypes.Instance, 1, fullName, fieldsNames);
                    }
                    catch { }
                    return null;
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
                        while (ProcessInstance.MainWindowHandle == IntPtr.Zero)
                        {
                            if (!ReloadProcess()) throw new Exception();
                            Thread.Sleep(100);
                        }

                        bool success = false;
                        while (!success)
                        {
                            do
                            {
                                if (!ReloadProcess()) throw new Exception();
                                if (TProcess.GetModuleBase(ProcessInstance, "GameAssembly.dll") == 0) break;
                                if (TProcess.GetModuleBase(ProcessInstance, "UnityPlayer.dll") == 0) break;
                                if (TProcess.GetModuleBase(ProcessInstance, "kernel32.dll") == 0) break;
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