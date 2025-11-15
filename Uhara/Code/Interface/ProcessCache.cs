using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class ProcessCache : MainShared
{
    internal static void Set(string name, string value)
    {
        do
        {
            string token = TProcess.GetToken(ProcessInstance);
            if (token == null) break;

            TSaves2.Set(token + "," + value,
                "ProcessCache", name);
        }
        while (false);
    }

    internal static string Get(string name)
    {
        do
        {
            string token = TProcess.GetToken(ProcessInstance);
            if (token == null) break;

            string getRaw = TSaves2.Get("ProcessCache", name);
            if (string.IsNullOrEmpty(getRaw)) break;

            string[] data = getRaw.Split(',');
            if (data.Length < 2) break;

            if (data[0] == token)
                return data[1];
        }
        while (false);
        return null;
    }
}
