using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class TSaves2
{
    private static string Developer = "";
    private static string Software = "";

    private static bool Registered = false;

    public static void DeleteKey(string path)
    {
        DeleteKey(new string[] { path });
    }

    public static void DeleteKey(params string[] location)
    {
        string keyPath = ConstructPath(location, withoutLast: true);
        RegistryKey key = GetCreateKey(keyPath, false);

        if (key != null)
        {
            key.DeleteSubKeyTree(location[location.Length - 1]);
            key.Close();
        }
    }

    public static void DeleteValue(string path)
    {
        DeleteValue(new string[] { path });
    }

    public static void DeleteValue(params string[] location)
    {
        string keyPath = ConstructPath(location, withoutLast: true);
        RegistryKey key = GetCreateKey(keyPath, false);

        if (key != null)
        {
            key.DeleteValue(location[location.Length - 1], false);
            key.Close();
        }
    }

    public static string[] GetValueNames(params string[] location)
    {
        if (Registered)
        {
            string keyPath = ConstructPath(location, withoutLast: false);
            RegistryKey key = GetCreateKey(keyPath, false);

            if (key != null)
            {
                string[] valueNames = key.GetValueNames();
                key.Close();
                return valueNames;
            }
        }

        return Array.Empty<string>();
    }

    public static string[] GetKeyNames(params string[] location)
    {
        if (Registered)
        {
            string keyPath = ConstructPath(location, withoutLast: false);
            RegistryKey key = GetCreateKey(keyPath, false);

            if (key != null)
            {
                string[] subKeys = key.GetSubKeyNames();
                key.Close();
                return subKeys;
            }
        }

        return Array.Empty<string>();
    }

    public static void Register(string developer, string software)
    {
        Developer = developer;
        Software = software;
        Registered = true;
    }

    public static string Get(string name)
    {
        return Get(new string[] { name });
    }

    public static string Get(params string[] location)
    {
        if (Registered)
        {
            string keyPath = ConstructPath(location, withoutLast: true);
            RegistryKey key = GetCreateKey(keyPath, false);

            if (key != null)
            {
                object value = key.GetValue(location[location.Length - 1]);
                key.Close();

                if (value != null)
                    return value.ToString();
            }
        }

        return null;
    }

    public static void Set(string data, string name)
    {
        Set(data, new string[] { name });
    }

    public static void Set(string data, params string[] location)
    {
        if (Registered)
        {
            string keyPath = ConstructPath(location, withoutLast: true);
            RegistryKey key = GetCreateKey(keyPath, true);

            if (key != null)
            {
                key.SetValue(location[location.Length - 1], data, RegistryValueKind.String);
                key.Close();
            }
        }
    }

    private static RegistryKey GetCreateKey(string keyPath, bool createIfNull)
    {
        RegistryKey hkcu = Registry.CurrentUser;
        RegistryKey key = hkcu.OpenSubKey(keyPath, writable: true);
        if (key == null)
        {
            if (createIfNull) key = hkcu.CreateSubKey(keyPath);

            hkcu.Close();
            return key;
        }
        else
        {
            hkcu.Close();
            return key;
        }
    }

    private static string ConstructPath(string[] location, bool withoutLast)
    {
        string keyPath = @"Software" + "\\" + Developer + "\\" + Software;

        if (withoutLast)
        {
            for (int i = 0; i < location.Length - 1; i++)
                keyPath += "\\" + location[i];
        }
        else
        {
            for (int i = 0; i < location.Length; i++)
                keyPath += "\\" + location[i];
        }

        return keyPath;
    }
}
