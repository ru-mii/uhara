using SharpDisasm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static TSignature;

internal class ScanUtility : MainShared
{
    public enum GameEngine
    {
        Unity,
        UnrealEngine
    }

    internal class UnrealEngine
    {
        internal static ulong SearchAddress(Enum identifier)
        {
            if (identifier is Function function)
            {
                if (function == Function.StaticConstructObject_Internal)
                {
                    do
                    {
                        ScanData scanData = new ScanData();
                        scanData.Signature = "00 00 00 80 00 00 10";
                        scanData.ReversedSearch = true;

                        scanData.Checkpoints = new List<KeyValuePair<string, int>>
                        {
                            new KeyValuePair<string, int>("FF FF 48 81 EC , 48 81 EC ?? ?? 00 00", 100),
                            new KeyValuePair<string, int>("C3 CC , E9 ?? ?? ?? ?? CC , CC CC", 115),
                        };
                        scanData.QueenCheckpointIndex = 2;

                        ulong address = TMemory.ScanAdvanced(ProcessInstance, scanData);
                        if (address == 0) break;

                        byte[] instrBytes = TMemory.ReadMemoryBytes(ProcessInstance, address, 50);
                        if (instrBytes == null) break;

                        Instruction[] instrs = TInstruction.GetInstructions2(instrBytes);
                        if (instrs == null) break;

                        if (instrs.Length < 2) break;

                        int offset = 0;
                        for (int i = 1; i < instrs.Length; i++)
                        {
                            if (instrs[i - 1].ToString() == "int3" && instrs[i].ToString() != "int3")
                            {
                                address += (ulong)offset + 1;
                                return address;
                            }
                            offset += instrs[i].Bytes.Length;
                        }
                    }
                    while (false);
                    return 0;
                }

                else if (function == Function.UObject_BeginDestroy)
                {
                    ulong address = 0;

                    if (address == 0)
                    {
                        ScanData scanData = new ScanData();
                        scanData.Signature = "48 83 EC ?? 8B 41 08 48 8B D9 C1 E8 0F A8 01 75";
                        scanData.FindStartFunction = true;

                        scanData.Checkpoints = new List<KeyValuePair<string, int>>
                        {
                            new KeyValuePair<string, int>("83 7C 24 ?? 00", 100),
                        };

                        address = TMemory.ScanAdvanced(ProcessInstance, scanData);
                    }

                    if (address == 0)
                    {
                        ScanData scanData = new ScanData();
                        scanData.Signature = "FF 25 00 00 00 00";
                        scanData.FindStartFunction = true;

                        scanData.Checkpoints = new List<KeyValuePair<string, int>>
                        {
                            new KeyValuePair<string, int>("A8 01 75", 30),
                            new KeyValuePair<string, int>("83 7C 24 ?? 00", 70),
                        };

                        address = TMemory.ScanAdvanced(ProcessInstance, scanData);
                    }

                    if (address == 0)
                    {
                        ScanData scanData = new ScanData();
                        scanData.Signature = "48 83 EC ?? 8B 41 08 48 8D 71 08 C1 E8 0F 33 ED 48 8B F9 A8 01 0F 85";
                        scanData.FindStartFunction = true;

                        address = TMemory.ScanAdvanced(ProcessInstance, scanData);
                    }

                    if (address == 0)
                    {
                        ScanData scanData = new ScanData();
                        scanData.Signature = "FF 25 00 00 00 00";
                        scanData.FindStartFunction = true;

                        scanData.Checkpoints = new List<KeyValuePair<string, int>>
                        {
                            new KeyValuePair<string, int>("A8 01 0F 85", 50),
                        };

                        address = TMemory.ScanAdvanced(ProcessInstance, scanData);
                    }

                    return address;
                }

                else if (function == Function.UObjectProcessEvent)
                {
                    ulong address = 0;

                    if (address == 0)
                    {
                        ScanData scanData = new ScanData();
                        scanData.Signature = "40 55 56 57 41 54 41 55 41 56 41 57 48 81 EC ?? ?? 00 00 48 8D 6C 24 ?? 48 89 9D ?? ?? 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C5 48 89 85 ?? 00 00 00";

                        scanData.Checkpoints = new List<KeyValuePair<string, int>>
                        {
                            new KeyValuePair<string, int>("F7 82 ?? 00 00 00 00 ?? 00 00", 150),
                        };

                        address = TMemory.ScanAdvanced(ProcessInstance, scanData);
                    }

                    if (address == 0)
                    {
                        ScanData scanData = new ScanData();
                        scanData.Signature = "40 55 56 57 41 54 41 55 41 56 41 57 48 81 EC ?? ?? 00 00 48 8D 6C 24 ?? 48 89 9D ?? ?? 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C5 48 89 85 ?? 00 00 00";

                        scanData.Checkpoints = new List<KeyValuePair<string, int>>
                        {
                            new KeyValuePair<string, int>("F7 86 ?? 00 00 00 ?? ?? 00 00", 185),
                        };

                        address = TMemory.ScanAdvanced(ProcessInstance, scanData);
                    }

                    if (address == 0)
                    {
                        ScanData scanData = new ScanData();
                        scanData.Signature = "FF 25 00 00 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 48 8D 6C 24 ?? 48 89 9D ?? ?? 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C5 48 89 85 ?? 00 00 00";

                        scanData.Checkpoints = new List<KeyValuePair<string, int>>
                        {
                            new KeyValuePair<string, int>("F7 82 ?? 00 00 00 00 ?? 00 00", 150),
                        };

                        address = TMemory.ScanAdvanced(ProcessInstance, scanData);
                    }

                    if (address == 0)
                    {
                        ScanData scanData = new ScanData();
                        scanData.Signature = "FF 25 00 00 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 48 8D 6C 24 ?? 48 89 9D ?? ?? 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C5 48 89 85 ?? 00 00 00";

                        scanData.Checkpoints = new List<KeyValuePair<string, int>>
                    {
                        new KeyValuePair<string, int>("F7 86 ?? 00 00 00 ?? ?? 00 00", 185),
                    };

                        address = TMemory.ScanAdvanced(ProcessInstance, scanData);
                    }

                    return address;
                }

                else if (function == Function.UMovieSceneSequencePlayer_Update)
                {
                    ulong address = 0;

                    if (address == 0)
                    {
                        ScanData scanData = new ScanData();
                        scanData.Signature = "48 ?? ?? FF ?? ?? ?? 00 00 F3 0F 59 F0";
                        scanData.FindStartFunction = true;

                        scanData.Checkpoints = new List<KeyValuePair<string, int>>
                        {
                            new KeyValuePair<string, int>("F3 0F 11 3B C6 43 04 01", 0x140),
                        };

                        address = TMemory.ScanAdvanced(ProcessInstance, scanData);
                    }

                    if (address == 0)
                    {
                        ScanData scanData = new ScanData();
                        scanData.Signature = "80 BB ?? ?? ?? ?? 01 0F 85 ?? ?? ?? ?? F6 83";
                        scanData.ReversedSearch = true;
                        scanData.QueenCheckpointIndex = 1;

                        scanData.Checkpoints = new List<KeyValuePair<string, int>>
                        {
                            new KeyValuePair<string, int>("?? ?? 48 83 EC ?? 48 8B 41 28 , FF 25 00 00 00 00", 165),
                        };

                        address = TMemory.ScanAdvanced(ProcessInstance, scanData);
                    }

                    return address;
                }
            }

            else if (identifier is Data data)
            {
                if (data == Data.FNamePool)
                {
                    ulong address = TMemory.ScanRel2(ProcessInstance, "48 8D 05 ???????? EB ?? 48 8D 0D ???????? E8 ???????? C6 05");
                    if (address == 0) address = TMemory.ScanRel2(ProcessInstance, "8B D9 74 ?? 48 8D 15 ???????? EB", offset: 4);
                    return address;
                }
            }

            return 0;
        }

        public enum Function
        {
            StaticConstructObject_Internal,
            UObjectProcessEvent,
            UObject_BeginDestroy,
            UMovieSceneSequencePlayer_Update
        }

        public enum Data
        {
            FNamePool,
        }
    }
}