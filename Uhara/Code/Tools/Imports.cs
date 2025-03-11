using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

class UImports
{
    [DllImport("dbghelp.dll", SetLastError = true, CharSet = CharSet.Ansi)]
    public static extern ulong SymLoadModuleEx(IntPtr hProcess, IntPtr hFile, string ImageName, string ModuleName, long BaseOfDll, int DllSize, IntPtr Data, int Flags);

    [DllImport("dbghelp.dll", SetLastError = true)]
    public static extern bool SymInitialize(IntPtr hProcess, string UserSearchPath, bool invadeProcess);

    [StructLayout(LayoutKind.Sequential)]
    internal struct SYMBOL_INFO
    {
        internal uint SizeOfStruct;
        internal uint TypeIndex; // Type index, or 0 if no type.
        internal ulong Reserved1;
        internal ulong Reserved2;
        internal uint Index;
        internal uint Size;
        internal ulong ModBase; // Base address of the module this symbol belongs to.
        internal uint Flags;
        internal ulong Value; // Value of the symbol, or 0 if not a value.
        internal ulong Address; // Address of the symbol including the base address of the module.
        internal uint Register; // Register holding the value or register-based address.
        internal uint Scope; // Scope of the symbol.
        internal uint Tag; // PDB classification.
        internal uint NameLen; // Actual length of the name.
        internal uint MaxNameLen;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        internal string Name;
    }

    [DllImport("dbghelp.dll", CharSet = CharSet.Ansi)]
    internal static extern bool SymEnumSymbols(IntPtr hProcess, ulong BaseOfDll, string Mask, SymEnumSymbolsProc callback, IntPtr userContext);

    internal delegate bool SymEnumSymbolsProc(ref SYMBOL_INFO pSymInfo, uint SymbolSize, IntPtr UserContext);

    internal class SymbolFunction
    {
        internal string name = "";
        internal ulong address = 0x0;

        internal SymbolFunction(string name, ulong address)
        {
            this.name = name;
            this.address = address;
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool ReadProcessMemory(
    IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesRead);

    [DllImport("kernel32.dll")]
    internal static extern void OutputDebugString(string lpOutputString);
}
