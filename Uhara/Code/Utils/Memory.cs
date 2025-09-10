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
using static TImports;

internal class TMemory : MainShared
{
    public static ulong[] ScanMultiple(Process process, string signature, string moduleName = null, int memoryProtection = -1)
    {
        List<ulong> resultsRaw = new List<ulong>();
        byte[] searchBytes = TSignature.GetBytes(signature);
        string searchMask = TSignature.GetMask(signature);

        List<ulong[]> sections = GetAllSections(process, moduleName);

        for (int i = 0; i < sections.Count; i++)
        {
            byte[] sectionBytes = ReadMemoryBytes(process, sections[i][0], (int)sections[i][1]);
            if (sectionBytes != null && sectionBytes.Length > 0)
            {
                int searchOffset = 0;
                while (true)
                {
                    searchOffset = FindInArray(sectionBytes, searchBytes, searchMask, searchOffset);
                    if (searchOffset == -1) break;

                    ulong foundAddress = sections[i][0] + (ulong)searchOffset;

                    if (memoryProtection == -1) resultsRaw.Add(foundAddress);
                    else if (GetMemoryProtection(process, foundAddress) == memoryProtection)
                        resultsRaw.Add(foundAddress);

                    searchOffset += searchBytes.Length;
                    if ((ulong)searchOffset >= sections[i][1]) break;
                }
            }
        }

        return resultsRaw.ToArray();
    }

    public static int GetMemoryProtection(Process process, ulong address, int size = 0x1000)
    {
        MBI mBi = new MBI();
        if (!VirtualQueryEx(process.Handle, (IntPtr)address, out mBi, (uint)size)) return -1;
        return (int)mBi.Protect;
    }

    internal static int GetMinimumOverwriteBackwards(Process process, ulong address, int overwrite)
    {
        ulong hookAddress = address - 0x1000;
        byte[] pageBytes = ReadMemoryBytes(process, hookAddress, 0x1000);
        Instruction[] instructions = TInstruction.GetInstructions2(pageBytes);

        List<int> insLengths = new List<int>();

        foreach (Instruction ins in instructions)
        {
            hookAddress += (ulong)ins.Bytes.Length;
            insLengths.Add(ins.Bytes.Length);

            if (hookAddress == address) break;
        }

        int offset = 0;
        for (int i = insLengths.Count - 1; i >= 0; i--)
        {
            offset += insLengths[i];
            if (offset >= overwrite)
            {
                return offset;
            }
        }

        return 0;
    }

    internal static ulong GetFunctionReturn(Process process, ulong functionAddress)
    {
        byte[] pageBytes = ReadMemoryBytes(process, functionAddress, 0x1000);
        Instruction[] instructions = TInstruction.GetInstructions2(pageBytes);

        ulong retAddress = functionAddress;
        int offset = 0;

        foreach (Instruction ins in instructions)
        {
            if (ins.ToString() == "ret") return retAddress + (ulong)offset;
            else offset += ins.Bytes.Length;
        }

        return 0;
    }

    internal static byte[] ConvertRelativeToAbsolute(byte[] bytes, ulong originalAddress)
    {
        Instruction[] instructions = TInstruction.GetInstructions2(bytes);
        List<byte[]> newBytes = new List<byte[]>();

        int offset = 0;
        foreach (Instruction ins in instructions)
        {
            string txtIns = ins.ToString();

            if (ins.Bytes.Length == 5)
            {
                if (txtIns.StartsWith("call"))
                {
                    ulong actualEnd = GetActualAddressFromRelative5ByteInstruction(ins.Bytes, originalAddress + (ulong)offset);
                    newBytes.Add(GetAbsoluteCallBytes(actualEnd));
                }

                else if (txtIns.StartsWith("jmp"))
                {
                    ulong actualEnd = GetActualAddressFromRelative5ByteInstruction(ins.Bytes, originalAddress + (ulong)offset);
                    newBytes.Add(GetAbsoluteJumpBytes(actualEnd));
                }
                else newBytes.Add(ins.Bytes);
            }

            else newBytes.Add(ins.Bytes);

            offset += ins.Bytes.Length;
        }

        return TArray.Merge(newBytes);
    }

    internal static ulong GetActualAddressFromRelative5ByteInstruction(byte[] bytes, ulong address)
    {
        Instruction instr = TInstruction.GetInstruction2(bytes, address);
        if (instr.Bytes.Length == 5)
        {
            int value = BitConverter.ToInt32(bytes, 1);
            return address + (ulong)(value + instr.Bytes.Length);
        }
        return 0;
    }

    internal static bool ConfirmBytes(Process process, ulong address, string signature)
    {
        return ConfirmBytes(process, address, TSignature.GetBytes(signature));
    }

    internal static bool ConfirmBytes(Process process, ulong address, byte[] bytes)
    {
        byte[] read = ReadMemoryBytes(process, address, bytes.Length);

        if (read != null) return read.SequenceEqual(bytes);
        else return false;
    }

    internal static string GetSignature(byte[] array, bool noSpaces = false)
    {
        string hex = BitConverter.ToString(array);
        if (noSpaces) return hex.Replace("-", " ").Replace(" ", "");
        else return hex.Replace("-", " ");
    }

    internal static bool FreeMemory(Process process, ulong address, int size, uint type = 0x00008000)
    {
        return TImports.VirtualFreeEx(process.Handle, (IntPtr)address, size, type);
    }

    internal static void FixRelative(Process process, ulong original, ulong current, int size)
    {
        byte[] originalBytes = ReadMemoryBytes(process, original, size);
        byte[] currentBytes = ReadMemoryBytes(process, current, size);

        Instruction[] _org = TInstruction.GetInstructions2(originalBytes);
        Instruction[] _cur = TInstruction.GetInstructions2(originalBytes);

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
                uint ripValue = TConvert.Parse<uint>(ripValueTxt);

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
    
    internal static byte[] GetAbsoluteJumpBytes(ulong destination)
    {
        byte[] stub = new byte[] { 0xFF, 0x25, 0x00, 0x00, 0x00, 0x00 };
        byte[] full = TArray.Merge(stub, BitConverter.GetBytes(destination));
        return full;
    }

    internal static ulong CreateAbsoluteJump(Process process, ulong source, ulong destination)
    {
        byte[] stub = new byte[] { 0xFF, 0x25, 0x00, 0x00, 0x00, 0x00 };
        byte[] full = TArray.Merge(stub, BitConverter.GetBytes(destination));
        RefWriteBytes(process, source, full);
        return (ulong)full.Length;
    }

    internal static ulong CreateAbsoluteCall(Process process, ulong source, ulong destination, byte rspArguments = 0)
    {
        byte[] subRsp = new byte[] { 0x48, 0x83, 0xEC, rspArguments };
        byte[] start = new byte[] { 0xEB, 0x08 };
        byte[] address = BitConverter.GetBytes(destination);
        byte[] end = new byte[] { 0xFF, 0x15, 0xF2, 0xFF, 0xFF, 0xFF, 0x90 };
        byte[] addRsp = new byte[] { 0x48, 0x83, 0xC4, rspArguments };

        byte[] full = null;
        if (rspArguments == 0) full = TArray.Merge(start, address, end);
        else full = TArray.Merge(subRsp, start, address, end, addRsp);

        RefWriteBytes(process, source, full);
        return (ulong)full.Length;
    }

    internal static byte[] GetAbsoluteCallBytes(ulong destination)
    {
        byte[] start = new byte[] { 0xEB, 0x08 };
        byte[] address = BitConverter.GetBytes(destination);
        byte[] end = new byte[] { 0xFF, 0x15, 0xF2, 0xFF, 0xFF, 0xFF, 0x90 };
        return TArray.Merge(start, address, end);
    }

    public static ulong ScanAdvanced(Process process, TSignature.ScanData scanData, string moduleName = null)
    {
        ulong searchAddress = 0;

        List<ulong[]> sections = GetAllSections(process, moduleName);
        List<byte[]> sectionsBytes = new List<byte[]>();

        foreach (ulong[] rSection in sections)
        {
            byte[] readBytes = ReadMemoryBytes(process, rSection[0], (int)rSection[1]);
            sectionsBytes.Add(readBytes);
        }

        List<KeyValuePair<string, int>> checkpoints = new List<KeyValuePair<string, int>>(scanData.Checkpoints);
        checkpoints.Insert(0, new KeyValuePair<string, int>(scanData.Signature, 0));

        // incorrect queen index check
        if (scanData.QueenCheckpointIndex < 0 || scanData.QueenCheckpointIndex >= checkpoints.Count)
            return 0;

        byte[] baseByteSignature = TSignature.GetBytes(scanData.Signature);

        for (int i = 0; i < sections.Count; i++)
        {
            if (sectionsBytes[i] == null) continue;
            int searchOffset = 0;

            int offsetSuccess = 0;

            bool chainSuccess = false;
            int baseOffset = 0;

            for (int j = 0; j < checkpoints.Count; j++)
            {
                string[] separateSigs = checkpoints[j].Key.Split(',');
                for (int k = 0; k < separateSigs.Length; k++)
                {
                    byte[] searchBytes = TSignature.GetBytes(separateSigs[k]);
                    string searchMask = TSignature.GetMask(separateSigs[k]);

                    int maxDistance = 0;

                    if (checkpoints[j].Value == 0) maxDistance = 0;
                    else maxDistance = checkpoints[j].Value;

                    if (scanData.ReversedSearch && j != 0 && k == 0)
                    {
                        searchOffset = searchOffset - checkpoints[j].Value;
                        if (searchOffset <= 0 && Math.Abs(searchOffset) < searchBytes.Length)
                        {
                            chainSuccess = false;
                            break;
                        }
                    }

                    int searchOffset2 = FindInArray(sectionsBytes[i], searchBytes, searchMask, searchOffset, maxDistance);
                    chainSuccess = searchOffset2 != -1;

                    if (chainSuccess)
                    {
                        searchOffset = searchOffset2;
                        if (j == 0) baseOffset = searchOffset2;
                        if (j == scanData.QueenCheckpointIndex) offsetSuccess = searchOffset2;
                        break;
                    }
                    else continue;
                }

                if (chainSuccess) continue;
                else if (j == 0) break;
                else
                {
                    searchOffset = baseOffset + baseByteSignature.Length;
                    if (searchOffset + baseByteSignature.Length > (int)sections[i][1]) break;
                    j = -1;
                }
            }

            if (chainSuccess)
            {
                searchAddress = sections[i][0] + (ulong)offsetSuccess;
                break;
            }
        }

        if (searchAddress != 0)
        {
            if (scanData.Relative)
            {
                ulong searchAddressRelative = (ulong)((long)searchAddress + scanData.ToRelativeInstructionOffset);
                Instruction instr = TInstruction.GetInstruction2(process, searchAddressRelative);

                long value = TInstruction.ExtractRipValue(instr);

                if (value != 0) searchAddress = (ulong)((long)searchAddressRelative + value) + (ulong)instr.Bytes.Length;
                else return 0;
            }
            else if (scanData.FindStartFunction)
            {
                ulong newAddress = TInstruction.GetAlignedAddress(process, searchAddress);
                byte[] disasm = ReadMemoryBytes(process, newAddress - 0x1000, 0x1000);
                Instruction[] instrs = TInstruction.GetInstructions2(disasm, newAddress - 0x1000);

                for (int i = instrs.Length - 1; i >= 0; i--)
                {
                    if (instrs[i].ToString() == "int3")
                    {
                        break;
                    }

                    newAddress -= (ulong)instrs[i].Bytes.Length;
                }

                searchAddress = newAddress;
            }

            searchAddress = (ulong)((long)searchAddress + scanData.Offset);
        }

        return searchAddress;
    }

    internal static ulong ScanSingle(Process process, string signature, string moduleName = null, int offset = 0)
    {
        byte[] searchBytes = TSignature.GetBytes(signature);
        string searchMask = TSignature.GetMask(signature);

        List<ulong[]> sections = GetAllSections(process, moduleName);

        foreach (ulong[] section in sections)
        {
            byte[] sectionBytes = ReadMemoryBytes(process, section[0], (int)section[1]);
            if (sectionBytes != null && sectionBytes.Length > 0)
            {
                int searchOffset = FindInArray(sectionBytes, searchBytes, searchMask);
                if (searchOffset != -1)
                {
                    ulong searchAddress = section[0] + (ulong)searchOffset;
                    searchAddress = (ulong)((long)searchAddress + offset);
                    return searchAddress;
                }
            }
        }

        return 0;
    }

    internal static ulong ScanRel(Process process, int offset, string signature)
    {
        byte[] searchBytes = TSignature.GetBytes(signature);
        string searchMask = TSignature.GetMask(signature);

        List<ulong[]> sections = GetAllSections(process);

        foreach (ulong[] section in sections)
        {
            byte[] sectionBytes = ReadMemoryBytes(ProcessInstance, section[0], (int)section[1]);
            if (sectionBytes != null && sectionBytes.Length > 0)
            {
                int searchOffset = FindInArray(sectionBytes, searchBytes, searchMask);
                if (searchOffset != -1)
                {
                    ulong searchAddress = section[0] + (ulong)searchOffset;
                    ulong relativeAddress = searchAddress + (ulong)offset;
                    long relativeValue = ReadMemory<int>(ProcessInstance, relativeAddress);
                    ulong destinationAddress = (ulong)((long)searchAddress + relativeValue + offset + 4);
                    return destinationAddress;
                }
            }
        }

        return 0;
    }

    internal static ulong ScanRel2(Process process, string signature, string moduleName = null, int offset = 0)
    {
        byte[] searchBytes = TSignature.GetBytes(signature);
        string searchMask = TSignature.GetMask(signature);

        ulong searchAddress = ScanSingle(process, signature, moduleName);
        if (searchAddress == 0) return 0;

        searchAddress = (ulong)((long)searchAddress + offset);

        Instruction instr = TInstruction.GetInstruction2(process, searchAddress);
        long value = TInstruction.ExtractRipValue(instr);

        if (value != 0) searchAddress = (ulong)((long)searchAddress + value) + (ulong)instr.Bytes.Length;
        else return 0;

        return searchAddress;
    }

    public static List<ulong[]> GetAllSections(Process process, string moduleName = null)
    {
        List<ulong[]> sections = new List<ulong[]>();

        ProcessModule procModule = TProcess.GetModule(process, moduleName);
        if (procModule == null) return sections;

        ulong baseAddress = (ulong)procModule.BaseAddress;
        byte[] peHeader = GetPEHeader(process, baseAddress);

        int peHeaderOffset = BitConverter.ToInt32(peHeader, 0x3C);
        int sectionCount = BitConverter.ToInt16(peHeader, peHeaderOffset + 0x6);
        int optionalHeaderSize = BitConverter.ToInt16(peHeader, peHeaderOffset + 0x14);
        int sectionHeaderOffset = peHeaderOffset + 0x18 + optionalHeaderSize;

        for (int i = 0; i < sectionCount; i++)
        {
            int currentSectionOffset = sectionHeaderOffset + (i * 0x28);
            uint virtualSize = BitConverter.ToUInt32(peHeader, currentSectionOffset + 0x08);
            uint virtualAddress = BitConverter.ToUInt32(peHeader, currentSectionOffset + 0x0C);

            sections.Add(new ulong[] { baseAddress + virtualAddress, virtualSize });
        }

        if (sections.Count > 0) sections = sections.OrderBy(x => x[0]).ToList();
        return sections;
    }

    internal static byte[] GetPEHeader(Process process, ulong baseAddress)
    {
        byte[] dosHeader = ReadMemoryBytes(process, baseAddress, 0x40);
        if (dosHeader == null) return null;

        int peHeaderOffset = BitConverter.ToInt32(dosHeader, 0x3C);

        byte[] peHeaderInfo = ReadMemoryBytes(process, baseAddress + (ulong)peHeaderOffset, 0x18);
        if (peHeaderInfo == null) return null;

        int sectionCount = BitConverter.ToInt16(peHeaderInfo, 0x6);
        int optionalHeaderSize = BitConverter.ToInt16(peHeaderInfo, 0x14);

        int sectionHeaderOffset = peHeaderOffset + 0x18 + optionalHeaderSize;
        int totalSize = sectionHeaderOffset + (sectionCount * 0x28);

        return ReadMemoryBytes(process, baseAddress, totalSize);
    }

    internal static int FindInArray(byte[] chunkData, string signature, int startPosition = 0, int maxDistance = 0)
    {
        return FindInArray(chunkData, TSignature.GetBytes(signature), TSignature.GetMask(signature), startPosition, maxDistance);
    }

    internal static int FindInArray(byte[] chunkData, byte[] byteSignature, string mask = "", int startPosition = 0, int maxDistance = 0)
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
            if (maxDistance != 0 && position >= startPosition + maxDistance)
            {
                return -1;
            }
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

        if (TImports.ReadProcessMemory(process.Handle, address, data, data.Length, out _))
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
        if (TImports.ReadProcessMemory(process.Handle, address, data, data.Length, out _))
            return data;
        return null;
    }
}
