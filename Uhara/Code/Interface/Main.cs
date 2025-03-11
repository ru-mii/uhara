using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class Main : Shared
{
    public dynamic CreateTool(string name)
    {
        name = name.ToLower();

        if (name == "unity" ||
        name == "unity3d")
        {
            return new UUnity();
        }

        else return null;
    }

    public void SetProcess(Process process)
    {
        Instance = process;
    }

    public Main ()
    {
        
    }
}