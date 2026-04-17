using LiveSplit.ComponentUtil;
using LiveSplit.Model;
using LiveSplit.View;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using SharpDisasm;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class Main
{
    public static string LIB_VERSION = "10";

    public ScriptSettings Settings = new ScriptSettings();
    public FileLogger FileLogger = new FileLogger();

    // ---
    internal static LiveSplitState CurrentState;

    internal static ulong UpdateCounter = 0;
    internal static string UniqueScriptLoadID;

    internal static List<MemoryWatcher> MemoryWatchers = new List<MemoryWatcher>();
    internal static List<(Type type, MemoryWatcher memoryWatcher, DeepPointer deepPointer)> ListWatchers = new List<(Type, MemoryWatcher, DeepPointer)>();
    internal static List<StringWatcher> StringWatchers = new List<StringWatcher>();

    internal static dynamic Vars;
    public static bool DebugMode = true;

    internal static ulong LastStartTime;

    // ---
    internal static dynamic _settings;

    internal static dynamic bf_script;
    internal static dynamic _script
    {
        get
        {
            if (bf_script == null) CheckSetProcessAndValues();
            return bf_script;
        }
        set
        {
            bf_script = value;
        }
    }

    private static volatile Process bf_ProcessInstance = null;
    internal static Process ProcessInstance
    {
        get
        {
            do
            {
                if (bf_ProcessInstance != null && !bf_ProcessInstance.HasExited) break;
                bf_ProcessInstance = null;

                FieldInfo gameField = _script.GetType().GetField("_game", BindingFlags.NonPublic | BindingFlags.Instance);
                Process gameInstance = (Process)(gameField?.GetValue(_script));
                if (gameInstance == null) break;

                Process tempProcess = null;
                try { tempProcess = Process.GetProcessById(gameInstance.Id); } catch { }

                if (TProcess.IsSameProcess(tempProcess, gameInstance)) bf_ProcessInstance = tempProcess;
            }
            while (false);
            return bf_ProcessInstance;
        }
        private set
        {
            Process tempProcess = value;
            Process newProcess = null;

            try { newProcess = Process.GetProcessById(tempProcess.Id); } catch { }
            bf_ProcessInstance = newProcess;
        }
    }

    private static IDictionary<string, object> _current;
    internal static IDictionary<string, object> current
    {
        get
        {
            if (_current == null) _current = _script.State?.Data;
            return _current;
        }
    }

    private static IDictionary<string, object> _old;
    internal static IDictionary<string, object> old
    {
        get
        {
            if (_old == null) _old = _script.OldState?.Data;
            return _old;
        }
    }

    public Main()
    {
        try
        {
            Thread.Sleep(50);
            TSaves2.Register("rumii", "uhara" + LIB_VERSION);
            UniqueScriptLoadID = TUtils.GenerateRandomString(32);
            TSaves2.Set(UniqueScriptLoadID, "IDs", "UniqueScriptLoadID");

            CheckSetProcessAndValues();

            // ---
            Vars.Uhara = this;
            Vars.Resolver = new PtrResolver();

            // ---
            Assembly liveSplitAssembly = Assembly.Load("LiveSplit.Core");
            Type extensionMethodsType = liveSplitAssembly.GetType("LiveSplit.ComponentUtil.ExtensionMethods");

            _RefAllocateMemory = extensionMethodsType.GetMethod("AllocateMemory", new Type[] { typeof(Process), typeof(int) });
            _RefReadBytes = extensionMethodsType.GetMethod("ReadBytes", new Type[] { typeof(Process), typeof(IntPtr), typeof(int) });
            _RefWriteBytes = extensionMethodsType.GetMethod("WriteBytes", new Type[] { typeof(Process), typeof(IntPtr), typeof(byte[]) });
            _RefCreateThread = extensionMethodsType.GetMethod("CreateThread", new Type[] { typeof(Process), typeof(IntPtr) });

            // ---
            if (!File.Exists("SharpDisasm.dll"))
                File.WriteAllBytes("SharpDisasm.dll", AsmBlocks.SharpDisasm);
        }
        catch { }
    }

    public class Vector3
    {
        public float x, y, z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public Instruction[] Disassemble(byte[] bytes)
    {
        try
        {
            return Disassemble(bytes, IntPtr.Zero);
        }
        catch { }
        return null;
    }

    public Instruction[] Disassemble(byte[] bytes, IntPtr address)
    {
        try
        {
            return TInstruction.GetInstructions2(bytes, (ulong)address);
        }
        catch { }
        return null;
    }

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

    internal static bool ReloadProcess()
    {
        do
        {
            if (ProcessInstance == null || ProcessInstance.HasExited) break;

            string lastName = ProcessInstance.ProcessName;
            string lastToken = TProcess.GetToken(ProcessInstance);
            if (string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(lastToken)) break;

            try { ProcessInstance = Process.GetProcessById(ProcessInstance.Id); } catch { };
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

            bf_script = null;
            CurrentState = timerForm.CurrentState;

            if (CurrentState?.Run?.AutoSplitter != null && CurrentState.Run.AutoSplitter.IsActivated)
            {
                dynamic dynComponent = CurrentState.Run.AutoSplitter.Component;
                bf_script = dynComponent.Script;
            }

            if (bf_script == null)
            {
                foreach (var smth in CurrentState.Layout.LayoutComponents)
                {
                    dynamic component = smth.Component;
                    if (component.GetType().Name.Contains("ASLComponent"))
                    {
                        bf_script = component.Script;
                        if (bf_script != null) break;
                    }
                }
            }

            if (bf_script != null)
            {
                Vars = bf_script.Vars;

                FieldInfo settingsField = bf_script.GetType().GetField("_settings", BindingFlags.NonPublic | BindingFlags.Instance);
                var settingsRaw = settingsField?.GetValue(bf_script);
                _settings = settingsRaw;
            }
        }
        catch { }
    }

    internal static void SetProcessCache(string id, string name, string data)
    {
        string token = TProcess.GetToken(ProcessInstance);
        if (string.IsNullOrEmpty(token)) return;

        TSaves2.Set(data, "ProcessCache", id, name);
        TSaves2.Set(token, "ProcessCache", id, name, "Token");
    }

    internal static string GetProcessCache(string id, string name)
    {
        string token = TProcess.GetToken(ProcessInstance);
        if (string.IsNullOrEmpty(token)) return null;

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

    public void ForceCleanMemory()
    {
        try
        {
            ReloadProcess();
            MemoryManager.ClearMemory();
        }
        catch { }
    }

    public bool IsModuleLoaded(string moduleName)
    {
        try
        {
            ReloadProcess();
            ProcessModule processModule = TProcess.GetModule(ProcessInstance, moduleName);
            if (processModule == null) return false;

            ulong modBase = TProcess.GetModuleBase(ProcessInstance, moduleName);
            return modBase != 0;
        }
        catch { }
        return false;
    }

    public bool Is64Bit()
    {
        try
        {
            return TProcess.Is64Bit(ProcessInstance);
        }
        catch { }
        return false;
    }

    public void AcceptOnFound(string signature)
    {
        try
        {
            do
            {
                string processName = ProcessInstance.ProcessName;
                if (string.IsNullOrEmpty(processName)) break;

                Process[] processes = Process.GetProcessesByName(processName);
                if (processes == null || processes.Length == 0) break;

                foreach (Process process in processes)
                {
                    try
                    {
                        ulong result = TMemory.ScanSingle(ProcessInstance, signature);
                        if (result != 0)
                        {
                            _script.GetType().GetField("_game", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(ProcessInstance, process);
                            return;
                        }
                    }
                    catch { }
                }
            }
            while (false);
        }
        catch { }
        _script.GetType().GetField("_game", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_script, null);
    }

    public bool Reject(bool condition = true)
    {
        try
        {
            if (condition)
            {
                _script.GetType().GetField("_game", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_script, null);
            }
        }
        catch { }
        return true;
    }

    public bool Reject(params int[] moduleMemorySizes)
    {
        try
        {
            return Reject(ProcessInstance.MainModule, moduleMemorySizes);
        }
        catch { }
        return false;
    }

    public bool Reject(string module, params int[] moduleMemorySizes)
    {
        try
        {
            return Reject(TProcess.GetModule(ProcessInstance, module), moduleMemorySizes);
        }
        catch { }
        return false;
    }

    public bool Reject(ProcessModule module, params int[] moduleMemorySizes)
    {
        try
        {
            if (ProcessInstance == null)
            {
                TUtils.Print("Process not loaded yet");
                return false;
            }

            if (module is null)
            {
                TUtils.Print("Module could not be found");
                return false;
            }

            if (moduleMemorySizes is null || moduleMemorySizes.Length == 0)
            {
                _script.GetType().GetField("_game", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_script, null);
                return true;
            }

            int exeModuleSize = TProcess.GetImageSize(ProcessInstance, module);
            if (moduleMemorySizes.Any(mms => mms == exeModuleSize))
            {
                _script.GetType().GetField("_game", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(_script, null);
                return true;
            }
        }
        catch { }
        return false;
    }

    public int GetImageSize(ProcessModule module)
    {
        try
        {
            return TProcess.GetImageSize(ProcessInstance, module);
        }
        catch { }
        return 0;
    }

    public int GetImageSize(string moduleName = null)
    {
        try
        {
            return TProcess.GetImageSize(ProcessInstance, moduleName);
        }
        catch { }
        return 0;
    }

    public string GetMD5Hash(string modulePath = null)
    {
        try
        {
            if (string.IsNullOrEmpty(modulePath) || !File.Exists(modulePath)) modulePath = ProcessInstance.MainModule.FileName;
            return GetHash(modulePath);
        }
        catch { }
        return null;
    }

    public string GetHash(string filePath)
    {
        try
        {
            if (!File.Exists(filePath)) return null;

            byte[] bytes = new byte[0];
            using (var md5 = MD5.Create())
            {
                using (var file = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    bytes = md5.ComputeHash(file);
            }
            return bytes.Select(x => x.ToString("X2")).Aggregate((a, b) => a + b);
        }
        catch { }
        return null;
    }

    public string GetHashRelative(string filePath)
    {
        try
        {
            do
            {
                string mainModulePath = ProcessInstance.MainModule.FileName;
                if (string.IsNullOrEmpty(mainModulePath)) break;

                string mainModuleDir = Path.GetDirectoryName(mainModulePath);
                filePath = Path.Combine(mainModuleDir, filePath);

                if (!File.Exists(filePath)) return null;

                byte[] bytes = new byte[0];
                using (var md5 = MD5.Create())
                {
                    using (var file = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        bytes = md5.ComputeHash(file);
                }
                return bytes.Select(x => x.ToString("X2")).Aggregate((a, b) => a + b);
            }
            while (false);
        }
        catch { }
        return null;
    }

    public void Log(string message)
    {
        try
        {
            if (!string.IsNullOrEmpty(message)) TImports.OutputDebugString("[UHARA] " + message);
            else TImports.OutputDebugString("[UHARA] " + "Trying to print null");
        }
        catch { }
    }

    public void AlertLoadless()
    {
        try
        {
            if (CurrentState.CurrentTimingMethod != TimingMethod.GameTime)
            {
                if (MessageBox.Show("This autosplitter is using load removal and recommends using GameTime comparison, would you like to switch to it?", "LiveSplit",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    CurrentState.CurrentTimingMethod = TimingMethod.GameTime;
                }
            }
        }
        catch { }
    }

    public void AlertGameTime()
    {
        try
        {
            if (CurrentState.CurrentTimingMethod != TimingMethod.GameTime)
            {
                if (MessageBox.Show("This autosplitter recommends using GameTime comparison, would you like to switch to it?", "LiveSplit",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    CurrentState.CurrentTimingMethod = TimingMethod.GameTime;
                }
            }
        }
        catch { }
    }

    public void AlertRealTime()
    {
        try
        {
            if (CurrentState.CurrentTimingMethod != TimingMethod.RealTime)
            {
                if (MessageBox.Show("This autosplitter recommends using RealTime comparison, would you like to switch to it?", "LiveSplit",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    CurrentState.CurrentTimingMethod = TimingMethod.RealTime;
                }
            }
        }
        catch { }
    }

    public void DisableDebug()
    {
        try
        {
            DebugMode = false;
        }
        catch { }
    }

    public void EnableDebug()
    {
        try
        {
            DebugMode = true;
        }
        catch { }
    }

    private void TimerStartedAction()
    {

    }

    public void Update()
    {
        UpdateCounter++;

        do
        {
            ulong newAttemptStartTime = 0;
            DateTime? dt = CurrentState?.AttemptStarted.Time;
            if (dt.Value.Year >= 1601) newAttemptStartTime = dt != null && dt.HasValue ? (ulong)dt.Value.ToFileTime() : 0;
            if (newAttemptStartTime == 0) break;

            bool flag = newAttemptStartTime != LastStartTime;
            LastStartTime = newAttemptStartTime;

            if (flag) TimerStartedAction();
        }
        while (false);

        try
        {
            do
            {
                if (ProcessInstance != null && !ProcessInstance.HasExited && ProcessInstance.Handle != IntPtr.Zero)
                {
                    foreach (var watcher in MemoryWatchers)
                    {
                        old[watcher.Name] = watcher.Current;
                        watcher.Update(ProcessInstance);
                        current[watcher.Name] = watcher.Current;
                    }

                    foreach (var watcher in StringWatchers)
                    {
                        old[watcher.Name] = watcher.Current;
                        watcher.Update(ProcessInstance);
                        current[watcher.Name] = watcher.Current;
                    }

                    foreach (var watcher in ListWatchers)
                    {
                        MemoryWatcher memWatcher = watcher.memoryWatcher;

                        memWatcher.Update(ProcessInstance);
                        if (memWatcher.Current == null) continue;
                        if ((IntPtr)memWatcher.Current == IntPtr.Zero) continue;

                        ulong listAddr = (ulong)(IntPtr)(memWatcher.Current);
                        if (listAddr == 0) continue;

                        Type type = watcher.type;

                        int itemSize = Marshal.SizeOf(type);
                        int count = TMemory.ReadMemory<ushort>(ProcessInstance, listAddr + 0x18);
                        int size = count * itemSize;

                        ulong listItemsAddr = TMemory.ReadMemory<ulong>(ProcessInstance, listAddr + 0x10);
                        byte[] listBytes = TMemory.ReadMemoryBytes(ProcessInstance, listItemsAddr + 0x20, size);
                        if (listBytes == null || listBytes.Length == 0) continue; 

                        // race safety check
                        ulong repeatListPtr = watcher.deepPointer.Deref<ulong>(ProcessInstance);
                        if (repeatListPtr != listAddr) continue;

                        // ---
                        if (type == typeof(IntPtr))
                        {
                            List<IntPtr> list = new List<IntPtr>();
                            for (int i = 0; i < size; i += itemSize) list.Add((IntPtr)BitConverter.ToInt64(listBytes, i));
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(UIntPtr))
                        {
                            List<UIntPtr> list = new List<UIntPtr>();
                            for (int i = 0; i < size; i += itemSize) list.Add((UIntPtr)BitConverter.ToUInt64(listBytes, i));
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(bool))
                        {
                            List<bool> list = new List<bool>();
                            for (int i = 0; i < size; i += itemSize) list.Add(BitConverter.ToBoolean(listBytes, i));
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(byte))
                        {
                            List<byte> list = new List<byte>();
                            for (int i = 0; i < size; i += itemSize) list.Add(listBytes[i]);
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(sbyte))
                        {
                            List<sbyte> list = new List<sbyte>();
                            for (int i = 0; i < size; i += itemSize) list.Add((sbyte)listBytes[i]);
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(char))
                        {
                            List<char> list = new List<char>();
                            for (int i = 0; i < size; i += itemSize) list.Add((char)listBytes[i]);
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(short))
                        {
                            List<short> list = new List<short>();
                            for (int i = 0; i < size; i += itemSize) list.Add(BitConverter.ToInt16(listBytes, i));
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(ushort))
                        {
                            List<ushort> list = new List<ushort>();
                            for (int i = 0; i < size; i += itemSize) list.Add(BitConverter.ToUInt16(listBytes, i));
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(int))
                        {
                            List<int> list = new List<int>();
                            for (int i = 0; i < size; i += itemSize) list.Add(BitConverter.ToInt32(listBytes, i));
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(uint))
                        {
                            List<uint> list = new List<uint>();
                            for (int i = 0; i < size; i += itemSize) list.Add(BitConverter.ToUInt32(listBytes, i));
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(long))
                        {
                            List<long> list = new List<long>();
                            for (int i = 0; i < size; i += itemSize) list.Add(BitConverter.ToInt64(listBytes, i));
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(ulong))
                        {
                            List<ulong> list = new List<ulong>();
                            for (int i = 0; i < size; i += itemSize) list.Add(BitConverter.ToUInt64(listBytes, i));
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(float))
                        {
                            List<float> list = new List<float>();
                            for (int i = 0; i < size; i += itemSize) list.Add(BitConverter.ToSingle(listBytes, i));
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(double))
                        {
                            List<double> list = new List<double>();
                            for (int i = 0; i < size; i += itemSize) list.Add(BitConverter.ToDouble(listBytes, i));
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(decimal))
                        {
                            List<decimal> list = new List<decimal>();
                            for (int i = 0; i < size; i += itemSize) list.Add(TUtils.ToDecimal(listBytes, i));
                            current[watcher.memoryWatcher.Name] = list;
                        }
                    }
                }
            }
            while (false);
        }
        catch { }
    }

    public dynamic CreateTool(string engine, string tool)
    {
        try
        {
            return CreateTool(engine, "default", tool);
        }
        catch { }
        return null;
    }

    public object CreateTool(string engine, string type, string tool)
    {
        ProcessInstance = null;
        TProcess.WaitTillSecondsOld(ProcessInstance, 1);
        if (!ReloadProcess()) throw new Exception();
        TProcess.WaitTillSecondsOld(ProcessInstance, 1);
        if (!ReloadProcess()) throw new Exception();

        try
        {
            engine = engine.ToLower();
            type = type.ToLower();
            tool = tool.ToLower();

            // unity
            if (ToolsShared.ToolNames.Unity.Data.Contains(engine))
            {
                if (ToolsShared.ToolNames.Unity.DotNet.Data.Contains(type))
                {
                    if (ToolsShared.ToolNames.Unity.DotNet.JitSave.Data.Contains(tool))
                    {
                        return new Tools.Unity.DotNet.JitSave();
                    }

                    if (ToolsShared.ToolNames.Unity.DotNet.Instance.Data.Contains(tool))
                    {
                        return new Tools.Unity.DotNet.Instance();
                    }
                }

                if (ToolsShared.ToolNames.Unity.Il2Cpp.Data.Contains(type))
                {
                    if (ToolsShared.ToolNames.Unity.Il2Cpp.JitSave.Data.Contains(tool))
                    {
                        return new Tools.Unity.IL2CPP.JitSave();
                    }

                    if (ToolsShared.ToolNames.Unity.Il2Cpp.Instance.Data.Contains(tool))
                    {
                        return new Tools.Unity.IL2CPP.Instance();
                    }
                }

                if (ToolsShared.ToolNames.Unity.Default.Data.Contains(type))
                {
                    if (ToolsShared.ToolNames.Unity.Default.GameObject.Data.Contains(tool))
                    {
                        return new Tools.Unity.Default.GameObject();
                    }
                }

                if (ToolsShared.ToolNames.Unity.Utils.Data.Contains(tool))
                {
                    return new Tools.Unity.Utilities();
                }
            }

            // unreal engine
            if (ToolsShared.ToolNames.UnrealEngine.Data.Contains(engine))
            {
                if (ToolsShared.ToolNames.UnrealEngine.Default.Data.Contains(type))
                {
                    if (ToolsShared.ToolNames.UnrealEngine.Default.Events.Data.Contains(tool))
                    {
                        return new Tools.UnrealEngine.Default.Events();
                    }

                    if (ToolsShared.ToolNames.UnrealEngine.Default.Utilities.Data.Contains(tool))
                    {
                        return new Tools.UnrealEngine.Default.Utilities();
                    }
                }
            }
        }
        catch { Thread.Sleep(500); }
        return null;
    }

    public void SetProcess(Process process)
    {
        try
        {
            try { ProcessInstance = Process.GetProcessById(process.Id); } catch { };
        }
        catch { }
    }
}