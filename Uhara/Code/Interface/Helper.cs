using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public partial class Main
{
    public IntPtr ScanSingle(string signature)
    {
        try
        {
            if (GetSetValues())
                return (IntPtr)UMemory.ScanSingle(signature);
        }
        catch { }
        return IntPtr.Zero;
    }

    public IntPtr ScanRel(int offset, string signature)
    {
        try
        {
            if (GetSetValues())
                return (IntPtr)UMemory.ScanRel(offset, signature);
        }
        catch { }
        return IntPtr.Zero;
    }

    public string GetCategoryName()
    {
        try
        {
            return UReflection.GetValue(UReflection.GetValue(Application.OpenForms["TimerForm"],
            "<CurrentState>k__BackingField",
            "<Run>k__BackingField",
            "categoryName")).ToString();
        }
        catch { }
        return null;
    }
}
