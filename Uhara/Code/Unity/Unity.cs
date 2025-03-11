using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class UUnity : Shared
{
    private bool JitSetup = false;

    IntPtr SaveRegister(string _namespace, string _class, string _method, int parametersCount)
    {
        if (CheckSetInstance())
        {
            try
            {

            }
            catch { }
        }
        return IntPtr.Zero;
    }
}
