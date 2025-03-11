using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

internal class UMemory
{
    internal static ulong ScanSingle(Process process, string signature)
    {
        byte[] searchBytes = USignature.GetBytes(signature);
        string searchMask = USignature.GetMask(signature);

        ulong baseAddress = (ulong)process.MainModule.BaseAddress;
        List<ulong[]> readSections = GetPESections(process);

        foreach (ulong[] section in readSections)
        {
            byte[] sectionBytes = ReadMemoryBytes(process, section[0], (int)section[1]);
            if (sectionBytes != null && sectionBytes.Length > 0)
            {
                int searchOffset = FindInArray(sectionBytes, searchBytes, searchMask);
                if (searchOffset != -1)
                {
                    return baseAddress + (ulong)searchOffset;
                }
            }
        }

        return 0;
    }

    internal static ulong ScanRel(Process process, int offset, string signature)
    {
        byte[] searchBytes = USignature.GetBytes(signature);
        string searchMask = USignature.GetMask(signature);

        ulong baseAddress = (ulong)process.MainModule.BaseAddress;
        List<ulong[]> readSections = GetPESections(process);

        foreach (ulong[] section in readSections)
        {
            byte[] sectionBytes = ReadMemoryBytes(process, section[0], (int)section[1]);
            if (sectionBytes != null && sectionBytes.Length > 0)
            {
                int searchOffset = FindInArray(sectionBytes, searchBytes, searchMask);
                if (searchOffset != -1)
                {
                    ulong searchAddress = section[0] + (ulong)searchOffset;
                    ulong relativeAddress = searchAddress + (ulong)offset;
                    long relativeValue = ReadMemory<int>(process, relativeAddress);
                    ulong destinationAddress = (ulong)((long)searchAddress + relativeValue + offset + 4);
                    return destinationAddress;
                }
            }
        }

        return 0;
    }

    internal static List<ulong[]> GetPESections(Process process)
    {
        List<ulong[]> sections = new List<ulong[]>();
        ulong baseAddress = (ulong)process.MainModule.BaseAddress;
        byte[] peHeader = ReadMemoryBytes(process, baseAddress, 0x1000);
        int peHeaderOffset = BitConverter.ToInt32(peHeader, 0x3C);
        int sectionCount = BitConverter.ToInt16(peHeader, peHeaderOffset + 0x6);
        int optionalHeaderSize = BitConverter.ToInt16(peHeader, peHeaderOffset + 0x14);
        int sectionHeaderOffset = peHeaderOffset + 0x18 + optionalHeaderSize;
        int newPeHeaderSize = sectionHeaderOffset + (0x28 * sectionCount);
        int extractSectionOffset = sectionHeaderOffset + 0x8;
        peHeader = ReadMemoryBytes(process, baseAddress, newPeHeaderSize);

        for (int i = 0; i < sectionCount; i++)
        {
            uint virtualSize = BitConverter.ToUInt32(peHeader, extractSectionOffset);
            uint virtualAddress = BitConverter.ToUInt32(peHeader, extractSectionOffset + 0x04);

            ulong[] sectionAddresses = new ulong[2];
            sectionAddresses[0] = baseAddress + virtualAddress;
            sectionAddresses[1] = virtualSize;

            extractSectionOffset += 0x28;
            sections.Add(sectionAddresses);
        }

        return sections;
    }

    internal static int FindInArray(byte[] chunkData, byte[] byteSignature, string mask = "", int startPosition = 0)
    {
        int position = startPosition;
        bool maskOn = mask.Contains("?") || mask.Contains("!");
        int found = 0; bool flag = false;

        while (position < chunkData.Length)
        {
            flag = false;
            if (maskOn)
            {
                if ((mask[found] == 'x' && chunkData[position] == byteSignature[found]) ||
                (mask[found] == '?') ||
                (mask[found] == '!' && chunkData[position] != byteSignature[found]))
                {
                    flag = true;
                }
            }
            else if (chunkData[position] == byteSignature[found]) flag = true;

            if (flag == true)
            {
                found += 1;
                if (found == byteSignature.Length) return position - found + 1;

            }
            else
            {
                position -= found - 1;
                found = 0;
            }

            if (flag) position += 1;
        }
        return -1;
    }

    internal static T ReadMemory<T>(Process process, ulong address) where T : unmanaged
    {
        return ReadMemory<T>(process, (IntPtr)address);
    }

    internal static T ReadMemory<T>(Process process, IntPtr address) where T : unmanaged
    {
        int typeSize = Marshal.SizeOf(typeof(T));
        byte[] data = new byte[typeSize];

        if (UImports.ReadProcessMemory(process.Handle, address, data, data.Length, out _))
        {
            if (typeof(T) == typeof(byte)) return (T)(object)data[0];
            else if (typeof(T) == typeof(short)) return (T)(object)BitConverter.ToInt16(data, 0);
            else if (typeof(T) == typeof(ushort)) return (T)(object)BitConverter.ToUInt16(data, 0);
            else if (typeof(T) == typeof(int)) return (T)(object)BitConverter.ToInt32(data, 0);
            else if (typeof(T) == typeof(uint)) return (T)(object)BitConverter.ToUInt32(data, 0);
            else if (typeof(T) == typeof(long)) return (T)(object)BitConverter.ToInt64(data, 0);
            else if (typeof(T) == typeof(ulong)) return (T)(object)BitConverter.ToUInt64(data, 0);
            else if (typeof(T) == typeof(double)) return (T)(object)BitConverter.ToDouble(data, 0);
            else if (typeof(T) == typeof(float)) return (T)(object)BitConverter.ToSingle(data, 0);
        }
        return default(T);
    }

    internal static byte[] ReadMemoryBytes(Process process, ulong address, int size)
    {
        return ReadMemoryBytes(process, (IntPtr)address, size);
    }

    internal static byte[] ReadMemoryBytes(Process process, IntPtr address, int size)
    {
        byte[] data = new byte[size];
        if (UImports.ReadProcessMemory(process.Handle, address, data, data.Length, out _))
            return data;
        return null;
    }
}
