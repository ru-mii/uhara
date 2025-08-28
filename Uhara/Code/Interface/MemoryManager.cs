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
            new Thread(() => { _FreeMemoryDelayed(address, size); }).Start();
        }
        catch { }
    }

    private static void _FreeMemoryDelayed(ulong address, int size)
    {
        try
        {
            if (!TProcess.IsAlive(Instance)) return;
            string tempToken = TProcess.GetToken(Instance);
            for (int i = 0; i < 150; i++) Thread.Sleep(100);
            if (!TProcess.IsAlive(Instance)) return;
            if (tempToken != TProcess.GetToken(Instance)) return;
            TMemory.FreeMemory(Instance, address, size);
            TProgram.Print("Deallocated memory at 0x" + address.ToString("X"));
        }
        catch { }
    }

    internal static void ClearMemory()
    {
        try
        {
            string token = TProcess.GetToken(Instance);
            string[] keys = TSaves2.GetKeyNames(RegistryName);

            foreach (string key in keys)
            {
                if (!key.StartsWith(token)) TSaves2.DeleteKey(RegistryName, key);
                else if (!key.EndsWith(UniqueScriptLoadID))
                {
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
                            TProgram.Print("Deallocation scheduled for 0x" + address.ToString("X"));
                        }
                    }

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
                            RefWriteBytes(Instance, address, recover);
                            TProgram.Print(recover.Length + " bytes recovered at 0x" + address.ToString("X"));
                        }
                    }
                }
            }
        }
        catch { }
    }

    internal static ulong AllocateSafe(int size)
    {
        try
        {
            ulong address = RefAllocateMemory(Instance, size);
            if (address != 0)
            {
                string token = TProcess.GetToken(Instance);
                string tokenPlus = token + UniqueScriptLoadID;
                TSaves2.Set("0x" + size.ToString("X"), RegistryName, tokenPlus,
                    Allocate, "0x" + address.ToString("X"));

                return address;
            }
        }
        catch { }
        return 0;
    }

    internal static void AddOverwrite(ulong address, byte[] recover)
    {
        try
        {
            if (address == 0 || recover == null)
                return;

            string data = TMemory.GetSignature(recover, true);

            string token = TProcess.GetToken(Instance);
            string tokenPlus = token + UniqueScriptLoadID;

            if (TSaves2.Get(RegistryName, tokenPlus, Overwrite, "0x" + address.ToString("X")) == null)
                TSaves2.Set(data, RegistryName, tokenPlus, Overwrite, "0x" + address.ToString("X"));
        }
        catch { }
    }
}