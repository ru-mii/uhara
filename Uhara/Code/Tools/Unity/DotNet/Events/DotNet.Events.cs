using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public partial class Tools : MainShared
{
    public partial class Unity
    {
        public partial class DotNet
        {
            public partial class Events
            {
                internal static string DebugClass = "Events";

                internal static InstanceCreation instanceCreation = null;
                internal static GetInstances getInstances = null;
                internal static InstanceDestroy instanceDestroy = null;

                public Events()
                {
                    instanceDestroy = new InstanceDestroy();
                    getInstances = new GetInstances();
                    instanceCreation = new InstanceCreation();
                }
            }
        }
    }
}