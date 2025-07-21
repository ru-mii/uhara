using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class USignature
{
    internal static string GetSignature(byte[] array)
    {
        string hex = BitConverter.ToString(array);
        return hex.Replace("-", " ");
    }

    internal static byte[] GetBytes(string signature)
    {
        signature = signature.Replace(" ", "").Replace("!", "").ToUpper();
        byte[] byteArray = new byte[signature.Length / 2];

        for (int i = 0; i < byteArray.Length; i++)
        {
            string byteValue = signature.Substring(i * 2, 2);

            if (byteValue != "??")
            {
                if (!IsCharHex(byteValue[0]) || !IsCharHex(byteValue[1])) return null;
                else byteArray[i] = Convert.ToByte(byteValue, 16);
            }
            else byteArray[i] = 0x00;
        }

        return byteArray;
    }

    internal static string GetMask(string signature)
    {
        string mask = "";
        signature = signature.Replace(" ", "");
        if (signature.Replace("!", "").Length % 2 == 0)
        {
            for (int i = 0; i < signature.Length; i++)
            {
                if (signature[i] == '!')
                {
                    mask += "!";
                    i += 1;
                }
                else if (signature[i] == '?') mask += "?";
                else mask += "x"; i += 1;
            }
        }

        return mask;
    }

    private static bool IsCharHex(char character)
    {
        if (!((character >= '0' && character <= '9') ||
        (character >= 'A' && character <= 'F') ||
        (character >= 'a' && character <= 'f')))
        {
            return false;
        }
        return true;
    }
}
