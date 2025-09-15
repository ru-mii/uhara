using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

internal class TProcess
{
    internal static ulong GetModuleEnd(Process process, string name = null)
    {
        if (name == null) return (ulong)process.MainModule.BaseAddress + (ulong)process.MainModule.ModuleMemorySize;
        else
        {
            ProcessModule module = GetModule(process, name);
            if (module == null) return 0;

            return (ulong)module.BaseAddress + (ulong)module.ModuleMemorySize;
        }
    }

    internal static ulong GetModuleBase(Process process, string name = null)
    {
        if (name == null)
        {
            return (ulong)process.MainModule.BaseAddress;
        }
        else
        {
            foreach (ProcessModule module in process.Modules)
            {
                if (module.ModuleName.ToLower() == name.ToLower())
                {
                    return (ulong)module.BaseAddress;
                }
            }
        }
        return 0;
    }

    internal static ulong GetProcAddress(Process process, string moduleName, string functionName)
    {
        do
        {
            ProcessModule module = GetModule(process, moduleName);
            if (module == null) break;

            ulong moduleBase = (ulong)module.BaseAddress;
            if (moduleBase == 0) break;

            return GetProcAddress(process, moduleBase, functionName);
        }
        while (false);
        return 0;
    }

    internal static ulong GetProcAddress(Process process, ulong moduleBase, string functionName)
    {
        byte[] searchNameBytes = TUtils.StringToMultibyte(functionName);

        ulong rbx = moduleBase;
        ulong rax = TMemory.ReadMemory<uint>(process, moduleBase + 0x3C);
        rax += rbx; // ntHeader
        ulong rcx = TMemory.ReadMemory<uint>(process, rax + 0x88); // export RVA
        rcx += rbx; // exportDir absolute
        ulong r10 = TMemory.ReadMemory<uint>(process, rcx + 0x18); // NumberOfNames
        ulong r11 = TMemory.ReadMemory<uint>(process, rcx + 0x20); // AddressOfNames RVA
        r11 += rbx; // absolute
        ulong r12 = TMemory.ReadMemory<uint>(process, rcx + 0x24); // AddressOfNameOrdinals RVA
        r12 += rbx; // absolute
        ulong r13 = TMemory.ReadMemory<uint>(process, rcx + 0x1C); // AddressOfFunctions RVA
        r13 += rbx; // absolute
        ulong rdx = 0;

        while (rdx < r10)
        {
            rax = TMemory.ReadMemory<uint>(process, r11 + (rdx * 4)); // name RVA
            rax += rbx; // absolute ptr to name string

            byte[] nameBytes = TMemory.ReadMemoryBytes(process, rax, searchNameBytes.Length);
            if (nameBytes.SequenceEqual(searchNameBytes))
            {
                ulong ordinal = TMemory.ReadMemory<ushort>(process, r12 + (rdx * 2));
                ulong funcRVA = TMemory.ReadMemory<uint>(process, r13 + (ordinal * 4));
                return rbx + funcRVA;
            }

            rdx++;
        }

        return 0;
    }

    internal static ProcessModule GetModule(Process process, string name = null)
    {
        if (process == null) return null;

        if (name == null)
        {
            return process.MainModule;
        }
        else
        {
            foreach (ProcessModule module in process.Modules)
            {
                if (module.ModuleName.ToLower() == name.ToLower())
                {
                    return module;
                }
            }
        }
        return null;
    }

    internal static string GetToken(Process process)
    {
        return GetStartTime(process).ToString("X") + "-" + process.Id.ToString("X");
    }

    internal static ulong GetStartTime(Process process)
    {
        return (ulong)((DateTimeOffset)process.StartTime).ToUnixTimeMilliseconds();
    }

    internal static IntPtr CreateRemoteThread(Process process, ulong entryPointAddress, bool waitForThread = false)
    {
        IntPtr remoteThread = IntPtr.Zero;

        if (waitForThread)
        {
            WaitForThread(TImports.CreateRemoteThread(process.Handle, IntPtr.Zero, 0,
                (IntPtr)entryPointAddress, IntPtr.Zero, 0, out _));
        }
        else remoteThread = TImports.CreateRemoteThread(process.Handle, IntPtr.Zero, 0,
                (IntPtr)entryPointAddress, IntPtr.Zero, 0, out _);

        return remoteThread;
    }

    internal static bool WaitForThread(IntPtr threadHandle, uint timeout = 0xFFFFFFFF)
    {
        return TImports.WaitForSingleObject(threadHandle, timeout) == 0;
    }

    internal static ulong GetTime(Process process)
    {
        ulong startTime = (ulong)((DateTimeOffset)process.StartTime).ToUnixTimeMilliseconds();
        ulong currentTime = (ulong)((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds();
        return currentTime - startTime;
    }

    internal static Process RefreshProcess(Process process)
    {
        try
        {
            do
            {
                Process newProcess = Process.GetProcessById(process.Id);
                if (newProcess == null) break;

                if (GetToken(process) != GetToken(newProcess)) break;
                if (process.ProcessName != newProcess.ProcessName) break;

                process = newProcess;
                return process;
            }
            while (false);
        }
        catch { }
        process = null;
        return null;
    }

    internal static bool IsAlive(Process process)
    {
        try
        {
            return
            process != null &&
            !process.HasExited &&
            process.MainModule != null;
        }
        catch { }
        return false;
    }
}
