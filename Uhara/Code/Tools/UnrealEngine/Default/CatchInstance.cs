using SharpDisasm;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class Tools :UShared
{
    public partial class UnrealEngine
    {
        public partial class Default
        {
            public class CatchInstance
            {
                private int EnvironmentAllocSize = 0x5000;

                private ulong FNamePoolAddress = 0;
                private ulong f_StaticConstructObject_Internal = 0;
                private ulong f_UObjectBeginDestroy = 0;

                private ulong AllocStart = 0;

                private ulong OutputEnd = 0;
                private ulong CodeEnd = 0;

                public enum EnvOffsets
                {
                    f_StartUharaSCOI = 0xD7,
                    f_StartUharaUOD = 0x22A,
                    CodeEnd = 0x500,
                    Data = 0x1000,
                    Arguments = 0x2000,
                    Output = 0x3000,
                }

                Dictionary<string, byte> QueueItems = new Dictionary<string, byte>();

                public void SetData(IntPtr fNamePoolAddress, IntPtr staticConstructObjectAddress)
                {
                    SetData(fNamePoolAddress, staticConstructObjectAddress, IntPtr.Zero);
                }

                public void SetData(IntPtr fNamePoolAddress, IntPtr staticConstructObjectAddress, IntPtr beginDestroyAddress)
                {
                    FNamePoolAddress = (ulong)fNamePoolAddress;
                    f_StaticConstructObject_Internal = (ulong)staticConstructObjectAddress;
                    f_UObjectBeginDestroy = (ulong)beginDestroyAddress;
                }

                public IntPtr Add(string name, byte instances = 1)
                {
                    if (instances == 0) instances = 1;
                    
                    if (QueueItems.ContainsKey(name)) QueueItems[name] += instances;
                    else QueueItems[name] = instances;

                    ulong outputPointer = OutputEnd;

                    OutputEnd += 0x8 * (ulong)instances;
                    return (IntPtr)outputPointer;
                }

                public void ProcessQueue()
                {
                    try
                    {
                        if (FNamePoolAddress == 0)
                            return;

                        RefWriteBytes(Instance, AllocStart + (ulong)EnvOffsets.Data, BitConverter.GetBytes(FNamePoolAddress));

                        WriteArguments();
                        HookFunctions();

                    }
                    catch { }
                }

                private void HookFunctions()
                {
                    // StaticConstructObject_Internal
                    if (f_StaticConstructObject_Internal != 0)
                    {
                        byte[] pageBytes = UMemory.ReadMemoryBytes(Instance, f_StaticConstructObject_Internal, 0x1000);
                        Instruction[] instructions = UInstruction.GetInstructions2(pageBytes);

                        int offset = 0;
                        int backwards = 0;
                        ulong funcEnd = 0;

                        for (int i = 0; i < instructions.Length; i++)
                        {
                            if (i > 0) offset += instructions[i - 1].Length;
                            string txtIns = instructions[i].ToString();

                            if (txtIns == "ret")
                            {
                                funcEnd = f_StaticConstructObject_Internal + (ulong)offset + 1;

                                for (int j = i; j >= 0; j--)
                                {
                                    backwards += instructions[j].Bytes.Length;
                                    if (backwards >= 14) break;
                                }

                                break;
                            }
                        }

                        if (backwards < 1)
                            return;

                        {
                            ulong hookStart = funcEnd - (ulong)backwards;
                            byte[] stolen = UMemory.ReadMemoryBytes(Instance, hookStart, backwards - 1);
                            byte[] stolenConverted = UMemory.ConvertRelativeToAbsolute(stolen, hookStart);

                            ulong jmpToStart = CodeEnd;
                            RefWriteBytes(Instance, CodeEnd, stolenConverted);
                            CodeEnd += (ulong)stolenConverted.Length;

                            ulong f_StartUhara = AllocStart + (ulong)EnvOffsets.f_StartUharaSCOI;
                            UMemory.CreateAbsoluteJump(Instance, CodeEnd, f_StartUhara);
                            CodeEnd += 14;

                            byte[] jumpIn = UMemory.GetAbsoluteJumpBytes(jmpToStart);
                            MemoryCleaner.AddOverwrite(hookStart, jumpIn, stolen);
                            UMemory.CreateAbsoluteJump(Instance, hookStart, jmpToStart);

                            UProgram.Print("Successfully hooked StaticConstructObject at 0x" + hookStart.ToString("X"));
                        }
                    }

                    // UOBject::Destroy
                    if (f_UObjectBeginDestroy != 0)
                    {
                        ulong callToStartUhara = CodeEnd;
                        int minimumOverwrite = UInstruction.GetMinimumOverwrite(Instance, f_UObjectBeginDestroy, 14);
                        byte[] stolen = UMemory.ReadMemoryBytes(Instance, f_UObjectBeginDestroy, minimumOverwrite);
                        byte[] stolenConverted = UMemory.ConvertRelativeToAbsolute(stolen, f_UObjectBeginDestroy);

                        ulong f_StartUhara = AllocStart + (ulong)EnvOffsets.f_StartUharaUOD;

                        UMemory.CreateAbsoluteCall(Instance, CodeEnd, f_StartUhara);
                        CodeEnd += 16;

                        RefWriteBytes(Instance, CodeEnd, stolenConverted);
                        CodeEnd += (ulong)stolenConverted.Length;

                        UMemory.CreateAbsoluteJump(Instance, CodeEnd, f_UObjectBeginDestroy + (ulong)stolenConverted.Length);
                        CodeEnd += 14;

                        byte[] jumpIn = UMemory.GetAbsoluteJumpBytes(callToStartUhara);
                        MemoryCleaner.AddOverwrite(f_UObjectBeginDestroy, jumpIn, stolen);
                        UMemory.CreateAbsoluteJump(Instance, f_UObjectBeginDestroy, callToStartUhara);

                        UProgram.Print("Successfully hooked BeginDestroy at 0x" + f_UObjectBeginDestroy.ToString("X"));
                    }
                }

                private void WriteArguments()
                {
                    ulong argumentsAddress = AllocStart + (ulong)EnvOffsets.Arguments;

                    foreach (var item in QueueItems)
                    {
                        RefWriteBytes(Instance, argumentsAddress, new byte[] { item.Value });
                        argumentsAddress += 2;
                        byte[] nameBytes = UProgram.StringToMultibyte(item.Key);
                        RefWriteBytes(Instance, argumentsAddress, nameBytes);
                        argumentsAddress += (ulong)nameBytes.Length;
                    }

                    RefWriteBytes(Instance, OutputEnd, BitConverter.GetBytes((ulong)0x1337));
                }

                public CatchInstance()
                {
                    try
                    {
                        AllocStart = RefAllocateMemory(Instance, EnvironmentAllocSize);
                        if (AllocStart != 0)
                        {
                            MemoryCleaner.AddAllocate(AllocStart, EnvironmentAllocSize);

                            CodeEnd = AllocStart + (ulong)EnvOffsets.CodeEnd;
                            OutputEnd = AllocStart + (ulong)EnvOffsets.Output;
                            QueueItems = new Dictionary<string, byte>();
                            
                            byte[] asmBlock = AsmBlocks.UnrealEngine_CatchInstance;
                            byte[] asmDecoded = UArray.DecodeAsmBlock(asmBlock);
                            RefWriteBytes(Instance, AllocStart, asmDecoded);
                        }
                    }
                    catch { UProgram.Print("Creating tool failed"); }
                }
            }
        }
    }
}
