using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Reflection;

public partial class Tools
{
    public partial class UnrealEngine
    {
        public partial class Default
        {
            public partial class Events
            {
                internal static string DebugClass = "Instance";
                internal static string ToolUniqueID = "OlDIgZzLoZjiyHwu";

                private FunctionCall functionCall = new FunctionCall();
                private InstanceCreation instanceCreation = new InstanceCreation();

                #region PUBLIC_API
                public IntPtr GetLastFunctionCallInstanceDestroyFlagPointer()
                {
                    try
                    {
                        return functionCall.GetLastDestroyInstanceFlagPointer();
                    }
                    catch { }
                    return IntPtr.Zero;
                }

                public IntPtr GetLastInstanceCreationInstanceDestroyFlagPointer()
                {
                    try
                    {
                        return instanceCreation.GetLastDestroyInstanceFlagPointer();
                    }
                    catch { }
                    return IntPtr.Zero;
                }

                public IntPtr InstancePtr(string className, string objectName)
                {
                    try
                    {
                        return instanceCreation.AddArgument(InstanceCreation.ArgTypes.Instance, className, objectName, 1);
                    }
                    catch { }
                    return IntPtr.Zero;
                }

                public IntPtr[] InstancePtr(string className, string objectName, short instances)
                {
                    try
                    {
                        do
                        {
                            IntPtr basePtr = instanceCreation.AddArgument(InstanceCreation.ArgTypes.Instance, className, objectName, instances);
                            if (basePtr == IntPtr.Zero) break;

                            List<IntPtr> result = new List<IntPtr>();
                            for (int i = 0; i < instances; i++) result.Add(basePtr + (0x8 * i));
                            return result.ToArray();
                        }
                        while (false);
                    }
                    catch { }
                    return new IntPtr[0];
                }

                public IntPtr InstanceFlag(string className, string objectName)
                {
                    try
                    {
                        return instanceCreation.AddArgument(InstanceCreation.ArgTypes.Flag, className, objectName, 1);
                    }
                    catch { }
                    return IntPtr.Zero;
                }

                public IntPtr FunctionFlag(string className, string objectName, string functionName)
                {
                    try
                    {
                        return functionCall.AddArgument(FunctionCall.ArgTypes.Flag, className, objectName, functionName, 1);
                    }
                    catch { }
                    return IntPtr.Zero;
                }

                public void FunctionFlag(string watcherName, string className, string objectName, string functionName)
                {
                    try
                    {
                        new PtrResolver().Watch<ulong>(watcherName, functionCall.AddArgument(FunctionCall.ArgTypes.Flag, className, objectName, functionName, 1));
                    }
                    catch { }
                }

                public IntPtr FunctionParentPtr(string className, string objectName, string functionName)
                {
                    try
                    {
                        return functionCall.AddArgument(FunctionCall.ArgTypes.Instance, className, objectName, functionName, 1);
                    }
                    catch { }
                    return IntPtr.Zero;
                }

                public void FunctionParentPtr<T>(string watcherName, string className, string objectName, string functionName) where T : unmanaged
                {
                    try
                    {
                        new PtrResolver().Watch<T>(watcherName, functionCall.AddArgument(FunctionCall.ArgTypes.Instance, className, objectName, functionName, 1));
                    }
                    catch { }
                }
                #endregion

                public Events()
                {
                    if (!Main.ReloadProcess()) throw new Exception();
                    MemoryManager.ClearMemory(ToolUniqueID);
                }
            }
        }
    }
}