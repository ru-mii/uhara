using LiveSplit.ComponentUtil;
using LiveSplit.Model;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
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

public partial class Main : MainShared
{
    public ScriptSettings Settings = new ScriptSettings();
    public FileLogger FileLogger = new FileLogger();

    public Main()
    {
        try
        {
            TSaves2.Register("rumii", "uhara");
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
        DebugMode = false;
    }

    /*
    public TypeDefinition Define(string source, params string[] references)
    {
        CSharpCodeProvider _codeProvider = new CSharpCodeProvider();
        CompilerParameters parameters = new()
        {
            GenerateInMemory = false,
            GenerateExecutable = false,
            CompilerOptions = "/optimize"
        };

        string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".dll");
        parameters.OutputAssembly = tempPath;

        parameters.ReferencedAssemblies.Add("mscorlib.dll");
        parameters.ReferencedAssemblies.Add("System.dll");
        parameters.ReferencedAssemblies.AddRange(references);

        CompilerResults results = _codeProvider.CompileAssemblyFromSource(parameters, source);

        if (results.Errors.HasErrors)
        {
            string errorMsg = string.Join("\n", results.Errors.Cast<CompilerError>().Select(e => $"{e.Line}: {e.ErrorText}"));
            throw new Exception("Compilation failed:\n" + errorMsg);
        }

        byte[] bytes;
        try { bytes = File.ReadAllBytes(tempPath); }
        finally { File.Delete(tempPath); }

        using var ms = new MemoryStream(bytes);
        using var peReader = new PEReader(ms);
        var reader = peReader.GetMetadataReader();

        TypeDefinition? found = null;
        int count = 0;
        foreach (var handle in reader.TypeDefinitions)
        {
            var td = reader.GetTypeDefinition(handle);
            string name = reader.GetString(td.Name);
            if (name != "<Module>")
            {
                found = td;
                count++;
            }
        }

        if (count == 0)
        {
            throw new Exception("The provided source code did not contain a type");
        }
        else if (count > 1)
        {
            throw new Exception("Multiple types found; expected only one");
        }

        return found.Value;
    }*/

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
                            script.GetType().GetField("_game", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(ProcessInstance, process);
                            return;
                        }
                    }
                    catch { }
                }
            }
            while (false);
        }
        catch { }
        script.GetType().GetField("_game", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(script, null);
    }

    public bool RejectOnFound(string signature)
    {
        try
        {
            if (DeveloperMode) TUtils.Print("Rejection check");
            if (TMemory.ScanSingle(ProcessInstance, signature) != 0)
            {
                if (DeveloperMode) TUtils.Print("REJECTED");
                Reject();
            }
            if (DeveloperMode) TUtils.Print("NOT REJECTED");
        }
        catch { }
        return true;
    }

    public bool Reject(bool condition = true)
    {
        try
        {
            if (condition)
            {
                script.GetType().GetField("_game", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(script, null);
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
                script.GetType().GetField("_game", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(script, null);
                return true;
            }

            int exeModuleSize = TProcess.GetImageSize(ProcessInstance, module);
            if (moduleMemorySizes.Any(mms => mms == exeModuleSize))
            {
                script.GetType().GetField("_game", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(script, null);
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

    public string GetMD5Hash(string path)
    {
        try
        {
            if (!File.Exists(path)) path = Path.Combine(Path.GetDirectoryName(ProcessInstance.MainModule.FileName), path);
            return GetHash(path);
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

    public void EnableDeveloperMode()
    {
        try
        {
            DeveloperMode = true;
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

    public void Update()
    {
        UpdateCounter++;

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

                        ulong address = (ulong)(IntPtr)(memWatcher.Current);
                        Type type = watcher.type;

                        int itemSize = Marshal.SizeOf(type);

                        int count = TMemory.ReadMemory<ushort>(ProcessInstance, address + 0x18);
                        int size = count * itemSize;

                        // ---
                        if (type == typeof(IntPtr))
                        {
                            List<IntPtr> list = new List<IntPtr>();
                            byte[] bytes = TMemory.ReadMemoryBytes(ProcessInstance, address + 0x20, size);

                            for (int i = 0; i < size; i += itemSize)
                            {
                                IntPtr value = (IntPtr)BitConverter.ToUInt64(bytes, i);
                                list.Add(value);
                            }

                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(bool))
                        {
                            List<bool> list = new List<bool>();
                            byte[] bytes = TMemory.ReadMemoryBytes(ProcessInstance, address + 0x20, size);

                            for (int i = 0; i < size; i += itemSize)
                            {
                                bool value = BitConverter.ToBoolean(bytes, i);
                                list.Add(value);
                            }

                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(byte))
                        {
                            List<byte> list = new List<byte>();
                            byte[] bytes = TMemory.ReadMemoryBytes(ProcessInstance, address + 0x20, size);

                            for (int i = 0; i < size; i += itemSize)
                            {
                                byte value = bytes[i];
                                list.Add(value);
                            }

                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(sbyte))
                        {
                            List<sbyte> list = new List<sbyte>();
                            byte[] bytes = TMemory.ReadMemoryBytes(ProcessInstance, address + 0x20, size);

                            for (int i = 0; i < size; i += itemSize)
                            {
                                sbyte value = (sbyte)bytes[i];
                                list.Add(value);
                            }

                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(char))
                        {
                            List<char> list = new List<char>();
                            byte[] bytes = TMemory.ReadMemoryBytes(ProcessInstance, address + 0x20, size);

                            for (int i = 0; i < size; i += itemSize)
                            {
                                char value = (char)bytes[i];
                                list.Add(value);
                            }

                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(short))
                        {
                            List<short> list = new List<short>();
                            byte[] bytes = TMemory.ReadMemoryBytes(ProcessInstance, address + 0x20, size);

                            for (int i = 0; i < size; i += itemSize)
                            {
                                short value = BitConverter.ToInt16(bytes, i);
                                list.Add(value);
                            }

                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(ushort))
                        {
                            List<ushort> list = new List<ushort>();
                            byte[] bytes = TMemory.ReadMemoryBytes(ProcessInstance, address + 0x20, size);

                            for (int i = 0; i < size; i += itemSize)
                            {
                                ushort value = BitConverter.ToUInt16(bytes, i);
                                list.Add(value);
                            }

                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(int))
                        {
                            List<int> list = new List<int>();
                            byte[] bytes = TMemory.ReadMemoryBytes(ProcessInstance, address + 0x20, size);

                            for (int i = 0; i < size; i += itemSize)
                            {
                                int value = BitConverter.ToInt32(bytes, i);
                                list.Add(value);
                            }

                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(uint))
                        {
                            List<uint> list = new List<uint>();
                            byte[] bytes = TMemory.ReadMemoryBytes(ProcessInstance, address + 0x20, size);

                            for (int i = 0; i < size; i += itemSize)
                            {
                                uint value = BitConverter.ToUInt32(bytes, i);
                                list.Add(value);
                            }

                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(long))
                        {
                            List<long> list = new List<long>();
                            byte[] bytes = TMemory.ReadMemoryBytes(ProcessInstance, address + 0x20, size);

                            for (int i = 0; i < size; i += itemSize)
                            {
                                long value = BitConverter.ToInt64(bytes, i);
                                list.Add(value);
                            }

                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(ulong))
                        {
                            List<ulong> list = new List<ulong>();
                            byte[] bytes = TMemory.ReadMemoryBytes(ProcessInstance, address + 0x20, size);

                            for (int i = 0; i < size; i += itemSize)
                            {
                                ulong value = BitConverter.ToUInt64(bytes, i);
                                list.Add(value);
                            }

                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(float))
                        {
                            List<float> list = new List<float>();
                            byte[] bytes = TMemory.ReadMemoryBytes(ProcessInstance, address + 0x20, size);

                            for (int i = 0; i < size; i += itemSize)
                            {
                                float value = BitConverter.ToSingle(bytes, i);
                                list.Add(value);
                            }

                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(double))
                        {
                            List<double> list = new List<double>();
                            byte[] bytes = TMemory.ReadMemoryBytes(ProcessInstance, address + 0x20, size);

                            for (int i = 0; i < size; i += itemSize)
                            {
                                double value = BitConverter.ToDouble(bytes, i);
                                list.Add(value);
                            }

                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(decimal))
                        {
                            List<decimal> list = new List<decimal>();
                            byte[] bytes = TMemory.ReadMemoryBytes(ProcessInstance, address + 0x20, size);

                            for (int i = 0; i < size; i += itemSize)
                            {
                                decimal value = TUtils.ToDecimal(bytes, i);
                                list.Add(value);
                            }

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
        TProcess.WaitTillSecondsOld(ProcessInstance, 1);
        if (!ReloadProcess()) throw new Exception();
        TProcess.WaitTillSecondsOld(ProcessInstance, 1);
        if (!ReloadProcess()) throw new Exception();

        try
        {
            engine = engine.ToLower();
            type = type.ToLower();
            tool = tool.ToLower();

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

                if (ToolsShared.ToolNames.Unity.Utils.Data.Contains(tool))
                {
                    return new Tools.Unity.Utilities();
                }
            }

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

            if (ToolsShared.ToolNames.UnrealEngine.Data.Contains(engine))
            {
                if (ToolsShared.ToolNames.UnrealEngine.Default.Data.Contains(type))
                {
                    if (ToolsShared.ToolNames.UnrealEngine.Default.CutsceneManager.Data.Contains(tool))
                    {
                        //return new Tools.UnrealEngine.Default.CutsceneManager();
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
            ProcessInstance = process;
        }
        catch { }
    }
}