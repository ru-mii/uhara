using SharpDisasm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

public partial class Tools : MainShared
{
    public partial class Unity
    {
        public partial class IL2CPP
        {
            public partial class Instance
            {
                internal class InstanceDestroy
                {
                    #region VARIABLES
                    static bool Loaded = false;
                    static int SubToolGeneralLimit = 15000;

                    static ulong AllocateSize = 0x10000;
                    static ulong AllocateStart = 0;

                    static ulong AddressFreeUse = 0;
                    static ulong AddressArguments = 0;
                    static ulong AddressArgumentsData = 0;

                    private struct GeneratedOffsets
                    {
                        public const ulong AddressArguments = 0x2000;
                        public const ulong AddressArgumentsData = 0x6000;
                        public const ulong AddressFreeUse = 0x80;

                        public const ulong HK_HookPoint = 0x10;

                        public const ulong ReturnAddress = 0x0;
                        public const ulong il2cpp_gc_wbarrier_set_field = 0x8;
                    }

                    static ulong GCStart = 0;
                    Dictionary<ulong, List<ulong>> GCCallsAll = new Dictionary<ulong, List<ulong>>();
                    // key is result address
                    // then the gc set field calls

                    ulong CallFinalSetField = 0;
                    #endregion
                    #region ASM_CODE
                    byte[] AsmCode = new byte[] {
    0xBC, 0x90, 0xDD, 0x90, 0x10, 0x90, 0x22, 0x90, 0xFC, 0x90, 0x7F, 0x90, 0x00, 0x90, 0x00, 0x90, 0x70, 0x90, 0xFA, 0x90, 0xDB, 0x90, 0x19, 0x90, 0xFC, 0x90, 0x7F, 0x90, 0x00, 0x90, 0x00, 0x90, 0x48, 0x90, 0x8D, 0x90, 0x05, 0x90, 0xE9, 0x90, 0xFF, 0x90, 0xFF, 0x90, 0xFF, 0x90, 0x48, 0x90, 0x8B, 0x90, 0x00, 0x90, 0x48, 0x90, 0x39, 0x90, 0x04, 0x90, 0x24, 0x90, 0x75, 0x90, 0x57, 0x90, 0x48, 0x90, 0x83, 0x90, 0xEC, 0x90, 0x28, 0x90, 0x52, 0x90, 0x48, 0x90, 0x83, 0x90, 0xEC, 0x90, 0x28, 0x90, 0xFF, 0x90, 0x15, 0x90, 0xD9, 0x90, 0xFF, 0x90, 0xFF, 0x90, 0xFF, 0x90, 0x48, 0x90, 0x83, 0x90, 0xC4, 0x90, 0x28, 0x90, 0x5A, 0x90, 0x50, 0x90, 0x41, 0x90, 0x52, 0x90, 0x48, 0x90, 0x85, 0x90, 0xD2, 0x90, 0x74, 0x90, 0x29, 0x90, 0x4C, 0x90, 0x8B, 0x90, 0x02, 0x90, 0x48, 0x90, 0x8D, 0x90, 0x05, 0x90, 0xBA, 0x90, 0x1F, 0x90, 0x00, 0x90, 0x00, 0x90, 0x48, 0x90, 0x83, 0x90, 0xE8, 0x90, 0x08, 0x90, 0x48, 0x90, 0x83, 0x90, 0xC0, 0x90, 0x08, 0x90, 0x48, 0x90, 0x83, 0x90, 0x38, 0x90, 0x00, 0x90, 0x74, 0x90, 0x11, 0x90, 0x4C, 0x90, 0x8B, 0x90, 0x10, 0x90, 0x4D, 0x90, 0x3B, 0x90, 0x02, 0x90, 0x75, 0x90, 0xEE, 0x90, 0x49, 0x90, 0xC7, 0x90, 0x02, 0x90, 0x00, 0x90, 0x00, 0x90, 0x00, 0x90, 0x00, 0x90, 0xEB, 0x90, 0xE5, 0x90, 0x41, 0x90, 0x5A, 0x90, 0x58, 0x90, 0x48, 0x90, 0x83, 0x90, 0xC4, 0x90, 0x28, 0x90, 0xC3, 0x90, 0x4C, 0x90, 0x8D, 0x90, 0x05, 0x90, 0x8C, 0x90, 0xFF, 0x90, 0xFF, 0x90, 0xFF, 0x90, 0x41, 0x90, 0xFF, 0x90, 0x20, 0x90, 0x48, 0x90, 0x8D, 0x90, 0x05, 0x90, 0x8A, 0x90, 0xFF, 0x90, 0xFF, 0x90, 0xFF, 0x90, 0xFF, 0x90, 0x20
};
                    #endregion

                    #region INTERNAL_API
                    internal void AddArgument(ulong instancePtr, short instances)
                    {
                        do
                        {
                            if (!Loaded) break;
                            for (int i = 0; i < instances; i++)
                            {
                                RefWriteBytes(ProcessInstance, AddressArguments, BitConverter.GetBytes(instancePtr + (ulong)(i * 0x8)));
                                AddressArguments += 0x8;
                            }
                        }
                        while (false);
                    }
                    #endregion

                    public InstanceDestroy()
                    {
                        try
                        {
                            do
                            {
                                if (ScanData() != Result.Success) break;
                                if (Allocate() != Result.Success) break;
                                if (WriteArgs() != Result.Success) break;
                                if (HookCode() != Result.Success) break;

                                Loaded = true;
                                TUtils.Print(DebugClass + "." + GetType().Name + "." + MethodBase.GetCurrentMethod().Name +
                                    " | " + "[FINISHED]");
                            }
                            while (false);
                        }
                        catch { }
                    }

                    #region SCAN_DATA
                    private Result ScanData()
                    {
                        Result result = Result.None;
                        try
                        {
                            do
                            {
                                ProcessInstance = TProcess.RefreshProcess(ProcessInstance);

                                ulong il2cpp_gc_wbarrier_set_field = TProcess.GetProcAddress(ProcessInstance, "GameAssembly.dll", "il2cpp_gc_wbarrier_set_field");
                                if (il2cpp_gc_wbarrier_set_field == 0) break;

                                ulong moduleBase = TProcess.GetModuleBase(ProcessInstance, "UnityPlayer.dll");
                                if (moduleBase == 0) break;

                                List<ulong> results = TMemory.ScanMultiple(ProcessInstance, "48 83 C4 ?? 5B C3 48", "UnityPlayer.dll", 0x20).ToList();

                                // ---
                                if (results.Count > 1)
                                {
                                    for (int i = 0; i < results.Count; i++)
                                    {
                                        try
                                        {
                                            GCCallsAll[results[i]] = new List<ulong>();

                                            ulong pastFirstReturn = results[i] + 6;

                                            ulong functionEnd = TMemory.GetFunctionReturn(ProcessInstance, pastFirstReturn);
                                            if (functionEnd != 0)
                                            {
                                                byte[] functionBytes = TMemory.ReadMemoryBytes(ProcessInstance, pastFirstReturn, (int)(functionEnd - pastFirstReturn));
                                                Instruction[] functionOps = TInstruction.GetInstructions2(functionBytes, pastFirstReturn);

                                                int counter = 0;
                                                foreach (Instruction op in functionOps)
                                                {
                                                    // is searched call
                                                    if (!op.ToString().StartsWith("call ")) continue;
                                                    if (op.Bytes[0] != 0xFF || op.Bytes[1] != 0x15) continue;

                                                    // relative check
                                                    uint value = TMemory.ReadMemory<uint>(ProcessInstance, op.Offset + 0x2);
                                                    ulong relativePtr = op.Offset + value + 0x6;
                                                    ulong relative = 0; try { relative = TMemory.ReadMemory<ulong>(ProcessInstance, relativePtr); } catch { }
                                                    if (relative != il2cpp_gc_wbarrier_set_field) continue;

                                                    GCCallsAll[results[i]].Add(op.Offset);

                                                    // counter update
                                                    counter++;
                                                }

                                                if (counter >= 3)
                                                    continue;

                                                results.RemoveAt(i);
                                                i--;
                                            }
                                        }
                                        catch { }
                                    }
                                }

                                // ---
                                if (results.Count > 1)
                                {
                                    for (int i = 0; i < results.Count; i++)
                                    {
                                        try
                                        {
                                            byte[] byteBlock = TMemory.ReadMemoryBytes(ProcessInstance, results[i], 200);
                                            byte[] signatureBytes = TSignature.GetBytes("48 83 C4 ?? 5B C3 CC CC");
                                            string signatureMask = TSignature.GetMask("48 83 C4 ?? 5B C3 CC CC");

                                            int offset = TMemory.FindInArray(byteBlock, signatureBytes, signatureMask);
                                            if (offset == -1)
                                            {
                                                results.RemoveAt(i);
                                                i--;
                                            }
                                        }
                                        catch { }
                                    }
                                }

                                // ---
                                if (results.Count > 1)
                                {
                                    for (int i = 0; i < results.Count; i++)
                                    {
                                        try
                                        {
                                            byte[] byteBlock = TMemory.ReadMemoryBytes(ProcessInstance, results[i] - 0x40, 0x40);
                                            byte[] signatureBytes = TSignature.GetBytes("CC 40 53 48 83 EC ??");
                                            string signatureMask = TSignature.GetMask("CC 40 53 48 83 EC ??");

                                            int offset = TMemory.FindInArray(byteBlock, signatureBytes, signatureMask);
                                            if (offset == -1)
                                            {
                                                results.RemoveAt(i);
                                                i--;
                                            }
                                        }
                                        catch { }
                                    }
                                }

                                // ---
                                if (results.Count == 1)
                                {
                                    try
                                    {
                                        List<ulong> setFieldCalls = GCCallsAll[results[0]];
                                        if (setFieldCalls.Count >= 3)
                                        {
                                            CallFinalSetField = setFieldCalls[setFieldCalls.Count - 3];
                                        }
                                    }
                                    catch { }
                                }

                                result = CallFinalSetField != 0 ? Result.Success : Result.Failed;
                            }
                            while (false);
                            if (result == Result.None) result = Result.Failed;
                        }
                        catch { }

                        TUtils.Print(DebugClass + "." + GetType().Name + "." + MethodBase.GetCurrentMethod().Name +
                            " | " + "Result: " + result.ToString()); return result;
                    }
                    #endregion
                    #region ALLOCATE
                    private Result Allocate()
                    {
                        Result result = Result.None;
                        try
                        {
                            do
                            {
                                AllocateStart = MemoryManager.AllocateSafe((int)AllocateSize, ToolUniqueID);
                                if (AllocateStart == 0) break;

                                byte[] decoded = TArray.DecodeBlock(AsmCode);
                                RefWriteBytes(ProcessInstance, AllocateStart, decoded);

                                AddressArguments = AllocateStart + GeneratedOffsets.AddressArguments;
                                AddressArgumentsData = AllocateStart + GeneratedOffsets.AddressArgumentsData;
                                AddressFreeUse = AllocateStart + GeneratedOffsets.AddressFreeUse;

                                result = Result.Success;
                            }
                            while (false);
                        }
                        catch { }
                        TUtils.Print(DebugClass + "." + GetType().Name + "." + MethodBase.GetCurrentMethod().Name +
                            " | " + "Result: " + result.ToString()); return result;
                    }
                    #endregion
                    #region WRITE_ARGS
                    private Result WriteArgs()
                    {
                        Result result = Result.None;
                        do
                        {
                            RefWriteBytes(ProcessInstance, AllocateStart + GeneratedOffsets.ReturnAddress, BitConverter.GetBytes(CallFinalSetField + 0x6));
                            RefWriteBytes(ProcessInstance, AllocateStart + GeneratedOffsets.il2cpp_gc_wbarrier_set_field, BitConverter.GetBytes(TProcess.GetProcAddress(ProcessInstance, "GameAssembly.dll", "il2cpp_gc_wbarrier_set_field")));
                            result = Result.Success;
                        }
                        while (false);
                        TUtils.Print(DebugClass + "." + GetType().Name + "." + MethodBase.GetCurrentMethod().Name +
                            " | " + "Result: " + result.ToString()); return result;
                    }
                    #endregion
                    #region HOOK_CODE
                    private Result HookCode()
                    {
                        Result result = Result.None;
                        do
                        {
                            ulong setFieldPtr = CallFinalSetField + TMemory.ReadMemory<uint>(ProcessInstance, CallFinalSetField + 0x2) + 0x6;

                            byte[] saveBytes = TMemory.ReadMemoryBytes(ProcessInstance, setFieldPtr, 0x8);
                            if (saveBytes == null || saveBytes.Length == 0) break;

                            MemoryManager.AddOverwrite(setFieldPtr, saveBytes, ToolUniqueID);
                            RefWriteBytes(ProcessInstance, setFieldPtr, BitConverter.GetBytes(AllocateStart + GeneratedOffsets.HK_HookPoint));

                            //TUtils.Print(DebugClass + "." + GetType().Name + "." + MethodBase.GetCurrentMethod().Name +
                            //" | " + "Hook: " + "0x" + CallFinalSetField.ToString("X"));

                            result = Result.Success;
                        }
                        while (false);
                        TUtils.Print(DebugClass + "." + GetType().Name + "." + MethodBase.GetCurrentMethod().Name +
                            " | " + "Result: " + result.ToString()); return result;
                    }
                    #endregion
                }
            }
        }
    }
}