using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class UArray
{
    public static void Insert(byte[] destination, byte[] toInsert, int position)
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
}
