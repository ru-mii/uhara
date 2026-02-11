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
        public partial class Default
        {
            public partial class GameObject
            {
                public class Converter
                {
                    #region VARIABLES
                    static int OffsetCachePtr = 0x10;

                    static int OffsetGameObjectManaged;

                    static int OffsetActiveSelf;
                    static int OffsetName;

                    static int OffsetTransform = 0x8;
                    static int OffsetTransformInternal;
                    static int OffsetPosition = 0x90;
                    static int OffsetLocalScale = 0xB0;
                    #endregion

                    #region INTERNAL_API
                    internal GameObjectResolvable InstanceToGameObjectResolvable(ulong instanceAddress, bool isInsideAPointer)
                    {
                        if (instanceAddress == 0) return null;
                        else if (isInsideAPointer) return new GameObjectResolvable(instanceAddress, new int[] { OffsetCachePtr });
                        else return new GameObjectResolvable(instanceAddress + (ulong)OffsetCachePtr, null);
                    }

                    internal GameObjectResolvable GameObjectToGameObjectResolvable(ulong objectAddress, bool isInsideAPointer)
                    {
                        if (objectAddress == 0) return null;
                        else if (isInsideAPointer) return new GameObjectResolvable(objectAddress, new int[0]);
                        else return new GameObjectResolvable(objectAddress, null);
                    }

                    internal bool Initiate()
                    {
                        try
                        {
                            do
                            {
                                if (!FindOffsets()) break;
                                return true;
                            }
                            while (false);
                        }
                        catch { }
                        return false;
                    }
                    #endregion
                    #region PRIVATE_API
                    public class GameObjectResolvable
                    {
                        public ulong StartAddress = 0;
                        public int[] WithinOffsets = new int[0];

                        public Transform transform;

                        public bool active { get { return activeSelf; } }
                        public bool activeSelf
                        {
                            get
                            {
                                try
                                {
                                    do
                                    {
                                        ulong instance = DerefToGameObject();
                                        if (instance == 0) break;

                                        DeepPointer dp = new DeepPointer(
                                            (IntPtr)(instance + (ulong)OffsetGameObjectManaged),
                                            OffsetActiveSelf);

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
                                        ulong instance = DerefToGameObject();
                                        if (instance == 0) break;

                                        DeepPointer dp = new DeepPointer(
                                            (IntPtr)(instance + (ulong)OffsetGameObjectManaged),
                                            OffsetName, 0x0);

                                        return dp.DerefString(Main.ProcessInstance, ReadStringType.ASCII, 128, null);
                                    }
                                    while (false);
                                }
                                catch { }
                                return null;
                            }
                        }

                        public class Transform
                        {
                            public Position position;
                            public LocalScale localScale;

                            private GameObjectResolvable _owner;
                            public Transform(GameObjectResolvable owner)
                            {
                                _owner = owner;
                                position = new Position(this);
                                localScale = new LocalScale(this);
                            }

                            public class Position
                            {
                                public float x
                                {
                                    get
                                    {
                                        try
                                        {
                                            do
                                            {
                                                ulong instance = _owner._owner.DerefToGameObject();
                                                if (instance == 0) break;

                                                DeepPointer dp = new DeepPointer(
                                                    (IntPtr)(instance + (ulong)OffsetGameObjectManaged), OffsetGameObjectManaged,
                                                    OffsetTransform, OffsetTransformInternal, OffsetPosition + 0x0);

                                                return dp.Deref<float>(Main.ProcessInstance, 0);
                                            }
                                            while (false);
                                        }
                                        catch { }
                                        return 0;
                                    }
                                }

                                public float y
                                {
                                    get
                                    {
                                        try
                                        {
                                            do
                                            {
                                                ulong instance = _owner._owner.DerefToGameObject();
                                                if (instance == 0) break;

                                                DeepPointer dp = new DeepPointer(
                                                    (IntPtr)(instance + (ulong)OffsetGameObjectManaged), OffsetGameObjectManaged,
                                                    OffsetTransform, OffsetTransformInternal, OffsetPosition + 0x4);

                                                return dp.Deref<float>(Main.ProcessInstance, 0);
                                            }
                                            while (false);
                                        }
                                        catch { }
                                        return 0;
                                    }
                                }

                                public float z
                                {
                                    get
                                    {
                                        try
                                        {
                                            do
                                            {
                                                ulong instance = _owner._owner.DerefToGameObject();
                                                if (instance == 0) break;

                                                DeepPointer dp = new DeepPointer(
                                                    (IntPtr)(instance + (ulong)OffsetGameObjectManaged), OffsetGameObjectManaged,
                                                    OffsetTransform, OffsetTransformInternal, OffsetPosition + 0x8);

                                                return dp.Deref<float>(Main.ProcessInstance, 0);
                                            }
                                            while (false);
                                        }
                                        catch { }
                                        return 0;
                                    }
                                }

                                private Transform _owner;
                                public Position(Transform owner)
                                {
                                    _owner = owner;
                                }
                            }

                            public class LocalScale
                            {
                                public float x
                                {
                                    get
                                    {
                                        try
                                        {
                                            do
                                            {
                                                ulong gameObject = _owner._owner.DerefToGameObject();
                                                if (gameObject == 0) break;

                                                DeepPointer dp = new DeepPointer(
                                                    (IntPtr)(gameObject + (ulong)OffsetGameObjectManaged), OffsetGameObjectManaged,
                                                    OffsetTransform, OffsetTransformInternal, OffsetLocalScale + 0x0);

                                                return dp.Deref<float>(Main.ProcessInstance, 0);
                                            }
                                            while (false);
                                        }
                                        catch { }
                                        return 0;
                                    }
                                }

                                public float y
                                {
                                    get
                                    {
                                        try
                                        {
                                            do
                                            {
                                                ulong gameObject = _owner._owner.DerefToGameObject();
                                                if (gameObject == 0) break;

                                                DeepPointer dp = new DeepPointer(
                                                    (IntPtr)(gameObject + (ulong)OffsetGameObjectManaged), OffsetGameObjectManaged,
                                                    OffsetTransform, OffsetTransformInternal, OffsetLocalScale + 0x4);

                                                return dp.Deref<float>(Main.ProcessInstance, 0);
                                            }
                                            while (false);
                                        }
                                        catch { }
                                        return 0;
                                    }
                                }

                                public float z
                                {
                                    get
                                    {
                                        try
                                        {
                                            do
                                            {
                                                ulong gameObject = _owner._owner.DerefToGameObject();
                                                if (gameObject == 0) break;

                                                DeepPointer dp = new DeepPointer(
                                                    (IntPtr)(gameObject + (ulong)OffsetGameObjectManaged), OffsetGameObjectManaged,
                                                    OffsetTransform, OffsetTransformInternal, OffsetLocalScale + 0x8);

                                                return dp.Deref<float>(Main.ProcessInstance, 0);
                                            }
                                            while (false);
                                        }
                                        catch { }
                                        return 0;
                                    }
                                }

                                private Transform _owner;
                                public LocalScale(Transform owner)
                                {
                                    _owner = owner;
                                }
                            }
                        }

                        public ulong DerefToGameObject()
                        {
                            if (WithinOffsets == null) return StartAddress;
                            else
                            {
                                DeepPointer dp = new DeepPointer((IntPtr)StartAddress, WithinOffsets);
                                return dp.Deref<ulong>(Main.ProcessInstance, 0);
                            }
                        }

                        public GameObjectResolvable(ulong instanceAddress, int[] withinOffsets)
                        {
                            StartAddress = instanceAddress;
                            WithinOffsets = withinOffsets;
                            transform = new Transform(this);
                        }
                    }
                    private bool FindOffsets()
                    {
                        bool success = false;
                        try
                        {
                            do
                            {
                                {
                                    ulong result = TMemory.ScanSingle(Main.ProcessInstance,
                                    "FF 90 ?? ?? ?? ?? 84 C0 0F 84 ?? ?? ?? ?? 48 8B ?? ?? 48 85 C9 0F 84 ?? ?? ?? ?? E8 ?? ?? ?? ?? 84 C0 0F 84",
                                    "UnityPlayer.dll", 0x20); if (result == 0) break;

                                    byte offset = TMemory.ReadMemoryBytes(Main.ProcessInstance, result + 0x11, 1)[0];
                                    if (offset != 0)
                                    {
                                        OffsetGameObjectManaged = offset;
                                        OffsetTransformInternal = OffsetGameObjectManaged + 0x8;
                                    }
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
                }
            }
        }
    }
}