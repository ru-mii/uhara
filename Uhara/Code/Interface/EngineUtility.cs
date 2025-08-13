using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static USignature;
using SharpDisasm;

internal class EngineUtility : UShared
{
    public enum GameEngine
    {
        Unity,
        UnrealEngine
    }

    internal class UnrealEngine
    {
        internal static ulong GetAddress(Enum identifier)
        {
            if (identifier is Function function)
            {
                if (function == Function.StaticConstructObject_Internal)
                {
                    ScanData scanData = new ScanData();
                    scanData.Signature = "00 00 00 80 00 00 10";
                    scanData.Reversed = true;

                    scanData.Checkpoints = new Dictionary<string, int>
                    {
                        ["FF FF 48 81 EC"] = 100,
                        ["C3 CC"] = 100
                    };

                    ulong address = UMemory.ScanAdvanced(Instance, scanData);
                    if (address == 0) return 0;

                    byte[] instrBytes = RefReadBytes(Instance, address, 50);
                    if (instrBytes == null) return 0;

                    Instruction[] instrs = UInstruction.GetInstructions2(instrBytes);
                    if (instrs == null) return 0;

                    if (instrs.Length < 2) return 0;

                    int offset = 0;
                    for (int i = 1; i < instrs.Length; i++)
                    {
                        if (instrs[i - 1].ToString() == "int3" && instrs[i].ToString() != "int3")
                        {
                            address += (ulong)offset;
                            return address;
                        }

                        offset += instrs[i].Bytes.Length;
                    }

                    return 0;
                }

                else if (function == Function.UObjectBeginDestroy)
                {
                    ScanData scanData = new ScanData();
                    scanData.Signature = "40 53 48 83 EC ?? 8B 41 08 48 8B D9 C1 E8 0F A8 01 75";

                    scanData.Checkpoints = new Dictionary<string, int>
                    {
                        ["83 7C 24 28 00"] = 150
                    };

                    ulong address = UMemory.ScanAdvanced(Instance, scanData);

                    // maybe already hooked or not found
                    if (address == 0)
                    {
                        scanData.Signature = "FF 25 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 0F A8 01 75";
                        address = UMemory.ScanAdvanced(Instance, scanData);
                    }

                    return address;
                }
            }

            else if (identifier is Data data)
            {
                if (data == Data.FNamePool)
                {
                    ulong address = UMemory.ScanRel(Instance, "48 8D 05 ???????? EB ?? 48 8D 0D ???????? E8 ???????? C6 05");
                    if (address == 0) address = UMemory.ScanRel(Instance, "8B D9 74 ?? 48 8D 15 ???????? EB", offset: 4);
                    return address;
                }
            }

            return 0;
        }

        public enum Function
        {
            StaticConstructObject_Internal,
            UObjectBeginDestroy
        }

        public enum Data
        {
            FNamePool,
        }
    }
}