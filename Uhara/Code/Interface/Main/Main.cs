using LiveSplit.ComponentUtil;
using LiveSplit.Model;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
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
            //if (!File.Exists("SharpDisasm.dll"))
                //File.WriteAllBytes("SharpDisasm.dll", AsmBlocks.SharpDisasm);
        }
        catch { }
        DebugMode = false;
    }

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
                ProcessInstance = null;
                return true;
            }

            int exeModuleSize = TProcess.GetImageSize(ProcessInstance, module);
            if (moduleMemorySizes.Any(mms => mms == exeModuleSize))
            {
                ProcessInstance = null;
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
            if (!string.IsNullOrEmpty(message))
                TImports.OutputDebugString("[UHARA] " + message);
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
                        watcher.Update(ProcessInstance);
                        //if (watcher.Current != null)
                        current[watcher.Name] = watcher.Current;
                    }

                    foreach (var watcher in StringWatchers)
                    {
                        watcher.Update(ProcessInstance);
                        //if (!string.IsNullOrEmpty(watcher.Current))
                            current[watcher.Name] = watcher.Current;
                    }

                    foreach (var watcher in ListWatchers)
                    {
                        watcher.memoryWatcher.Update(ProcessInstance);
                        if (watcher.memoryWatcher.Current == null) continue;
                        if ((IntPtr)watcher.memoryWatcher.Current == IntPtr.Zero) continue;

                        ulong address = (ulong)watcher.memoryWatcher.Current;
                        Type type = watcher.type;

                        int size = TMemory.ReadMemory<ushort>(ProcessInstance, address + 0x18);

                        address = TMemory.ReadMemory<ulong>(ProcessInstance, address + 0x10);
                        if (address == 0) continue;

                        int count = TMemory.ReadMemory<ushort>(ProcessInstance, address + 0x18);
                        if (count > size) continue;

                        // ---
                        if (type == typeof(IntPtr))
                        {
                            List<IntPtr> list = new List<IntPtr>();
                            int itemSize = Marshal.SizeOf(typeof(IntPtr));

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<IntPtr>(ProcessInstance, address + (ulong)i));
                            }
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(bool))
                        {
                            List<bool> list = new List<bool>();
                            int itemSize = sizeof(bool);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<bool>(ProcessInstance, address + (ulong)i));
                            }
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(byte))
                        {
                            List<byte> list = new List<byte>();
                            int itemSize = sizeof(byte);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<byte>(ProcessInstance, address + (ulong)i));
                            }
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(sbyte))
                        {
                            List<sbyte> list = new List<sbyte>();
                            int itemSize = sizeof(sbyte);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<sbyte>(ProcessInstance, address + (ulong)i));
                            }
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(char))
                        {
                            List<char> list = new List<char>();
                            int itemSize = sizeof(char);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<char>(ProcessInstance, address + (ulong)i));
                            }
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(short))
                        {
                            List<short> list = new List<short>();
                            int itemSize = sizeof(short);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<short>(ProcessInstance, address + (ulong)i));
                            }
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(ushort))
                        {
                            List<ushort> list = new List<ushort>();
                            int itemSize = sizeof(ushort);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<ushort>(ProcessInstance, address + (ulong)i));
                            }
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(int))
                        {
                            List<int> list = new List<int>();
                            int itemSize = sizeof(int);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<int>(ProcessInstance, address + (ulong)i));
                            }
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(uint))
                        {
                            List<uint> list = new List<uint>();
                            int itemSize = sizeof(uint);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<uint>(ProcessInstance, address + (ulong)i));
                            }
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(long))
                        {
                            List<long> list = new List<long>();
                            int itemSize = sizeof(long);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<long>(ProcessInstance, address + (ulong)i));
                            }
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(ulong))
                        {
                            List<ulong> list = new List<ulong>();
                            int itemSize = sizeof(ulong);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<ulong>(ProcessInstance, address + (ulong)i));
                            }
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(float))
                        {
                            List<float> list = new List<float>();
                            int itemSize = sizeof(float);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<float>(ProcessInstance, address + (ulong)i));
                            }
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(double))
                        {
                            List<double> list = new List<double>();
                            int itemSize = sizeof(double);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<double>(ProcessInstance, address + (ulong)i));
                            }
                            current[watcher.memoryWatcher.Name] = list;
                        }

                        else if (type == typeof(decimal))
                        {
                            List<decimal> list = new List<decimal>();
                            int itemSize = sizeof(decimal);

                            for (int i = 0x20; i < count; i += itemSize)
                            {
                                list.Add(TMemory.ReadMemory<decimal>(ProcessInstance, address + (ulong)i));
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

    public dynamic CreateTool(string engine, string type, string tool)
    {
        try
        {
            TProcess.WaitTillSecondsOld(ProcessInstance, 3);
            if (!ReloadProcess()) throw new Exception();

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