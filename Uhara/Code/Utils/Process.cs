using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

internal class TProcess
{
    internal static bool Is64Bit(Process process)
    {
        IntPtr processHandle = TImports.OpenProcess(0x0400 | 0x0010, false, process.Id);

        if (processHandle != IntPtr.Zero)
        {
            IntPtr peHeaderAddress = process.MainModule.BaseAddress;
            byte[] buffer = new byte[4096];
            TImports.ReadProcessMemory(processHandle, peHeaderAddress, buffer, buffer.Length, out _);

            int peHeaderOffset = BitConverter.ToInt32(buffer, 0x3C);
            int machineOffset = peHeaderOffset + 4;
            ushort machine = BitConverter.ToUInt16(buffer, machineOffset);

            TImports.CloseHandle(processHandle);
            if (machine == 0x014c) return false;
            else return true;
        }
        return true;
    }

    internal static int GetImageSize(Process process, ProcessModule module)
    {
        if (module == null) module = process.MainModule;

        TImports.MODULEINFO modInfo;
        if (TImports.GetModuleInformation(process.Handle, module.BaseAddress,
        out modInfo, Marshal.SizeOf(typeof(TImports.MODULEINFO))))
            return modInfo.SizeOfImage;

        return 0;
    }

    internal static int GetImageSize(Process process, string moduleName = null)
    {
        ProcessModule module = GetModule(process, moduleName);
        return GetImageSize(process, module);
    }

    internal static bool WaitTillSecondsOld(Process process, int seconds)
    {
        ulong currentTime = TUtils.GetTimeMiliseconds();
        ulong processStartTime = GetStartTimeMiliseconds(process);

        if (processStartTime != 0)
        {
            long waitTime = (long)((processStartTime + (ulong)(seconds * 1000) - currentTime));
            if (waitTime > 0) Thread.Sleep((int)waitTime);
            return true;
        }
        return false;
    }

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
        return GetStartTimeMiliseconds(process).ToString("X") + "-" + process.Id.ToString("X");
    }

    internal static ulong GetStartTimeSeconds(Process process)
    {
        return (ulong)((DateTimeOffset)process.StartTime).ToUnixTimeSeconds();
    }

    internal static ulong GetStartTimeMiliseconds(Process process)
    {
        return (ulong)((DateTimeOffset)process.StartTime).ToUnixTimeMilliseconds();
    }

    internal static IntPtr CreateRemoteThread(Process process, ulong entryPointAddress, int waitForThread = 0)
    {
        IntPtr remoteThread = TImports.CreateRemoteThread(process.Handle, IntPtr.Zero, 0,
                    (IntPtr)entryPointAddress, IntPtr.Zero, 0, out _);

        if (waitForThread != 0) WaitForThread(remoteThread, waitForThread);

        return remoteThread;
    }

    internal static bool WaitForThread(IntPtr threadHandle, int timeout = -1)
    {
        return TImports.WaitForSingleObject(threadHandle, (uint)timeout) == 0;
    }

    internal static ulong GetTime(Process process)
    {
        ulong startTime = (ulong)((DateTimeOffset)process.StartTime).ToUnixTimeMilliseconds();
        ulong currentTime = (ulong)((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds();
        return currentTime - startTime;
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
