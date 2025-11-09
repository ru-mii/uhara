using LiveSplit.ComponentUtil;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public class PtrResolver : MainShared
{
    #region DEREF
    public IntPtr Deref((IntPtr _base, int[] offsets) offsets)
    {
        return _Deref(offsets._base, offsets: offsets.offsets);
    }

    public IntPtr Deref(object _base, params int[] offsets)
    {
        return _Deref(_base, offsets: offsets);
    }

    public IntPtr Deref(string moduleName, object _base, params int[] offsets)
    {
        return _Deref(_base, moduleName, offsets);
    }

    public IntPtr Deref(Module module, object _base, params int[] offsets)
    {
        return _Deref(_base, module.Name, offsets);
    }

    public IntPtr Deref(object _base, string moduleName = null, params int[] offsets)
    {
        return _Deref(_base, moduleName, offsets);
    }

    private IntPtr _Deref(object _base, string moduleName = null, params int[] offsets)
    {
        try
        {
            DeepPointer deepPointer = new DeepPointer((IntPtr)_base, offsets);
            return deepPointer.Deref<IntPtr>(ProcessInstance);
        }
        catch { }
        return IntPtr.Zero;
    }
    #endregion
    #region READ
    public T Read<T>((IntPtr _base, int[] offsets) offsets) where T : unmanaged
    {
        return _Read<T>(offsets._base, offsets: offsets.offsets);
    }

    public T Read<T>(object _base, params int[] offsets) where T : unmanaged
    {
        return _Read<T>(_base, offsets: offsets);
    }

    public T Read<T>(string moduleName, object _base, params int[] offsets) where T : unmanaged
    {
        return _Read<T>(_base, moduleName, offsets);
    }

    public T Read<T>(Module module, object _base, params int[] offsets) where T : unmanaged
    {
        return _Read<T>(_base, module.Name, offsets);
    }

    public T Read<T>(object _base, string moduleName = null, params int[] offsets) where T : unmanaged
    {
        return _Read<T>(_base, moduleName, offsets);
    }

    private T _Read<T>(object _base, string moduleName = null, params int[] offsets) where T : unmanaged
    {
        try
        {
            DeepPointer deepPointer;
            if (_base is int)
            {
                if (string.IsNullOrEmpty(moduleName)) deepPointer = new DeepPointer((int)_base, offsets);
                else deepPointer = new DeepPointer(moduleName, (int)_base, offsets);
            }
            else deepPointer = new DeepPointer((IntPtr)_base, offsets);

            return deepPointer.Deref<T>(ProcessInstance);
        }
        catch { }
        return default(T);
    }
    #endregion

    #region WATCH
    public void Watch<T>(string name, (IntPtr _base, int[] offsets) offsets) where T : unmanaged
    {
        _Watch<T>(name, offsets._base, offsets: offsets.offsets);
    }

    public void Watch<T>(string name, object _base, params int[] offsets) where T : unmanaged
    {
        _Watch<T>(name, _base, offsets: offsets);
    }

    public void Watch<T>(string name, string moduleName, object _base, params int[] offsets) where T : unmanaged
    {
        _Watch<T>(name, _base, moduleName, offsets);
    }

    public void Watch<T>(string name, Module module, object _base, params int[] offsets) where T : unmanaged
    {
        _Watch<T>(name, _base, module.Name, offsets);
    }

    public void Watch<T>(string name, object _base, string moduleName = null, params int[] offsets) where T : unmanaged
    {
        _Watch<T>(name, _base, moduleName, offsets);
    }

    private void _Watch<T>(string name, object _base, string moduleName = null, params int[] offsets) where T : unmanaged
    {
        try
        {
            DeepPointer deepPointer;
            if (_base.GetType() == typeof(int))
            {
                if (string.IsNullOrEmpty(moduleName)) deepPointer = new DeepPointer((int)_base, offsets);
                else deepPointer = new DeepPointer(moduleName, (int)_base, offsets);
            }
            else deepPointer = new DeepPointer((IntPtr)_base, offsets);

            MemoryWatcher memoryWatcher = new MemoryWatcher<T>(deepPointer);
            memoryWatcher.Name = name;
            memoryWatcher.Current = default(T);
            MemoryWatchers.Add(memoryWatcher);
        }
        catch { }
    }
    #endregion
    #region WATCH_LIST
    public void WatchList<T>(string name, (IntPtr _base, int[] offsets) offsets) where T : unmanaged
    {
        _WatchList<T>(name, offsets._base, offsets: offsets.offsets);
    }

    public void WatchList<T>(string name, object _base, params int[] offsets) where T : unmanaged
    {
        _WatchList<T>(name, _base, offsets: offsets);
    }

    public void WatchList<T>(string name, string moduleName, object _base, params int[] offsets) where T : unmanaged
    {
        _WatchList<T>(name, _base, moduleName, offsets);
    }

    public void WatchList<T>(string name, Module module, object _base, params int[] offsets) where T : unmanaged
    {
        _WatchList<T>(name, _base, module.Name, offsets);
    }

    public void WatchList<T>(string name, object _base, string moduleName = null, params int[] offsets) where T : unmanaged
    {
        _WatchList<T>(name, _base, moduleName, offsets);
    }

    private void _WatchList<T>(string name, object _base, string moduleName = null, params int[] offsets) where T : unmanaged
    {
        try
        {
            DeepPointer deepPointer;
            if (_base.GetType() == typeof(int))
            {
                if (string.IsNullOrEmpty(moduleName)) deepPointer = new DeepPointer((int)_base, offsets);
                else deepPointer = new DeepPointer(moduleName, (int)_base, offsets);
            }
            else deepPointer = new DeepPointer((IntPtr)_base, offsets);

            MemoryWatcher memoryWatcher = new MemoryWatcher<IntPtr>(deepPointer);
            memoryWatcher.Name = name;
            memoryWatcher.Current = new List<T>();

            ListWatchers.Add((typeof(T), memoryWatcher));
        }
        catch { }
    }
    #endregion
    #region WATCH_STRING
    public void WatchString(string name, (IntPtr _base, int[] offsets) offsets)
    {
        _WatchString(name, offsets._base, offsets: offsets.offsets);
    }

    public void WatchString(string name, object _base, params int[] offsets)
    {
        _WatchString(name, _base, offsets: offsets);
    }

    public void WatchString(string name, int length, object _base, params int[] offsets)
    {
        _WatchString(name, _base, length: length, offsets: offsets);
    }

    public void WatchString(string name, ReadStringType readStringType, object _base, params int[] offsets)
    {
        _WatchString(name, _base, offsets: offsets);
    }

    public void WatchString(string name, int length, ReadStringType readStringType, object _base, params int[] offsets)
    {
        _WatchString(name, _base, length, offsets: offsets);
    }

    public void WatchString(string name, string moduleName, object _base, params int[] offsets)
    {
        _WatchString(name, _base, moduleName: moduleName, offsets: offsets);
    }

    public void WatchString(string name, int length, string moduleName, object _base, params int[] offsets)
    {
        _WatchString(name, _base, length, moduleName: moduleName, offsets: offsets);
    }

    public void WatchString(string name, ReadStringType readStringType, string moduleName, object _base, params int[] offsets)
    {
        _WatchString(name, _base, readStringType: readStringType, moduleName: moduleName, offsets: offsets);
    }

    public void WatchString(string name, int length, ReadStringType readStringType, string moduleName, object _base, params int[] offsets)
    {
        _WatchString(name, _base, length, readStringType, moduleName, offsets);
    }

    public void WatchString(string name, Module module, object _base, params int[] offsets)
    {
        _WatchString(name, _base, moduleName: module.Name, offsets: offsets);
    }

    public void WatchString(string name, int length, Module module, object _base, params int[] offsets)
    {
        _WatchString(name, _base, length, moduleName: module.Name, offsets: offsets);
    }

    public void WatchString(string name, ReadStringType readStringType, Module module, object _base, params int[] offsets)
    {
        _WatchString(name, _base, readStringType :readStringType, moduleName: module.Name, offsets: offsets);
    }

    public void WatchString(string name, int length, ReadStringType readStringType, Module module, object _base, params int[] offsets)
    {
        _WatchString(name, _base, length, readStringType, module.Name, offsets);
    }

    public void WatchString(string name, object _base, int length = 128, ReadStringType readStringType = ReadStringType.AutoDetect,
        string moduleName = null, params int[] offsets)
    {
        _WatchString(name, _base, length, readStringType , moduleName, offsets);
    }

    private void _WatchString(string name, object _base, int length = 128, ReadStringType readStringType = ReadStringType.AutoDetect,
        string moduleName = null, params int[] offsets)
    {
        try
        {
            DeepPointer deepPointer;
            if (_base.GetType() == typeof(int))
            {
                if (string.IsNullOrEmpty(moduleName)) deepPointer = new DeepPointer((int)_base, offsets);
                else deepPointer = new DeepPointer(moduleName, (int)_base, offsets);
            }
            else deepPointer = new DeepPointer((IntPtr)_base, offsets);

            StringWatcher stringWatcher = new StringWatcher(deepPointer, 128);
            stringWatcher.Name = name;
            stringWatcher.Current = null;
            StringWatchers.Add(stringWatcher);

            current[name] = null;
        }
        catch { }
    }
    #endregion
    #region WATCH_UNITY_STRING
    public void WatchUnityString(string name, (IntPtr _base, int[] offsets) offsets)
    {
        _WatchUnityString(name, offsets._base, offsets: offsets.offsets);
    }

    public void WatchUnityString(string name, object _base, params int[] offsets)
    {
        _WatchUnityString(name, _base, offsets: offsets);
    }

    public void WatchUnityString(string name, int length, object _base, params int[] offsets)
    {
        _WatchUnityString(name, _base, length: length, offsets: offsets);
    }

    public void WatchUnityString(string name, ReadStringType readStringType, object _base, params int[] offsets)
    {
        _WatchUnityString(name, _base, offsets: offsets);
    }

    public void WatchUnityString(string name, int length, ReadStringType readStringType, object _base, params int[] offsets)
    {
        _WatchUnityString(name, _base, length, offsets: offsets);
    }

    public void WatchUnityString(string name, string moduleName, object _base, params int[] offsets)
    {
        _WatchUnityString(name, _base, moduleName: moduleName, offsets: offsets);
    }

    public void WatchUnityString(string name, int length, string moduleName, object _base, params int[] offsets)
    {
        _WatchUnityString(name, _base, length, moduleName: moduleName, offsets: offsets);
    }

    public void WatchUnityString(string name, ReadStringType readStringType, string moduleName, object _base, params int[] offsets)
    {
        _WatchUnityString(name, _base, readStringType: readStringType, moduleName: moduleName, offsets: offsets);
    }

    public void WatchUnityString(string name, int length, ReadStringType readStringType, string moduleName, object _base, params int[] offsets)
    {
        _WatchUnityString(name, _base, length, readStringType, moduleName, offsets);
    }

    public void WatchUnityString(string name, Module module, object _base, params int[] offsets)
    {
        _WatchUnityString(name, _base, moduleName: module.Name, offsets: offsets);
    }

    public void WatchUnityString(string name, int length, Module module, object _base, params int[] offsets)
    {
        _WatchUnityString(name, _base, length, moduleName: module.Name, offsets: offsets);
    }

    public void WatchUnityString(string name, ReadStringType readStringType, Module module, object _base, params int[] offsets)
    {
        _WatchUnityString(name, _base, readStringType: readStringType, moduleName: module.Name, offsets: offsets);
    }

    public void WatchUnityString(string name, int length, ReadStringType readStringType, Module module, object _base, params int[] offsets)
    {
        _WatchUnityString(name, _base, length, readStringType, module.Name, offsets);
    }

    public void WatchUnityString(string name, object _base, int length = 128, ReadStringType readStringType = ReadStringType.AutoDetect,
        string moduleName = null, params int[] offsets)
    {
        _WatchUnityString(name, _base, length, readStringType, moduleName, offsets);
    }

    private void _WatchUnityString(string name, object _base, int length = 128, ReadStringType readStringType = ReadStringType.UTF16,
        string moduleName = null, params int[] offsets)
    {
        try
        {
            List<int> _offsets = offsets.ToList();
            _offsets.Add(0x18);
            offsets = _offsets.ToArray();

            DeepPointer deepPointer;
            if (_base.GetType() == typeof(int))
            {
                if (string.IsNullOrEmpty(moduleName)) deepPointer = new DeepPointer((int)_base, offsets);
                else deepPointer = new DeepPointer(moduleName, (int)_base, offsets);
            }
            else deepPointer = new DeepPointer((IntPtr)_base, offsets);

            StringWatcher stringWatcher = new StringWatcher(deepPointer, 128);
            stringWatcher.Name = name;
            stringWatcher.Current = null;
            StringWatchers.Add(stringWatcher);

            current[name] = null;
        }
        catch { }
    }
    #endregion
}