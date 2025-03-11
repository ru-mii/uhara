using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

class USymbol
{
    internal static UDynamicVars Symbols = new UDynamicVars();
    private static List<UImports.SymbolFunction> symbolFunctions = new List<UImports.SymbolFunction>();

    internal static UImports.SymbolFunction[] GetModuleSymbols(Process process, string moduleName)
    {
        ProcessModule rawModule = UProcess.GetModule(process, moduleName);
        if (rawModule != null)
        {
            UImports.SymbolFunction[] symbolFunctions = GetModuleSymbols(process, rawModule);
            if (symbolFunctions != null) return symbolFunctions;
        }
        return null;
    }

    internal static UImports.SymbolFunction[] GetModuleSymbols(Process process, ProcessModule module)
    {
        UImports.SymbolFunction[] checkArray = (UImports.SymbolFunction[])Symbols[module.ModuleName.ToLower()];
        if (checkArray != null) return checkArray;

        symbolFunctions.Clear();
        IntPtr hProcess = process.Handle;
        if (!UImports.SymInitialize(hProcess, null, false)) return null;
        ulong baseAddress = UImports.SymLoadModuleEx(hProcess, IntPtr.Zero, module.FileName,
            null, module.BaseAddress.ToInt64(), 0, IntPtr.Zero, 0);

        UImports.SYMBOL_INFO symbolInfo = new UImports.SYMBOL_INFO();
        symbolInfo.SizeOfStruct = (uint)Marshal.SizeOf(typeof(UImports.SYMBOL_INFO));
        symbolInfo.MaxNameLen = 1024;

        UImports.SymEnumSymbols(hProcess, baseAddress, "*", SymbolCallback, IntPtr.Zero);
        UImports.SymbolFunction[] toReturn = symbolFunctions.ToArray();

        if (toReturn.Length > 0)
        {
            Symbols[module.ModuleName.ToLower()] = symbolFunctions.ToArray();
            return toReturn;
        }
        return null;
    }

    private static bool SymbolCallback(ref UImports.SYMBOL_INFO pSymInfo, uint SymbolSize, IntPtr UserContext)
    {
        symbolFunctions.Add(new UImports.SymbolFunction(pSymInfo.Name, pSymInfo.Address));
        return true;
    }

    internal static ulong GetSymbolAddress(Process process, string moduleName, string functionName)
    {
        UImports.SymbolFunction[] checkArray = GetModuleSymbols(process, moduleName);
        if (checkArray != null)
        {
            foreach (UImports.SymbolFunction symF in symbolFunctions)
            {
                if (symF.name.ToLower() == functionName.ToLower())
                {
                    return symF.address;
                }
            }
        }

        return 0;
    }
}
