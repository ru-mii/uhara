using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

internal class TProcess
{
    internal static ProcessModule GetModule(Process process, string name = null)
    {
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
            WaitForThread(UImports.CreateRemoteThread(process.Handle, IntPtr.Zero, 0,
                (IntPtr)entryPointAddress, IntPtr.Zero, 0, out _));
        }
        else remoteThread = UImports.CreateRemoteThread(process.Handle, IntPtr.Zero, 0,
                (IntPtr)entryPointAddress, IntPtr.Zero, 0, out _);

        return remoteThread;
    }

    internal static bool WaitForThread(IntPtr threadHandle, uint timeout = 0xFFFFFFFF)
    {
        return UImports.WaitForSingleObject(threadHandle, timeout) == 0;
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
