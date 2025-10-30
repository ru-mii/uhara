using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class TSignature
{
    public class ScanData
    {
        public string Signature { get; set; }
        public bool Relative { get; set; }
        public int ToRelativeInstructionOffset { get; set; }
        public List<KeyValuePair<string, int>> Checkpoints { get; set; }
        public int QueenCheckpointIndex { get; set; }
        public bool ReversedSearch { get; set; }
        public int Offset { get; set; }
        public bool FindStartFunction { get; set; }

        public ScanData(string signature = null, bool isRelative = false, int toRelativeInstructionOffset = 0, List<KeyValuePair<string, int>> checkpoints = null, int queenCheckpointIndex = 0, bool reversed = false, int offset = 0, bool findStartFunction = false)
        {
            Signature = signature;
            Relative = isRelative;
            ToRelativeInstructionOffset = toRelativeInstructionOffset;
            Checkpoints = checkpoints == null ? new List<KeyValuePair<string, int>>() : checkpoints;
            QueenCheckpointIndex = queenCheckpointIndex;
            ReversedSearch = reversed;
            Offset = offset;
            FindStartFunction = findStartFunction;
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

            if (byteValue == "??") byteArray[i] = 0x00;
            else if (byteValue[0] == '?') byteArray[i] = Convert.ToByte("0" + byteValue[1], 16);
            else if (byteValue[1] == '?') byteArray[i] = Convert.ToByte(byteValue[0] + "0", 16);
            else
            {
                if (!Uri.IsHexDigit(byteValue[0]) || !Uri.IsHexDigit(byteValue[1])) return null;
                else byteArray[i] = Convert.ToByte(byteValue, 16);
            }
        }

        return byteArray;
    }

    internal static string GetMask(string signature)
    {
        string mask = "";
        signature = signature.Replace(" ", "");
        if (signature.Replace("!", "").Length % 2 == 0)
        {
            for (int i = 0; i < signature.Length; i += 2)
            {
                if (signature[i] == '!')
                {
                    mask += "!";
                    i += 1;
                }
                else if (signature[i] == '?' || signature[i+1] == '?') mask += "?";
                else if (signature[i] == '?') mask += "<";
                else if (signature[i+1] == '?') mask += ">";
                else mask += "x";
            }
        }

        return mask;
    }
}
