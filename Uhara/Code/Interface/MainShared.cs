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

    internal static LiveSplitState CurrentState = null;
    internal static dynamic script = null;
    internal static List<MemoryWatcher> MemoryWatchers = new List<MemoryWatcher>();
    internal static List<StringWatcher> StringWatchers = new List<StringWatcher>();

    public static bool DebugMode = true;
    private Dictionary<string, object> Indexer = new Dictionary<string, object>();
    #endregion
    #region PROPERTIES
    private static volatile Process _ProcessInstance = null;
    internal static  Process ProcessInstance
    {
        get
        {
            if (script == null) CheckSetProcessAndValues();
            if (_ProcessInstance == null || _ProcessInstance.HasExited)
            {
                FieldInfo gameField = script.GetType().GetField("_game", BindingFlags.NonPublic | BindingFlags.Instance);
                var gameInstance = gameField?.GetValue(script);
                _ProcessInstance = (Process)gameInstance;
            }
            return _ProcessInstance;
        }
        set
        {
            _ProcessInstance = value;
        }
    }

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

    public static void AddWatcher(MemoryWatcher watcher)
    {
        try
        {
            MemoryWatchers.Add(watcher);
        }
        catch { }
    }

    public static void AddWatcher<T>(DeepPointer deepPointer) where T : unmanaged
    {
        try
        {
            MemoryWatchers.Add(new MemoryWatcher<T>(deepPointer));
        }
        catch { }
    }

    public static void AddWatcher<T>(IntPtr baseAddress, params int[] offsets) where T : unmanaged
    {
        try
        {
            MemoryWatchers.Add(new MemoryWatcher<T>(new DeepPointer(baseAddress, offsets)));
        }
        catch { }
    }

    public object this[string key]
    {
        get
        {
            try
            {
                MemoryWatcher watcher = MemoryWatchers.FirstOrDefault(m => m.Name == key);
                if (watcher == null) watcher = StringWatchers.FirstOrDefault(m => m.Name == key);
                return watcher;
            }
            catch { }
            return null;
        }
    }

    internal static bool ReloadProcess()
    {
        do
        {
            if (ProcessInstance == null) break;

            string lastName = ProcessInstance.ProcessName;
            string lastToken = TProcess.GetToken(ProcessInstance);
            if (string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(lastToken)) break;

            ProcessInstance = Process.GetProcessById(ProcessInstance.Id);
            if (ProcessInstance == null || ProcessInstance.HasExited) break;

            string currentName = ProcessInstance.ProcessName;
            string currentToken = TProcess.GetToken(ProcessInstance);
            if (string.IsNullOrEmpty(currentName) || string.IsNullOrEmpty(currentToken)) break;

            if (currentName != lastName) break;
            if (currentToken != lastToken) break;

            return true;
        }
        while (false);
        return false;
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
            CurrentState = timerForm.CurrentState;
            
            if (CurrentState?.Run?.AutoSplitter != null && CurrentState.Run.AutoSplitter.IsActivated)
            {
                dynamic dynComponent = CurrentState.Run.AutoSplitter.Component;
                script = dynComponent.Script;
            }

            if (script == null)
            {
                foreach (var smth in CurrentState.Layout.LayoutComponents)
                {
                    dynamic component = smth.Component;
                    if (component.GetType().Name.Contains("ASLComponent"))
                    {
                        script = component.Script;
                        if (script != null) break;
                    }
                }
            }

            if (script != null)
            {
                FieldInfo gameField = script.GetType().GetField("_game", BindingFlags.NonPublic | BindingFlags.Instance);
                var gameInstance = gameField?.GetValue(script);

                Vars = script.Vars;
                ProcessInstance = (Process)gameInstance;
                if (ProcessInstance != null) MemoryManager.ClearMemory();
            }
        }
        catch { }
    }

    internal static void SetProcessCache(string id, string name, string data)
    {
        string token = TProcess.GetToken(ProcessInstance);
        if (token == null) return;

        TSaves2.Set(data, "ProcessCache", id, name);
        TSaves2.Set(token, "ProcessCache", id, name, "Token");
    }

    internal static string GetProcessCache(string id, string name)
    {
        string token = TProcess.GetToken(ProcessInstance);
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