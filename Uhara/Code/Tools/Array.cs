using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class UArray
{
    internal static byte[] Merge(params byte[][] arrays)
    {
        byte[] byteArray = new byte[0];
        foreach (byte[] array in arrays)
            byteArray = byteArray.Concat(array).ToArray();

        return byteArray;
    }
}
