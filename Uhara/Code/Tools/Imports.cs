﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

class UImports
{
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool ReadProcessMemory(
    IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesRead);

    [DllImport("kernel32.dll")]
    internal static extern void OutputDebugString(string lpOutputString);
}