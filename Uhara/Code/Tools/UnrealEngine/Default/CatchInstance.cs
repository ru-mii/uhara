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

                private ulong FNamePool = 0;
                private ulong StaticConstructObject_Internal = 0;
                private ulong UObjectBeginDestroy = 0;

                private ulong AllocStart = 0;
                private ulong OutputEnd = 0;
                private ulong CodeEnd = 0;

                private string UEVersion;

                public enum EnvOffsets
                {
                    f_StartUharaSCOI = 0xEC,
                    f_StartUharaUOD = 0x23F,
                    CodeEnd = 0x500,
                    Data = 0x1000,
                    Arguments = 0x2000,
                    Output = 0x3000,
                }

                private readonly string[] SupportedUEVersions = new string[]
                {
                    "4.27.2.0",
                    "5.1.1.0",
                    "5.3.2.0",
                    "5.5.4.0",
                    "5.6.0.0",
                };

                Dictionary<string, byte> QueueItems = new Dictionary<string, byte>();

                public void SetUEVersion(string version)
                {
                    UEVersion = version;
                }

                public void SetData(IntPtr fNamePoolAddress, IntPtr staticConstructObjectAddress)
                {
                    SetData(fNamePoolAddress, staticConstructObjectAddress, IntPtr.Zero);
                }

                public void SetData(IntPtr fNamePoolAddress, IntPtr staticConstructObjectAddress, IntPtr beginDestroyAddress)
                {
                    FNamePool = (ulong)fNamePoolAddress;
                    StaticConstructObject_Internal = (ulong)staticConstructObjectAddress;
                    UObjectBeginDestroy = (ulong)beginDestroyAddress;
                }

                public IntPtr Add(string name, byte instances = 1)
                {
                    try
                    {
                        if (instances == 0) instances = 1;

                        if (QueueItems.ContainsKey(name)) QueueItems[name] += instances;
                        else QueueItems[name] = instances;

                        ulong outputPointer = OutputEnd;

                        OutputEnd += 0x8 * (ulong)instances;
                        return (IntPtr)outputPointer;
                    }
                    catch { }
                    return IntPtr.Zero;
                }

                public void ProcessQueue()
                {
                    try
                    {
                        ScanForInfo();

                        if (FNamePool == 0)
                            return;

                        // provide fnamepool to the remote thread
                        RefWriteBytes(Instance, AllocStart + (ulong)EnvOffsets.Data, BitConverter.GetBytes(FNamePool));

                        WriteArguments();
                        HookFunctions();

                    }
                    catch { }
                }

                private void ScanForInfo()
                {
                    try
                    {
                        string processPath = Instance.MainModule.FileName;

                        if (UEVersion == null)
                            UEVersion = UProgram.GetFileVersion(processPath);

                        if (UEVersion == null)
                        {
                            UProgram.Print("Couldn't retrieve Unreal Engine version, use SetUEVersion() "+
                                "to set the version manuall, with 1.2.3.4 format");
                            return;
                        }

                        if (!SupportedUEVersions.Contains(UEVersion))
                        {
                            UProgram.Print(UEVersion + " is not supported for " + GetType().Name + " tool, " +
                                "use SetData(FNamePool, StaticConstructObject_Internal, UObject::BeginDestroy) " +
                                "to provide required addresses for this tool");
                            return;
                        }

                        string alreadyHookedStart = "FF 25 ?? ?? ?? ??";

                        if (FNamePool == 0)
                        {
                            USignature.AdvancedSignature advSig =
                                Signatures.UnrealEngine.Get(Signatures.UnrealEngine.Data.FNamePool, UEVersion);

                            FNamePool = UMemory.ScanSingle(advSig);

                            if (FNamePool == 0 && advSig.Signature.Length >= alreadyHookedStart.Length)
                            {
                                advSig.Signature = advSig.Signature.Substring(alreadyHookedStart.Length);
                                advSig.Signature = alreadyHookedStart + advSig.Signature;

                                FNamePool = UMemory.ScanSingle(advSig);
                            }
                        }

                        if (StaticConstructObject_Internal == 0)
                        {
                            USignature.AdvancedSignature advSig =
                                Signatures.UnrealEngine.Get(Signatures.UnrealEngine.Function.StaticConstructObject_Internal, UEVersion);

                            StaticConstructObject_Internal = UMemory.ScanSingle(advSig);

                            if (StaticConstructObject_Internal == 0 && advSig.Signature.Length >= alreadyHookedStart.Length)
                            {
                                advSig.Signature = advSig.Signature.Substring(alreadyHookedStart.Length);
                                advSig.Signature = alreadyHookedStart + advSig.Signature;

                                StaticConstructObject_Internal = UMemory.ScanSingle(advSig);
                            }
                        }

                        if (UObjectBeginDestroy == 0)
                        {
                            USignature.AdvancedSignature advSig =
                                Signatures.UnrealEngine.Get(Signatures.UnrealEngine.Function.UObjectBeginDestroy, UEVersion);

                            UObjectBeginDestroy = UMemory.ScanSingle(advSig);

                            if (UObjectBeginDestroy == 0 && advSig.Signature.Length >= alreadyHookedStart.Length)
                            {
                                advSig.Signature = advSig.Signature.Substring(alreadyHookedStart.Length);
                                advSig.Signature = alreadyHookedStart + advSig.Signature;

                                UObjectBeginDestroy = UMemory.ScanSingle(advSig);
                            }
                        }
                    }
                    catch { }
                }

                private void HookFunctions()
                {
                    // StaticConstructObject_Internal
                    if (StaticConstructObject_Internal != 0)
                    {
                        byte[] pageBytes = UMemory.ReadMemoryBytes(Instance, StaticConstructObject_Internal, 0x1000);
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
                                funcEnd = StaticConstructObject_Internal + (ulong)offset + 1;

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
                    if (UObjectBeginDestroy != 0)
                    {
                        ulong callToStartUhara = CodeEnd;
                        int minimumOverwrite = UInstruction.GetMinimumOverwrite(Instance, UObjectBeginDestroy, 14);
                        byte[] stolen = UMemory.ReadMemoryBytes(Instance, UObjectBeginDestroy, minimumOverwrite);
                        byte[] stolenConverted = UMemory.ConvertRelativeToAbsolute(stolen, UObjectBeginDestroy);

                        ulong f_StartUhara = AllocStart + (ulong)EnvOffsets.f_StartUharaUOD;

                        UMemory.CreateAbsoluteCall(Instance, CodeEnd, f_StartUhara);
                        CodeEnd += 16;

                        RefWriteBytes(Instance, CodeEnd, stolenConverted);
                        CodeEnd += (ulong)stolenConverted.Length;

                        UMemory.CreateAbsoluteJump(Instance, CodeEnd, UObjectBeginDestroy + (ulong)stolenConverted.Length);
                        CodeEnd += 14;

                        byte[] jumpIn = UMemory.GetAbsoluteJumpBytes(callToStartUhara);
                        MemoryCleaner.AddOverwrite(UObjectBeginDestroy, jumpIn, stolen);
                        UMemory.CreateAbsoluteJump(Instance, UObjectBeginDestroy, callToStartUhara);

                        UProgram.Print("Successfully hooked BeginDestroy at 0x" + UObjectBeginDestroy.ToString("X"));
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
