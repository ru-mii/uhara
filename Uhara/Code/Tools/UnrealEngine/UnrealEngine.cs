using SharpDisasm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public partial class Tools : MainShared
{
    public partial class UnrealEngine
    {
        public void LockFps(float fps)
        {
            try
            {
                bool success = false;
                try
                {
                    do
                    {
                        ProcessInstance = TProcess.RefreshProcess(ProcessInstance);

                        // ---
                        ulong address = TMemory.ScanSingle(ProcessInstance, "EB 03 0F 28 C6 48 8B 5C 24 ?? 0F 28 74 24 ?? 44 0F 28 ?? 24 ?? 44 0F 28 4C 24 ?? 48 83 C4 ?? 5F C3", null, 0x20);
                        if (address == 0) TMemory.ScanSingle(ProcessInstance, "EB 03 0F 28 C6 48 8B 5C 24 ?? 0F 28 74 24 ?? 44 0F 28 ?? 24 ?? 48 83 C4 ?? 5F C3", null, 0x20);
                        if (address == 0) break;

                        // ---
                        byte[] ending = TMemory.ReadMemoryBytes(ProcessInstance, address, 0x100);
                        if (ending == null) break;

                        Instruction[] instrs = TInstruction.GetInstructions2(ending, address);
                        if (instrs == null || instrs.Length < 3) break;

                        if (instrs[1].ToString() != "movaps xmm0, xmm6") break;

                        // ---
                        address = instrs[2].Offset;
                        int minimumOverwrite = TInstruction.GetMinimumOverwrite(ProcessInstance, address, 14);
                        if (minimumOverwrite == 0) break;

                        byte[] stolen = TMemory.ReadMemoryBytes(ProcessInstance, address, minimumOverwrite);
                        if (stolen == null) break;
                        MemoryManager.AddOverwrite(address, stolen);

                        ulong allocated = MemoryManager.AllocateSafe(0x1000);

                        RefWriteBytes(ProcessInstance, allocated, BitConverter.GetBytes(fps));
                        allocated += 0x8;

                        ulong myCodeStart = allocated;

                        byte[] overwriteXmm = new byte[] { 0x50, 0x48, 0x8B, 0x05, 0xF0, 0xFF, 0xFF, 0xFF, 0x66, 0x48, 0x0F, 0x6E, 0xC0, 0x58 };
                        RefWriteBytes(ProcessInstance, allocated, overwriteXmm);
                        allocated += (ulong)overwriteXmm.Length;
                        RefWriteBytes(ProcessInstance, allocated, stolen);
                        allocated += (ulong)stolen.Length;

                        TMemory.CreateAbsoluteJump(ProcessInstance, allocated, address + (ulong)minimumOverwrite);
                        TMemory.CreateAbsoluteJump(ProcessInstance, address, myCodeStart);

                        success = true;
                    }
                    while (false);

                }
                catch { }

                if (!success)
                {
                    TUtils.Print("LockFps function failed, this game might be incompatible, retrying...");
                    Thread.Sleep(1000);
                }
            }
            catch { }
        }

        public string FNameToString(uint fName)
        {
            try
            {
                do
                {
                    ulong FNamePoolAddress = ToolsShared.ToolData.UnrealEngine.D_FNamePoolAddress;
                    if (FNamePoolAddress == 0) break;

                    var nameIdx = (fName & 0x000000000000FFFF) >> 0x00;
                    var chunkIdx = (fName & 0x00000000FFFF0000) >> 0x10;
                    var number = (fName & 0xFFFFFFFF00000000) >> 0x20;

                    IntPtr chunk = (IntPtr)TMemory.ReadMemory<ulong>(ProcessInstance,
                        FNamePoolAddress + (ulong)(0x10 + (int)chunkIdx * 0x8));

                    IntPtr entry = chunk + (int)nameIdx * sizeof(short);
                    int length = TMemory.ReadMemory<short>(ProcessInstance, entry) >> 6;
                    if (length > byte.MaxValue || length <= 0) break;

                    return TUtils.MultibyteToString(TMemory.ReadMemoryBytes(ProcessInstance, entry + sizeof(short), length));
                }
                while (false);
            }
            catch { }
            return null;
        }
    }
}