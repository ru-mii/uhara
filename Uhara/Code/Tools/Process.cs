using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

internal class UProcess
{
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
