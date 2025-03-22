using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class UProgram
{
    internal static byte[] StringToMultibyte(string text)
    {
        List<byte> array = new List<byte>();
        for (int i = 0; i < text.Length; i++)
        {
            array.Add(Convert.ToByte(text[i]));
        }

        array.Add(0);
        return array.ToArray();
    }

    internal static void Print(string message)
    {
        UImports.OutputDebugString(message);
    }
}
