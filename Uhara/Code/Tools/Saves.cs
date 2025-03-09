using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class USaves
{
    private static string software = "uhara";
    private static string developer = "rumii";
    private static string keyPath = @"Software" + "\\" + developer + "\\" + software;

    internal static string Get(string name)
    {
        RegistryKey key = CheckGetDefault();
        if (key != null)
        {
            object value = key.GetValue(name);
            if (value != null) return value.ToString();
        }
        return "";
    }

    internal static void Set(string name, string value)
    {
        RegistryKey key = CheckGetDefault();
        if (key != null)
        {
            key.SetValue(name, value, RegistryValueKind.String);
        }
    }

    private static RegistryKey CheckGetDefault()
    {
        RegistryKey hkcu = Registry.CurrentUser;
        RegistryKey key = hkcu.OpenSubKey(keyPath, writable: true);
        if (key == null) return hkcu.CreateSubKey(keyPath);
        else return key;
    }
}
