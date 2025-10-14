using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class ToolsShared
{
    internal class ToolData
    {
        internal class UnrealEngine
        {
            public static ulong F_StaticConstructObject_Internal = 0;
            public static ulong F_UObjectBeginDestroy = 0;
            public static ulong F_UObjectProcessEvent = 0;
            public static ulong D_FNamePoolAddress = 0;
        }
    }

    internal class ToolNames
    {
        internal class Unity
        {
            internal static readonly string[] Data = new string[] { "unity", "unityengine", "unity3d" };

            internal class Utils
            {
                internal static readonly string[] Data = new string[] { "utils" };
            }

            internal class DotNet
            {
                internal static readonly string[] Data = new string[] { "dotnet", "cs", "csharp", "mono" };

                internal class JitSave
                {
                    internal static readonly string[] Data = new string[] { "jitsave" };
                }

                internal class Instance
                {
                    internal static readonly string[] Data = new string[] { "instance" };
                }
            }

            internal class Il2Cpp
            {
                internal static readonly string[] Data = new string[] { "il2cpp", "cpp" };

                internal class JitSave
                {
                    internal static readonly string[] Data = new string[] { "jitsave" };
                }

                internal class Instance
                {
                    internal static readonly string[] Data = new string[] { "instance" };
                }
            }
        }

        internal class UnrealEngine
        {
            internal static readonly string[] Data = new string[] { "unrealengine" };

            internal class Default
            {
                internal static readonly string[] Data = new string[] { "default" };

                internal class Events
                {
                    internal static readonly string[] Data = new string[] { "events" };
                }

                internal class CutsceneManager
                {
                    internal static readonly string[] Data = new string[] { "cutscenemanager" };
                }
            }
        }
    }
}
