using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class TUtils : MainShared
{
    internal static string MultibyteToString(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return string.Empty;

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < bytes.Length && bytes[i] != 0; i++)
        {
            sb.Append((char)bytes[i]);
        }

        return sb.ToString();
    }

    internal static string MultibyteToString2(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return string.Empty;

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] < 32 || bytes[i] > 126) break;
            else sb.Append((char)bytes[i]);
        }

        sb.Append(0);
        return sb.ToString();
    }

    internal static string GetFileVersion(string path)
    {
        if (File.Exists(path))
        {
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(path);
            if (versionInfo.FileVersion != null)
            {
                return
                    versionInfo.FileMajorPart + "." +
                    versionInfo.FileMinorPart + "." +
                    versionInfo.FileBuildPart + "." +
                    versionInfo.FilePrivatePart;
            }
        }

        return null;
    }

    internal static string GenerateRandomString(int length)
    {
        string text = "";
        while (text.Length <= length)
            text += Guid.NewGuid().ToString("N");

        text = text.Remove(length);
        return text;
    }

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

    internal static bool Print(string message)
    {
        if (DebugMode) TImports.OutputDebugString("[UHARA] " + message);
        return false;
    }
}