using LiveSplit.ComponentUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public class PtrResolver : MainShared
{
    public void Watch<T>(string name, object base_, params int[] offsets) where T : unmanaged
    {
        WatchFull<T>(name, base_, offsets: offsets);
    }

    public void Watch<T>(string name, string moduleName, object base_, params int[] offsets) where T : unmanaged
    {
        WatchFull<T>(name, base_, moduleName, offsets);
    }

    public void Watch<T>(string name, Module module, object base_, params int[] offsets) where T : unmanaged
    {
        WatchFull<T>(name, base_, module.Name, offsets);
    }

    public void WatchFull<T>(string name, object base_, string moduleName = null, params int[] offsets) where T : unmanaged
    {
        try
        {
            DeepPointer deepPointer;
            if (base_.GetType() == typeof(int))
            {
                if (string.IsNullOrEmpty(moduleName)) deepPointer = new DeepPointer((int)base_, offsets);
                else deepPointer = new DeepPointer(moduleName, (int)base_, offsets);
            }
            else deepPointer = new DeepPointer((IntPtr)base_, offsets);

            MemoryWatcher memoryWatcher = new MemoryWatcher<T>(deepPointer);
            memoryWatcher.Name = name;
            MemoryWatchers.Add(memoryWatcher);

            current[name] = default(T);
        }
        catch { }
    }

    // ---

    public void WatchString(string name, object base_, params int[] offsets)
    {
        WatchStringFull(name, base_, offsets: offsets);
    }

    public void WatchString(string name, int length, object base_, params int[] offsets)
    {
        WatchStringFull(name, base_, length: length, offsets: offsets);
    }

    public void WatchString(string name, ReadStringType readStringType, object base_, params int[] offsets)
    {
        WatchString(name, base_, offsets: offsets);
    }

    public void WatchString(string name, int length, ReadStringType readStringType, object base_, params int[] offsets)
    {
        WatchStringFull(name, base_, length, offsets: offsets);
    }

    public void WatchString(string name, string moduleName, object base_, params int[] offsets)
    {
        WatchStringFull(name, base_, moduleName: moduleName, offsets: offsets);
    }

    public void WatchString(string name, int length, string moduleName, object base_, params int[] offsets)
    {
        WatchStringFull(name, base_, length, moduleName: moduleName, offsets: offsets);
    }

    public void WatchString(string name, ReadStringType readStringType, string moduleName, object base_, params int[] offsets)
    {
        WatchStringFull(name, base_, readStringType: readStringType, moduleName: moduleName, offsets: offsets);
    }

    public void WatchString(string name, int length, ReadStringType readStringType, string moduleName, object base_, params int[] offsets)
    {
        WatchStringFull(name, base_, length, readStringType, moduleName, offsets);
    }

    public void WatchString(string name, Module module, object base_, params int[] offsets)
    {
        WatchStringFull(name, base_, moduleName: module.Name, offsets: offsets);
    }

    public void WatchString(string name, int length, Module module, object base_, params int[] offsets)
    {
        WatchStringFull(name, base_, length, moduleName: module.Name, offsets: offsets);
    }

    public void WatchString(string name, ReadStringType readStringType, Module module, object base_, params int[] offsets)
    {
        WatchStringFull(name, base_, readStringType :readStringType, moduleName: module.Name, offsets: offsets);
    }

    public void WatchString(string name, int length, ReadStringType readStringType, Module module, object base_, params int[] offsets)
    {
        WatchStringFull(name, base_, length, readStringType, module.Name, offsets);
    }

    public void WatchStringFull(string name, object base_, int length = 128, ReadStringType readStringType = ReadStringType.AutoDetect,
        string moduleName = null, params int[] offsets)
    {
        try
        {
            DeepPointer deepPointer;
            if (base_.GetType() == typeof(int))
            {
                if (string.IsNullOrEmpty(moduleName)) deepPointer = new DeepPointer((int)base_, offsets);
                else deepPointer = new DeepPointer(moduleName, (int)base_, offsets);
            }
            else deepPointer = new DeepPointer((IntPtr)base_, offsets);

            StringWatcher stringWatcher = new StringWatcher(deepPointer, 128);
            stringWatcher.Name = name;
            StringWatchers.Add(stringWatcher);

            current[name] = null;
        }
        catch { }
    }
}