using LiveSplit.ASL;
using LiveSplit.ComponentUtil;
using LiveSplit.Model;
using LiveSplit.UI.Components;
using LiveSplit.View;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public class MainShared
{
    #region FIELDS
    internal static string ProcessName = null;
    internal static dynamic Vars;

    internal static string UniqueScriptLoadID = null;
    internal static ulong UpdateCounter = 0;

    internal static dynamic script = null;
    internal List<MemoryWatcher> MemoryWatchers = new List<MemoryWatcher>();

    public static bool DebugMode = true;
    #endregion
    #region PROPERTIES
    internal static volatile Process Instance = null;

    private static IDictionary<string, object> _current;
    internal static IDictionary<string, object> current
    {
        get
        {
            if (_current == null) _current = script.State.Data;
            return _current;
        }
        set
        {

        }
    }
    #endregion

    internal void AddWatcher(MemoryWatcher watcher)
    {
        try
        {
            MemoryWatchers.Add(watcher);
        }
        catch { }
    }

    internal static void CheckSetProcessAndValues()
    {
        try
        {
            TimerForm timerForm = null;
            foreach (Form form in Application.OpenForms)
            {
                if (form is TimerForm tf)
                {
                    timerForm = tf;
                    break;
                }
            }
            if (timerForm == null) return;

            script = null;

            LiveSplitState currentState = timerForm.CurrentState;
            foreach (var smth in currentState.Layout.LayoutComponents)
            {
                dynamic component = smth.Component;
                if (component.GetType().Name.Contains("ASLComponent"))
                {
                    script = component.Script;
                    if (script != null) break;
                }
            }

            if (script == null)
            {
                if (currentState?.Run?.AutoSplitter != null && currentState.Run.AutoSplitter.IsActivated)
                {
                    dynamic dynComponent = currentState.Run.AutoSplitter.Component;
                    script = dynComponent.Script;
                }
            }

            if (script != null)
            {
                FieldInfo gameField = script.GetType().GetField("_game", BindingFlags.NonPublic | BindingFlags.Instance);
                var gameInstance = gameField?.GetValue(script);

                Vars = script.Vars;
                Instance = (Process)gameInstance;
                if (Instance != null) MemoryManager.ClearMemory();
            }
        }
        catch { }
        MemoryManager.ClearMemory();
    }

    internal static void SetProcessCache(string id, string name, string data)
    {
        string token = TProcess.GetToken(Instance);
        if (token == null) return;

        TSaves2.Set(data, "ProcessCache", id, name);
        TSaves2.Set(token, "ProcessCache", id, name, "Token");
    }

    internal static string GetProcessCache(string id, string name)
    {
        string token = TProcess.GetToken(Instance);
        if (token == null) return null;

        string data = TSaves2.Get("ProcessCache", id, name);
        if (string.IsNullOrEmpty(data)) return null;

        string dataToken = TSaves2.Get("ProcessCache", id, name, "Token");
        if (string.IsNullOrEmpty(dataToken)) return null;

        if (token == dataToken) return data;
        return null;
    }

    internal static MethodInfo _RefAllocateMemory;
    internal static ulong RefAllocateMemory(Process process, int size)
    {
        return (ulong)(IntPtr)_RefAllocateMemory.Invoke(null, new object[] { process, size });
    }

    internal static MethodInfo _RefReadBytes;
    internal static byte[] RefReadBytes(Process process, ulong address, int count)
    {
        return (byte[])_RefReadBytes.Invoke(null, new object[] { process, (IntPtr)address, count });
    }

    internal static MethodInfo _RefWriteBytes;
    internal static void RefWriteBytes(Process process, ulong address, byte[] bytes)
    {
        _RefWriteBytes.Invoke(null, new object[] { process, (IntPtr)address, bytes });
    }

    internal static MethodInfo _RefCreateThread;
    internal static void RefCreateThread(Process process, ulong address)
    {
        _RefCreateThread.Invoke(null, new object[] { process, (IntPtr)address });
    }
}