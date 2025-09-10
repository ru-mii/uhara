using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Tools.Unity.DotNet.Instance.InstanceCreation;

public partial class Tools : MainShared
{
    public partial class Unity
    {
        public partial class DotNet
        {
            public partial class Instance
            {
                #region PUBLIC_API
                public InstanceWatcherBuild Get(string fullName, params string[] fieldsNames)
                {
                    try
                    {
                        return instanceCreation.AddArgument(ArgTypes.Instance, 1, fullName, fieldsNames);
                    }
                    catch { }
                    return null;
                }

                public InstanceWatcherBuild[] Get(short instances, string fullName, params string[] fieldsNames)
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
                    return new InstanceWatcherBuild[0];
                }

                public InstanceWatcherBuild InstanceFlag(string fullName)
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

                internal static InstanceCreation instanceCreation = null;
                internal static GetInstances getInstances = null;
                internal static InstanceDestroy instanceDestroy = null;
                internal static OffsetResolver offsetResolver = null;

                public Instance()
                {
                    offsetResolver = new OffsetResolver();
                    instanceDestroy = new InstanceDestroy();
                    getInstances = new GetInstances();
                    instanceCreation = new InstanceCreation();
                }
            }
        }
    }
}