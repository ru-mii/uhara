using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class UPath
{
    internal static string FindFile(string dir, string fileName)
    {
        if (!Directory.Exists(dir)) { return ""; }
        string[] allFiles = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
        foreach (string file in allFiles)
        {
            if (file.EndsWith(fileName))
                return file;
        }
        return "";
    }
}