using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

internal class UProcess
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
