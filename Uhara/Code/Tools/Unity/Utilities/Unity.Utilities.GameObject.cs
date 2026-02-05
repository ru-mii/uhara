using LiveSplit.ComponentUtil;
using SharpDisasm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public partial class Tools
{
    public partial class Unity
    {
        public partial class Utilities
        {
            public class GameObject
            {
                #region VARIABLES
                bool IsLoaded = false;

                static int OffsetCachePtr = 0x10;
                static int OffsetGameObjectManaged;
                static int OffsetActiveSelf;
                static int OffsetName;
                #endregion

                #region INTERNAL_API
                internal GameObjectResolvable ConvertInstanceToGameObject(IntPtr instanceAddress, bool isWithinAPointer)
                {
                    return ConvertInstanceToGameObject((ulong)instanceAddress, isWithinAPointer);
                }

                internal GameObjectResolvable ConvertInstanceToGameObject(ulong instanceAddress, bool isWithinAPointer)
                {
                    return IsLoaded ? new GameObjectResolvable(isWithinAPointer, instanceAddress) : null;
                }
                #endregion

                #region GAME_OBJECT_RESOLVABLE
                public class GameObjectResolvable
                {
                    private bool IsInstanceWithinPointer = false;
                    public ulong InstanceAddress = 0;

                    public bool active { get { return activeSelf; } }
                    public bool activeSelf
                    {
                        get
                        {
                            try
                            {
                                do
                                {
                                    ulong instance = DerefInstanceIfWithinPtr();
                                    if (instance == 0) break;

                                    DeepPointer dp = new DeepPointer((IntPtr)(instance + (ulong)OffsetCachePtr),
                                        OffsetGameObjectManaged, OffsetActiveSelf);

                                    return dp.Deref<byte>(Main.ProcessInstance) == 1;
                                }
                                while (false);
                            }
                            catch { }
                            return false;
                        }
                    }

                    public string name
                    {
                        get
                        {
                            try
                            {
                                do
                                {
                                    ulong instance = DerefInstanceIfWithinPtr();
                                    if (instance == 0) break;

                                    DeepPointer dp = new DeepPointer((IntPtr)(instance + (ulong)OffsetCachePtr),
                                        OffsetGameObjectManaged, OffsetName, 0);

                                    return dp.DerefString(Main.ProcessInstance, ReadStringType.ASCII, 128, null);
                                }
                                while (false);
                            }
                            catch { }
                            return null;
                        }
                    }

                    public ulong DerefInstanceIfWithinPtr()
                    {
                        do
                        {
                            if (IsInstanceWithinPointer)
                            {
                                ulong instance = TMemory.ReadMemory<ulong>(Main.ProcessInstance, InstanceAddress);
                                if (instance == 0) break; else return instance;
                            }
                            else return InstanceAddress;
                        }
                        while (false);
                        return 0;
                    }

                    public GameObjectResolvable(bool isInstanceWithinPointer, ulong instanceAddress)
                    {
                        IsInstanceWithinPointer = isInstanceWithinPointer;
                        InstanceAddress = instanceAddress;
                    }
                }
                #endregion
                #region FIND_OFFSETS
                private bool FindOffsets()
                {
                    bool success = false;
                    try
                    {
                        do
                        {
                            if (LegacyVersion) break;

                            {
                                ulong result = TMemory.ScanSingle(Main.ProcessInstance,
                                "FF 90 ?? ?? ?? ?? 84 C0 0F 84 ?? ?? ?? ?? 48 8B ?? ?? 48 85 C9 0F 84 ?? ?? ?? ?? E8 ?? ?? ?? ?? 84 C0 0F 84",
                                "UnityPlayer.dll", 0x20); if (result == 0) break;

                                byte offset = TMemory.ReadMemoryBytes(Main.ProcessInstance, result + 0x11, 1)[0];
                                if (offset != 0) OffsetGameObjectManaged = offset;
                                else break;
                            }

                            {
                                ulong result = TMemory.ScanSingle(Main.ProcessInstance,
                                "0F 95 C0 48 83 C4 20 5B C3 80 79",
                                "UnityPlayer.dll", 0x20); if (result == 0) break;

                                ulong startFunction = TMemory.GetFunctionStart(Main.ProcessInstance, result);
                                if (startFunction == 0) break;

                                byte[] headerFunction = TMemory.ReadMemoryBytes(Main.ProcessInstance, startFunction, 0x1000);
                                if (headerFunction == null || headerFunction.Length == 0) break;

                                Instruction[] instructions = TInstruction.GetInstructions2(headerFunction, startFunction);
                                if (instructions == null || instructions.Length == 0) break;

                                byte offset = 0;
                                foreach (Instruction ins in instructions)
                                {
                                    string insTxt = ins.ToString();
                                    if ((insTxt.Contains("byte [") || insTxt.Contains("byte ptr [")) &&
                                        insTxt.Contains("+"))
                                    {
                                        string parsed = insTxt.Substring(insTxt.IndexOf("+") + 1);
                                        parsed = parsed.Remove(parsed.IndexOf("]"));
                                        offset = TConvert.Parse<byte>(parsed);
                                        break;
                                    }
                                }

                                if (offset != 0) OffsetActiveSelf = offset;
                                else break;
                            }

                            {
                                OffsetName = (OffsetActiveSelf - OffsetActiveSelf % 4) + 0xC;
                            }

                            success = true;
                        }
                        while (false);
                    }
                    catch { }

                    if (success) TUtils.Print("Unity.Utils | GameObject loaded successfuly");
                    else TUtils.Print("Unity.Utils | GameObject loading failed");
                    return success;
                }
                #endregion

                #region CONSTRUCTOR
                public GameObject()
                {
                    try
                    {
                        do
                        {
                            if (!FindOffsets()) break;

                            IsLoaded = true;
                        }
                        while (false);
                    }
                    catch { }
                }
                #endregion
            }
        }
    }
}