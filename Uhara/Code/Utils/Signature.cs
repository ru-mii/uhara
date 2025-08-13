using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class USignature
{
    public class ScanData
    {
        public string Signature { get; set; }
        public bool Relative { get; set; }
        public int ToRelativeInstructionOffset { get; set; }
        public Dictionary<string, int> Checkpoints { get; set; }
        public bool Reversed { get; set; }
        public int Offset { get; set; }

        public ScanData(string signature = null, bool isRelative = false, int toRelativeInstructionOffset = 0, Dictionary<string, int> checkpoints = null, bool reversed = false, int offset = 0)
        {
            Signature = signature;
            Relative = isRelative;
            ToRelativeInstructionOffset = toRelativeInstructionOffset;
            Checkpoints = checkpoints == null ? new Dictionary<string, int>() : checkpoints;
            Reversed = reversed;
            Offset = offset;
        }
    }

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
                if (!Uri.IsHexDigit(byteValue[0]) || !Uri.IsHexDigit(byteValue[1])) return null;
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
}
