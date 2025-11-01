using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

internal class MemoryManager : MainShared
{
    private static readonly string RegistryName = "MemoryManager";
    private static readonly string Allocate = "Allocate";
    private static readonly string Overwrite = "Overwrite";

    private static void FreeMemoryDelayed(ulong address, int size)
    {
        try
        {
            Thread newThread = new Thread(() => { _FreeMemoryDelayed(address, size); });
            newThread.IsBackground = true;
            newThread.Start();
        }
        catch { }
    }

    private static void _FreeMemoryDelayed(ulong address, int size)
    {
        try
        {
            if (!TProcess.IsAlive(ProcessInstance)) return;
            string tempToken = TProcess.GetToken(ProcessInstance);
            for (int i = 0; i < 600; i++) Thread.Sleep(100); // 1 minute
            if (!TProcess.IsAlive(ProcessInstance)) return;
            if (tempToken != TProcess.GetToken(ProcessInstance)) return;
            TMemory.FreeMemory(ProcessInstance, address, size);
            //TUtils.Print("Deallocated memory at 0x" + address.ToString("X"));
        }
        catch { }
    }

    internal static ulong AllocateTimeLimited(int size, int time)
    {
        try
        {
            do
            {
                ulong allocated = RefAllocateMemory(ProcessInstance, size);
                if (allocated == 0) break;

                FreeLargeMemoryInternal(allocated, size, time);
                return allocated;
            }
            while (false);
        }
        catch { }
        return 0;
    }

    internal static void FreeLargeMemoryInternal(ulong address, int size, int delay = 60000)
    {
        try
        {
            do
            {
                ulong _Sleep = TProcess.GetProcAddress(ProcessInstance, "kernel32.dll", "Sleep");
                if (_Sleep == 0) break;

                ulong _VirtualFree = TProcess.GetProcAddress(ProcessInstance, "kernel32.dll", "VirtualFree");
                if (_VirtualFree == 0) break;

                ulong allocated = RefAllocateMemory(ProcessInstance, 0x1000);
                if (allocated == 0) break;

                byte[] bytesExec = new byte[]
                {
                   0x48, 0x83, 0xEC, 0x28, 0x48, 0x8B, 0x0D, 0xCD, 0xFF, 0xFF, 0xFF, 0x48, 0x8B, 0x05, 0xCE, 0xFF,
                   0xFF, 0xFF, 0xFF, 0xD0, 0x48, 0x8B, 0x0D, 0xCD, 0xFF, 0xFF, 0xFF, 0x48, 0x8B, 0x15, 0xCE, 0xFF,
                   0xFF, 0xFF, 0x49, 0xB8, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x48, 0x8B, 0x05, 0xC5,
                   0xFF, 0xFF, 0xFF, 0xFF, 0xD0, 0x48, 0x83, 0xC4, 0x28, 0xC3
                };

                byte[] writeBytes = TArray.Merge(
                    BitConverter.GetBytes((ulong)delay),
                    BitConverter.GetBytes(_Sleep),
                    BitConverter.GetBytes(address),
                    BitConverter.GetBytes((ulong)size),
                    BitConverter.GetBytes(_VirtualFree),
                    bytesExec);

                RefWriteBytes(ProcessInstance, allocated, writeBytes);
                RefCreateThread(ProcessInstance, allocated + 0x28);
            }
            while (false);
        }
        catch { }
    }

    internal static void ClearMemory(string uniqueId = "")
    {
        try
        {
            string token = TProcess.GetToken(ProcessInstance);
            string[] keys = TSaves2.GetKeyNames(RegistryName);

            foreach (string key in keys)
            {
                if (!key.StartsWith(token)) TSaves2.DeleteKey(RegistryName, key);
                else if (!key.Contains(UniqueScriptLoadID) || (!string.IsNullOrEmpty(uniqueId) && key.EndsWith(uniqueId)))
                {
                    // recover
                    {
                        string[] valueNames = TSaves2.GetValueNames(RegistryName, key, Overwrite);
                        foreach (string valueName in valueNames)
                        {
                            string dataRaw = TSaves2.Get(RegistryName, key, Overwrite, valueName);
                            if (dataRaw == null) continue;

                            ulong address = TConvert.Parse<ulong>(valueName);
                            byte[] recover = TSignature.GetBytes(dataRaw);

                            if (address == 0 || recover == null) continue;

                            TSaves2.DeleteValue(RegistryName, key, Overwrite, valueName);
                            RefWriteBytes(ProcessInstance, address, recover);
                            //TUtils.Print(recover.Length + " bytes recovered at 0x" + address.ToString("X"));
                        }
                    }

                    // deallocate
                    {
                        string[] valueNames = TSaves2.GetValueNames(RegistryName, key, Allocate);
                        foreach (string valueName in valueNames)
                        {
                            string dataRaw = TSaves2.Get(RegistryName, key, Allocate, valueName);
                            if (dataRaw == null) continue;

                            ulong address = TConvert.Parse<ulong>(valueName);
                            int size = TConvert.Parse<int>(dataRaw);

                            if (address == 0 || size == 0) continue;

                            TSaves2.DeleteValue(RegistryName, key, Allocate, valueName);
                            FreeMemoryDelayed(address, size);
                            //TUtils.Print("Deallocation scheduled for 0x" + address.ToString("X"));
                        }
                    }
                }
            }
        }
        catch { }
    }

    internal static ulong AllocateSafe(int size, string uniqueId = "")
    {
        try
        {
            ulong address = RefAllocateMemory(ProcessInstance, size);
            if (address != 0)
            {
                string token = TProcess.GetToken(ProcessInstance);
                string tokenPlus = token + UniqueScriptLoadID + uniqueId;
                TSaves2.Set("0x" + size.ToString("X"), RegistryName, tokenPlus,
                    Allocate, "0x" + address.ToString("X"));

                return address;
            }
        }
        catch { }
        return 0;
    }

    internal static void AddOverwrite(ulong address, byte[] recover, string uniqueId = "")
    {
        try
        {
            if (address == 0 || recover == null)
                return;

            string data = TMemory.GetSignature(recover, true);

            string token = TProcess.GetToken(ProcessInstance);
            string tokenPlus = token + UniqueScriptLoadID + uniqueId;

            if (TSaves2.Get(RegistryName, tokenPlus, Overwrite, "0x" + address.ToString("X")) == null)
                TSaves2.Set(data, RegistryName, tokenPlus, Overwrite, "0x" + address.ToString("X"));
        }
        catch { }
    }
}