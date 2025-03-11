using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class Shared
{
    internal static Process Instance = null;

    internal static bool CheckSetInstance()
    {
        try
        {
            if (UProcess.IsAlive(Instance)) return true;

            dynamic currState = UReflection.GetValue(UReflection.GetValue(Application.OpenForms["TimerForm"],
                    "<CurrentState>k__BackingField"));

            if (currState != null)
            {
                Instance = UReflection.GetValue(UReflection.GetValue(currState,
                    "<Run>k__BackingField",
                    "<AutoSplitter>k__BackingField",
                    "<Component>k__BackingField",
                    "<Script>k__BackingField",
                    "_game"));

                if (!UProcess.IsAlive(Instance))
                {
                    dynamic value = UReflection.GetValue(UReflection.GetValue(currState,
                    "<Layout>k__BackingField",
                    "<LayoutComponents>k__BackingField",
                    "_items"));

                    if (value != null)
                    {
                        int elements = value.Length;
                        for (int i = 0; i < elements; i++)
                        {
                            dynamic index = value.GetValue(i);
                            if (index == null) break;
                            else
                            {
                                dynamic script = UReflection.GetValue(index,
                                    "<Component>k__BackingField",
                                    "<Script>k__BackingField");

                                if (script != null)
                                {
                                    Instance = UReflection.GetValue(script, "_game");
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        catch { }
        return UProcess.IsAlive(Instance);
    }
}