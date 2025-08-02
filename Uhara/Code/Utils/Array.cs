using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class UArray
{
    internal static byte[] DecodeAsmBlock(byte[] asmBlock)
    {
        List<byte> decoded = new List<byte>();
        for (int i = 0; i < asmBlock.Length; i++)
            if (i % 2 == 0) decoded.Add(asmBlock[i]);

        return decoded.ToArray();
    }

    internal static void Insert(byte[] destination, byte[] toInsert, int position)
    {
        Array.Copy(toInsert, 0, destination, position, toInsert.Length);
    }

    internal static byte[] Merge(params byte[][] arrays)
    {
        byte[] byteArray = new byte[0];
        foreach (byte[] array in arrays)
            byteArray = byteArray.Concat(array).ToArray();

        return byteArray;
    }

    internal static byte[] Merge(List<byte[]> arrays)
    {
        byte[] byteArray = new byte[0];
        foreach (byte[] array in arrays)
            byteArray = byteArray.Concat(array).ToArray();

        return byteArray;
    }
}
