using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

internal class UProcess
{
    public static IntPtr CreateRemoteThread(Process process, ulong entryPointAddress, bool waitForThread = false)
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

    public static bool WaitForThread(IntPtr threadHandle, uint timeout = 0xFFFFFFFF)
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
            //Process.GetProcessById(process.Id) != null;
        }
        catch { }
        return false;
    }
}
