using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class UProgram : UShared
{
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

    internal static void Print(string message)
    {
        if(DebugMode) UImports.OutputDebugString("[UHARA] " + message);
    }
}