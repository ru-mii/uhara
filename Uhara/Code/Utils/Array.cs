using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class TArray
{
    internal static byte[] DecodeBlock(byte[] asmBlock)
    {
        List<byte> decoded = new List<byte>();
        for (int i = 0; i < asmBlock.Length; i++)
            if (i % 2 == 0) decoded.Add(asmBlock[i]);

        return decoded.ToArray();
    }

    internal static byte[] ExtractArray(byte[] source, int position, int length)
    {
        byte[] newCopy = source.ToList().ToArray();
        newCopy = GutArray(newCopy, 0, position);
        newCopy = GutArray(newCopy, length, newCopy.Length - length);
        return newCopy;
    }

    internal static byte[] GutArray(byte[] original, int position, int length)
    {
        byte[] newArray = new byte[original.Length - length];

        Array.Copy(original, 0, newArray, 0, position);
        Array.Copy(original, position + length, newArray, position, original.Length - position - length);

        return newArray;
    }

    internal static byte[] StuffArray(byte[] original, int position, int length, byte stuffType)
    {
        int newSize = original.Length + length;
        byte[] newArray = new byte[newSize];

        Array.Copy(original, 0, newArray, 0, position);
        for (int i = 0; i < length; i++) newArray[position + i] = stuffType;
        Array.Copy(original, position, newArray, position + length, original.Length - position);

        return newArray;
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
