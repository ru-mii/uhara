using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

internal class UReflection
{
    internal static dynamic GetValue(dynamic start, params string[] objects)
    {
        try
        {
            foreach (string name in objects)
            {
                start = start.GetType().GetField(name, BindingFlags.NonPublic |
                    BindingFlags.Instance).GetValue(start);

                if (start == null) return null;
            }
        }
        catch { return null; }
        return start;
    }
}
