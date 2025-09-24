using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class Tools : MainShared
{
    public partial class UnrealEngine
    {
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