using LiveSplit.ComponentUtil;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

public class PtrResolver
{
    #region WATCHERS_ACCESS
    public object this[string key]
    {
        get
        {
            try
            {
                MemoryWatcher watcher = Main.MemoryWatchers.FirstOrDefault(m => m.Name == key);
                if (watcher == null) watcher = Main.StringWatchers.FirstOrDefault(m => m.Name == key);
                return watcher;
            }
            catch { }
            return null;
        }
    }
    #endregion

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
            int addition = 0;
            if (offsets.Length > 0)
            {
                addition = offsets[offsets.Length - 1];
                List<int> modified = offsets.ToList();
                modified.RemoveAt(modified.Count - 1);
                offsets = modified.ToArray();
            }

            DeepPointer deepPointer;
            if (_base.GetType() == typeof(int))
            {
                if (string.IsNullOrEmpty(moduleName)) deepPointer = new DeepPointer((int)_base, offsets);
                else deepPointer = new DeepPointer(moduleName, (int)_base, offsets);
            }
            else if (_base.GetType() == typeof(IntPtr)) deepPointer = new DeepPointer((IntPtr)_base, offsets);
            else deepPointer = new DeepPointer((IntPtr)Convert.ToInt64(_base), offsets);

            IntPtr result = deepPointer.Deref<IntPtr>(Main.ProcessInstance);
            if (result != IntPtr.Zero) return result + addition;
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
            else if (_base.GetType() == typeof(IntPtr)) deepPointer = new DeepPointer((IntPtr)_base, offsets);
            else deepPointer = new DeepPointer((IntPtr)Convert.ToInt64(_base), offsets);

            return deepPointer.Deref<T>(Main.ProcessInstance);
        }
        catch { }
        return default(T);
    }
    #endregion
    #region TRY_READ
    public bool TryRead<T>(out T result, (IntPtr _base, int[] offsets) offsets) where T : unmanaged
    {
        return _TryRead<T>(out result, offsets._base, offsets: offsets.offsets);
    }

    public bool TryRead<T>(out T result, object _base, params int[] offsets) where T : unmanaged
    {
        return _TryRead<T>(out result, _base, offsets: offsets);
    }

    public bool TryRead<T>(out T result, string moduleName, object _base, params int[] offsets) where T : unmanaged
    {
        return _TryRead<T>(out result, _base, moduleName, offsets);
    }

    public bool TryRead<T>(out T result, Module module, object _base, params int[] offsets) where T : unmanaged
    {
        return _TryRead<T>(out result, _base, module.Name, offsets);
    }

    public bool TryRead<T>(out T result, object _base, string moduleName = null, params int[] offsets) where T : unmanaged
    {
        return _TryRead<T>(out result, _base, moduleName, offsets);
    }

    private bool _TryRead<T>(out T result, object _base, string moduleName = null, params int[] offsets) where T : unmanaged
    {
        try
        {
            DeepPointer deepPointer;
            if (_base is int)
            {
                if (string.IsNullOrEmpty(moduleName)) deepPointer = new DeepPointer((int)_base, offsets);
                else deepPointer = new DeepPointer(moduleName, (int)_base, offsets);
            }
            else if (_base.GetType() == typeof(IntPtr)) deepPointer = new DeepPointer((IntPtr)_base, offsets);
            else deepPointer = new DeepPointer((IntPtr)Convert.ToInt64(_base), offsets);


            T value;
            if (deepPointer.Deref<T>(Main.ProcessInstance, out value))
            {
                result = value;
                return true;
            }
        }
        catch { }
        result = default;
        return false;
    }
    #endregion
    #region READ_ARRAY
    public T[] ReadArray<T>((IntPtr _base, int[] offsets) offsets) where T : unmanaged
    {
        return _ReadArray<T>(typeof(T), offsets._base, offsets: offsets.offsets);
    }

    public T[] ReadArray<T>(object _base, params int[] offsets) where T : unmanaged
    {
        return _ReadArray<T>(typeof(T), _base, offsets: offsets);
    }

    public T[] ReadArray<T>(string moduleName, object _base, params int[] offsets) where T : unmanaged
    {
        return _ReadArray<T>(typeof(T), _base, moduleName, offsets);
    }

    public T[] ReadArray<T>(Module module, object _base, params int[] offsets) where T : unmanaged
    {
        return _ReadArray<T>(typeof(T), _base, module.Name, offsets);
    }

    public T[] ReadArray<T>(object _base, string moduleName = null, params int[] offsets) where T : unmanaged
    {
        return _ReadArray<T>(typeof(T), _base, moduleName, offsets);
    }

    private T[] _ReadArray<T>(Type type, object _base, string moduleName = null, params int[] offsets) where T : unmanaged
    {
        try
        {
            do
            {
                DeepPointer deepPointer;
                if (_base.GetType() == typeof(int))
                {
                    if (string.IsNullOrEmpty(moduleName)) deepPointer = new DeepPointer((int)_base, offsets);
                    else deepPointer = new DeepPointer(moduleName, (int)_base, offsets);
                }
                else if (_base.GetType() == typeof(IntPtr)) deepPointer = new DeepPointer((IntPtr)_base, offsets);
                else deepPointer = new DeepPointer((IntPtr)Convert.ToInt64(_base), offsets);

                // ---
                ulong listAddr = deepPointer.Deref<ulong>(Main.ProcessInstance);
                if (listAddr == 0) break;

                int itemSize = Marshal.SizeOf(type);
                int count = TMemory.ReadMemory<ushort>(Main.ProcessInstance, listAddr + 0x18);
                int size = count * itemSize;

                ulong listItemsAddr = listAddr;
                byte[] listBytes = TMemory.ReadMemoryBytes(Main.ProcessInstance, listItemsAddr + 0x20, size);
                if (listBytes == null || listBytes.Length == 0) break;

                // race safety check
                if (listAddr != deepPointer.Deref<ulong>(Main.ProcessInstance)) break;

                // ---
                List<T> list = new List<T>();
                for (int i = 0; i < size; i += itemSize)
                {
                    byte[] extract = TArray.Extract(listBytes, i, itemSize);
                    list.Add(BytesToType<T>(extract));
                }

                return list.ToArray();
            }
            while (false);
        }
        catch { }
        return new List<T>().ToArray();
    }
    #endregion
    #region READ_LIST
    public List<T> ReadList<T>((IntPtr _base, int[] offsets) offsets) where T : unmanaged
    {
        return _ReadList<T>(typeof(T), offsets._base, offsets: offsets.offsets);
    }

    public List<T> ReadList<T>(object _base, params int[] offsets) where T : unmanaged
    {
        return _ReadList<T>(typeof(T), _base, offsets: offsets);
    }

    public List<T> ReadList<T>(string moduleName, object _base, params int[] offsets) where T : unmanaged
    {
        return _ReadList<T>(typeof(T), _base, moduleName, offsets);
    }

    public List<T> ReadList<T>(Module module, object _base, params int[] offsets) where T : unmanaged
    {
        return _ReadList<T>(typeof(T), _base, module.Name, offsets);
    }

    public List<T> ReadList<T>(object _base, string moduleName = null, params int[] offsets) where T : unmanaged
    {
        return _ReadList<T>(typeof(T), _base, moduleName, offsets);
    }

    private List<T> _ReadList<T>(Type type, object _base, string moduleName = null, params int[] offsets) where T : unmanaged
    {
        try
        {
            do
            {
                DeepPointer deepPointer;
                if (_base.GetType() == typeof(int))
                {
                    if (string.IsNullOrEmpty(moduleName)) deepPointer = new DeepPointer((int)_base, offsets);
                    else deepPointer = new DeepPointer(moduleName, (int)_base, offsets);
                }
                else if (_base.GetType() == typeof(IntPtr)) deepPointer = new DeepPointer((IntPtr)_base, offsets);
                else deepPointer = new DeepPointer((IntPtr)Convert.ToInt64(_base), offsets);

                // ---
                ulong listAddr = deepPointer.Deref<ulong>(Main.ProcessInstance);
                if (listAddr == 0) break;

                int itemSize = Marshal.SizeOf(type);
                int count = TMemory.ReadMemory<ushort>(Main.ProcessInstance, listAddr + 0x18);
                int size = count * itemSize;

                ulong listItemsAddr = TMemory.ReadMemory<ulong>(Main.ProcessInstance, listAddr + 0x10);
                byte[] listBytes = TMemory.ReadMemoryBytes(Main.ProcessInstance, listItemsAddr + 0x20, size);
                if (listBytes == null || listBytes.Length == 0) break;

                // race safety check
                if (listAddr != deepPointer.Deref<ulong>(Main.ProcessInstance)) break;

                // ---
                List<T> list = new List<T>();
                for (int i = 0; i < size; i += itemSize)
                {
                    byte[] extract = TArray.Extract(listBytes, i, itemSize);
                    list.Add(BytesToType<T>(extract));
                }

                return list;
            }
            while (false);
        }
        catch { }
        return new List<T>();
    }
    #endregion
    #region READ_STRING
    public string ReadString(object _base, params int[] offsets)
    {
        return _ReadString(_base, offsets: offsets);
    }

    public string ReadString(int length, object _base, params int[] offsets)
    {
        return _ReadString(_base, length: length, offsets: offsets);
    }

    public string ReadString(ReadStringType readStringType, object _base, params int[] offsets)
    {
        return _ReadString(_base, offsets: offsets);
    }

    public string ReadString(int length, ReadStringType readStringType, object _base, params int[] offsets)
    {
        return _ReadString(_base, length, offsets: offsets);
    }

    public string ReadString(string moduleName, object _base, params int[] offsets)
    {
        return _ReadString(_base, moduleName: moduleName, offsets: offsets);
    }

    public string ReadString(int length, string moduleName, object _base, params int[] offsets)
    {
        return _ReadString(_base, length, moduleName: moduleName, offsets: offsets);
    }

    public string ReadString(ReadStringType readStringType, string moduleName, object _base, params int[] offsets)
    {
        return _ReadString(_base, readStringType: readStringType, moduleName: moduleName, offsets: offsets);
    }

    public string ReadString(int length, ReadStringType readStringType, string moduleName, object _base, params int[] offsets)
    {
        return _ReadString(_base, length, readStringType, moduleName, offsets);
    }

    public string ReadString(Module module, object _base, params int[] offsets)
    {
        return _ReadString(_base, moduleName: module.Name, offsets: offsets);
    }

    public string ReadString(int length, Module module, object _base, params int[] offsets)
    {
        return _ReadString(_base, length, moduleName: module.Name, offsets: offsets);
    }
    string ReadString(ReadStringType readStringType, Module module, object _base, params int[] offsets)
    {
        return _ReadString(_base, readStringType: readStringType, moduleName: module.Name, offsets: offsets);
    }

    public string ReadString(int length, ReadStringType readStringType, Module module, object _base, params int[] offsets)
    {
        return _ReadString(_base, length, readStringType, module.Name, offsets);
    }

    public string ReadString(object _base, int length = 128, ReadStringType readStringType = ReadStringType.AutoDetect,
        string moduleName = null, params int[] offsets)
    {
        return _ReadString(_base, length, readStringType, moduleName, offsets);
    }

    private string _ReadString(object _base, int length = 128, ReadStringType readStringType = ReadStringType.AutoDetect,
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
            else if (_base.GetType() == typeof(IntPtr)) deepPointer = new DeepPointer((IntPtr)_base, offsets);
            else deepPointer = new DeepPointer((IntPtr)Convert.ToInt64(_base), offsets);

            return deepPointer.DerefString(Main.ProcessInstance, readStringType, length, null);
        }
        catch { }
        return null;
    }
    #endregion
    #region READ_BYTES
    public byte[] ReadBytes((IntPtr _base, int[] offsets) offsets, int size)
    {
        return _ReadBytes(offsets._base, size, offsets: offsets.offsets);
    }

    public byte[] ReadBytes(object _base, int size, params int[] offsets)
    {
        return _ReadBytes(_base, size, offsets: offsets);
    }

    public byte[] ReadBytes(string moduleName, object _base, int size, params int[] offsets)
    {
        return _ReadBytes(_base, size, moduleName, offsets);
    }

    public byte[] ReadBytes(Module module, object _base, int size, params int[] offsets)
    {
        return _ReadBytes(_base, size, module.Name, offsets);
    }

    private byte[] _ReadBytes(object _base, int size, string moduleName = null, params int[] offsets)
    {
        try
        {
            DeepPointer deepPointer;
            if (_base is int)
            {
                if (string.IsNullOrEmpty(moduleName)) deepPointer = new DeepPointer((int)_base, offsets);
                else deepPointer = new DeepPointer(moduleName, (int)_base, offsets);
            }
            else if (_base.GetType() == typeof(IntPtr)) deepPointer = new DeepPointer((IntPtr)_base, offsets);
            else deepPointer = new DeepPointer((IntPtr)Convert.ToInt64(_base), offsets);

            return deepPointer.DerefBytes(Main.ProcessInstance, size);
        }
        catch { }
        return null;
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
            if (!string.IsNullOrEmpty(name))
            {
                var oldWatcher = Main.MemoryWatchers.FirstOrDefault(m => m.Name == name);
                if (oldWatcher != null) Main.MemoryWatchers.Remove(oldWatcher);
            }

            DeepPointer deepPointer;
            if (_base.GetType() == typeof(int))
            {
                if (string.IsNullOrEmpty(moduleName)) deepPointer = new DeepPointer((int)_base, offsets);
                else deepPointer = new DeepPointer(moduleName, (int)_base, offsets);
            }
            else if (_base.GetType() == typeof(IntPtr)) deepPointer = new DeepPointer((IntPtr)_base, offsets);
            else deepPointer = new DeepPointer((IntPtr)Convert.ToInt64(_base), offsets);

            MemoryWatcher memoryWatcher = new MemoryWatcher<T>(deepPointer);
            memoryWatcher.Name = name;
            memoryWatcher.Current = default(T);
            Main.MemoryWatchers.Add(memoryWatcher);
            Main.current[name] = default(T);
        }
        catch { }
    }
    #endregion
    #region WATCH_ARRAY
    public void WatchArray<T>(string name, object _base, params int[] offsets) where T : unmanaged
    {
        _WatchArray<T>(name, _base, offsets: offsets);
    }

    public void WatchArray<T>(string name, string moduleName, object _base, params int[] offsets) where T : unmanaged
    {
        _WatchArray<T>(name, _base, moduleName, offsets);
    }

    public void WatchArray<T>(string name, Module module, object _base, params int[] offsets) where T : unmanaged
    {
        _WatchArray<T>(name, _base, module.Name, offsets);
    }

    public void WatchArray<T>(string name, object _base, string moduleName = null, params int[] offsets) where T : unmanaged
    {
        _WatchArray<T>(name, _base, moduleName, offsets);
    }

    private void _WatchArray<T>(string name, object _base, string moduleName = null, params int[] offsets) where T : unmanaged
    {
        try
        {
            if (!string.IsNullOrEmpty(name))
            {
                var oldWatcher = Main.MemoryWatchers.FirstOrDefault(m => m.Name == name);
                if (oldWatcher != null) Main.MemoryWatchers.Remove(oldWatcher);
            }

            DeepPointer deepPointer;
            if (_base.GetType() == typeof(int))
            {
                if (string.IsNullOrEmpty(moduleName)) deepPointer = new DeepPointer((int)_base, offsets);
                else deepPointer = new DeepPointer(moduleName, (int)_base, offsets);
            }
            else if (_base.GetType() == typeof(IntPtr)) deepPointer = new DeepPointer((IntPtr)_base, offsets);
            else deepPointer = new DeepPointer((IntPtr)Convert.ToInt64(_base), offsets);

            MemoryWatcher memoryWatcher = new MemoryWatcher<IntPtr>(deepPointer);
            memoryWatcher.Name = name;
            memoryWatcher.Current = IntPtr.Zero;
            Main.CountableWatchers.Add((Main.CountableStyle.Array, typeof(T), memoryWatcher, deepPointer));
            Main.current[name] = new List<T>().ToArray();
        }
        catch { }
    }
    #endregion
    #region WATCH_LIST
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
            if (!string.IsNullOrEmpty(name))
            {
                var oldWatcher = Main.MemoryWatchers.FirstOrDefault(m => m.Name == name);
                if (oldWatcher != null) Main.MemoryWatchers.Remove(oldWatcher);
            }

            DeepPointer deepPointer;
            if (_base.GetType() == typeof(int))
            {
                if (string.IsNullOrEmpty(moduleName)) deepPointer = new DeepPointer((int)_base, offsets);
                else deepPointer = new DeepPointer(moduleName, (int)_base, offsets);
            }
            else if (_base.GetType() == typeof(IntPtr)) deepPointer = new DeepPointer((IntPtr)_base, offsets);
            else deepPointer = new DeepPointer((IntPtr)Convert.ToInt64(_base), offsets);

            MemoryWatcher memoryWatcher = new MemoryWatcher<IntPtr>(deepPointer);
            memoryWatcher.Name = name;
            memoryWatcher.Current = IntPtr.Zero;
            Main.CountableWatchers.Add((Main.CountableStyle.List, typeof(T), memoryWatcher, deepPointer));
            Main.current[name] = new List<T>();
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
        _WatchString(name, _base, length, readStringType, moduleName, offsets);
    }

    private void _WatchString(string name, object _base, int length = 128, ReadStringType readStringType = ReadStringType.AutoDetect,
        string moduleName = null, params int[] offsets)
    {
        try
        {
            if (!string.IsNullOrEmpty(name))
            {
                var oldWatcher = Main.MemoryWatchers.FirstOrDefault(m => m.Name == name);
                if (oldWatcher != null) Main.MemoryWatchers.Remove(oldWatcher);
            }

            DeepPointer deepPointer;
            if (_base.GetType() == typeof(int))
            {
                if (string.IsNullOrEmpty(moduleName)) deepPointer = new DeepPointer((int)_base, offsets);
                else deepPointer = new DeepPointer(moduleName, (int)_base, offsets);
            }
            else if (_base.GetType() == typeof(IntPtr)) deepPointer = new DeepPointer((IntPtr)_base, offsets);
            else deepPointer = new DeepPointer((IntPtr)Convert.ToInt64(_base), offsets);

            StringWatcher stringWatcher = new StringWatcher(deepPointer, readStringType, length);
            stringWatcher.Name = name;
            stringWatcher.Current = null;
            Main.StringWatchers.Add(stringWatcher);
            Main.current[name] = null;
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
            if (!string.IsNullOrEmpty(name))
            {
                var oldWatcher = Main.MemoryWatchers.FirstOrDefault(m => m.Name == name);
                if (oldWatcher != null) Main.MemoryWatchers.Remove(oldWatcher);
            }

            List<int> _offsets = offsets.ToList();
            _offsets.Add(0x14);
            offsets = _offsets.ToArray();

            DeepPointer deepPointer;
            if (_base.GetType() == typeof(int))
            {
                if (string.IsNullOrEmpty(moduleName)) deepPointer = new DeepPointer((int)_base, offsets);
                else deepPointer = new DeepPointer(moduleName, (int)_base, offsets);
            }
            else if (_base.GetType() == typeof(IntPtr)) deepPointer = new DeepPointer((IntPtr)_base, offsets);
            else deepPointer = new DeepPointer((IntPtr)Convert.ToInt64(_base), offsets);

            StringWatcher stringWatcher = new StringWatcher(deepPointer, 128);
            stringWatcher.Name = name;
            stringWatcher.Current = null;
            Main.StringWatchers.Add(stringWatcher);
            Main.current[name] = null;
        }
        catch { }
    }
    #endregion

    #region UTILITIES
    public bool CheckFlag(string watcherName)
    {
        try
        {
            do
            {
                ulong curr = Convert.ToUInt64(Main.current[watcherName]);
                ulong ol = Convert.ToUInt64(Main.old[watcherName]);
                return curr != ol && curr != 0;
            }
            while (false);
        }
        catch { }
        return false;
    }

    public T BytesToType<T>(byte[] bytes) where T : unmanaged
    {
        Type type = typeof(T);
        if (type == typeof(IntPtr)) return (T)(object)new IntPtr(BitConverter.ToInt64(bytes, 0));
        else if (type == typeof(UIntPtr)) return (T)(object)new UIntPtr(BitConverter.ToUInt64(bytes, 0));
        else if (type == typeof(bool)) return (T)(object)BitConverter.ToBoolean(bytes, 0);
        else if (type == typeof(byte)) return (T)(object)bytes[0];
        else if (type == typeof(sbyte)) return (T)(object)(sbyte)bytes[0];
        else if (type == typeof(char)) return (T)(object)BitConverter.ToChar(bytes, 0);
        else if (type == typeof(short)) return (T)(object)BitConverter.ToInt16(bytes, 0);
        else if (type == typeof(ushort)) return (T)(object)BitConverter.ToUInt16(bytes, 0);
        else if (type == typeof(int)) return (T)(object)BitConverter.ToInt32(bytes, 0);
        else if (type == typeof(uint)) return (T)(object)BitConverter.ToUInt32(bytes, 0);
        else if (type == typeof(long)) return (T)(object)BitConverter.ToInt64(bytes, 0);
        else if (type == typeof(ulong)) return (T)(object)BitConverter.ToUInt64(bytes, 0);
        else if (type == typeof(float)) return (T)(object)BitConverter.ToSingle(bytes, 0);
        else if (type == typeof(double)) return (T)(object)BitConverter.ToDouble(bytes, 0);
        else if (type == typeof(decimal)) return (T)(object)TUtils.ToDecimal(bytes, 0);
        return default(T);
    }
    #endregion
}