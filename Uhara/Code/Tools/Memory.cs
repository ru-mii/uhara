using SharpDisasm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

internal class UMemory : UShared
{
    public static void FixRelative(Process process, ulong original, ulong current, int size)
    {
        byte[] originalBytes = ReadMemoryBytes(process, original, size);
        byte[] currentBytes = ReadMemoryBytes(process, current, size);

        Instruction[] _org = UInstruction.GetInstructions2(originalBytes);
        Instruction[] _cur = UInstruction.GetInstructions2(originalBytes);

        int total = 0;
        for (int i = 0; i < _org.Length; i++)
        {
            if (total != 0) total += _org[i - 1].Bytes.Length;

            string orgTxt = _org[i].ToString();
            string curTxt = _cur[i].ToString();

            byte[] orgFullBytes = _org[i].Bytes;
            byte[] curFullBytes = _cur[i].Bytes;

            if (orgTxt != curTxt) continue;
            if (!orgFullBytes.SequenceEqual(curFullBytes)) continue;

            if (orgTxt.Contains("rip"))
            {
                if (!orgTxt.Contains("]")) continue;

                string ripValueTxt = orgTxt.Remove(orgTxt.IndexOf("]"));
                ripValueTxt = ripValueTxt.Substring(ripValueTxt.IndexOf("rip") + 3);
                uint ripValue = UConvert.Parse<uint>(ripValueTxt);

                if (ripValue == 0) continue;

                byte[] ripValueBytes = BitConverter.GetBytes(ripValue);
                int byteOffset = FindInArray(curFullBytes, ripValueBytes);

                byte[] newRipValueBytes = BitConverter.GetBytes(ripValue - (uint)(current - original));

                RefWriteBytes(process, current + (ulong)(byteOffset + total), newRipValueBytes);
            }
            else if (orgTxt.StartsWith("call"))
            {
                if (orgFullBytes.Length != 5) continue;
                if (orgTxt.StartsWith("call e") || orgTxt.StartsWith("call r")) continue;

                uint ripValue = BitConverter.ToUInt32(orgFullBytes, 1);
                byte[] newRipValueBytes = BitConverter.GetBytes(ripValue - (uint)(current - original));
                RefWriteBytes(process, current + 1 + (ulong)total, newRipValueBytes);
            }
        }
    }

    internal static void CreateAbsoluteJump(Process process, ulong source, ulong destination)
    {
        byte[] stub = new byte[] { 0xFF, 0x25, 0x00, 0x00, 0x00, 0x00 };
        byte[] full = UArray.Merge(stub, BitConverter.GetBytes(destination));
        RefWriteBytes(process, source, full);
    }

    internal static byte[] GetByteArray(string signature)
    {
        signature = signature.Replace(" ", "").Replace("!", "");
        byte[] byteArray = new byte[signature.Length / 2];

        for (int i = 0; i < byteArray.Length; i++)
        {
            string byteValue = signature.Substring(i * 2, 2);

            if (byteValue != "??")
            {
                if (!IsCharHex(byteValue[0]) || !IsCharHex(byteValue[1])) return null;
                else byteArray[i] = Convert.ToByte(byteValue, 16);
            }
            else byteArray[i] = 0x00;
        }

        return byteArray;
    }

    private static bool IsCharHex(char character)
    {
        if (!((character >= '0' && character <= '9') ||
        (character >= 'A' && character <= 'F') ||
        (character >= 'a' && character <= 'f')))
        {
            return false;
        }
        return true;
    }

    internal static ulong ScanSingle(string signature)
    {
        byte[] searchBytes = USignature.GetBytes(signature);
        string searchMask = USignature.GetMask(signature);

        ulong baseAddress = (ulong)Instance.MainModule.BaseAddress;
        byte[] peHeader = ReadMemoryBytes(Instance, baseAddress, 0x1000);
        List<ulong[]> readSections = GetReadableSections(peHeader);

        foreach (ulong[] section in readSections)
        {
            byte[] sectionBytes = ReadMemoryBytes(Instance, section[0], (int)section[1]);
            if (sectionBytes != null && sectionBytes.Length > 0)
            {
                int searchOffset = FindInArray(sectionBytes, searchBytes, searchMask);
                if (searchOffset != -1)
                {
                    return section[0] + (ulong)searchOffset;
                }
            }
        }

        return 0;
    }

    internal static ulong ScanRel(int offset, string signature)
    {
        byte[] searchBytes = USignature.GetBytes(signature);
        string searchMask = USignature.GetMask(signature);

        ulong baseAddress = (ulong)Instance.MainModule.BaseAddress;
        byte[] peHeader = ReadMemoryBytes(Instance, baseAddress, 0x1000);
        List<ulong[]> readSections = GetReadableSections(peHeader);

        foreach (ulong[] section in readSections)
        {
            byte[] sectionBytes = ReadMemoryBytes(Instance, section[0], (int)section[1]);
            if (sectionBytes != null && sectionBytes.Length > 0)
            {
                int searchOffset = FindInArray(sectionBytes, searchBytes, searchMask);
                if (searchOffset != -1)
                {
                    ulong searchAddress = section[0] + (ulong)searchOffset;
                    ulong relativeAddress = searchAddress + (ulong)offset;
                    long relativeValue = ReadMemory<int>(Instance, relativeAddress);
                    ulong destinationAddress = (ulong)((long)searchAddress + relativeValue + offset + 4);
                    return destinationAddress;
                }
            }
        }

        return 0;
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

    internal static List<ulong[]> GetReadableSections(byte[] peHeader)
    {
        List<ulong[]> readableSections = new List<ulong[]>();
        ulong baseAddress = (ulong)Instance.MainModule.BaseAddress;

        int peHeaderOffset = BitConverter.ToInt32(peHeader, 0x3C);
        int sectionCount = BitConverter.ToInt16(peHeader, peHeaderOffset + 0x6);
        int optionalHeaderSize = BitConverter.ToInt16(peHeader, peHeaderOffset + 0x14);
        int sectionHeaderOffset = peHeaderOffset + 0x18 + optionalHeaderSize;

        int startSectionOffset = sectionHeaderOffset;

        for (int i = 0; i < sectionCount; i++)
        {
            int extractSectionOffset = 0;
            for (int j = startSectionOffset; j < peHeader.Length; j++)
            {
                if (peHeader[j] == 0)
                {
                    if ((j - startSectionOffset) % 4 == 0)
                    {
                        extractSectionOffset = j;
                        break;
                    }
                }
            }
            startSectionOffset += 0x28;

            uint virtualSize = BitConverter.ToUInt32(peHeader, extractSectionOffset);
            uint virtualAddress = BitConverter.ToUInt32(peHeader, extractSectionOffset + 0x04);

            ulong[] sectionAddresses = new ulong[2];
            sectionAddresses[0] = baseAddress + virtualAddress;
            sectionAddresses[1] = virtualSize;

            readableSections.Add(sectionAddresses);
        }

        return readableSections;
    }
}
