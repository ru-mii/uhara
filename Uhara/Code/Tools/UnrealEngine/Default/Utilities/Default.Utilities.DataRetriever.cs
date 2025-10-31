﻿using SharpDisasm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public partial class Tools : MainShared
{
	public partial class UnrealEngine
	{
		public partial class Default
		{
			public partial class Utilities
			{
				public class DataRetriever
				{
					public IntPtr FindData(string dataName)
					{
						try
						{
							string dataNameLower = dataName.ToLower();
							ulong address = 0;

							if (false) { }

							else if (dataNameLower == "gengine" || dataNameLower == "engine" || dataNameLower == "gameengine")
							{
								address = TMemory.ScanRel(ProcessInstance, 3, "48 89 05 ???????? 48 85 C9 74 ?? E8 ???????? 48 8D 4D");
								if (address == 0) TMemory.ScanRel(ProcessInstance, 8, "E8 ???????? 48 8B 0D ???????? 49 8B ?? 48 8B 01 FF 90 ???????? 48 8D");
							}

							else if (dataNameLower == "gworld" || dataNameLower == "world")
							{
								address = TMemory.ScanRel(ProcessInstance, 3, "48 8B 1D ?? ?? ?? ?? 48 85 DB 74 ?? 41 B0 01");
                                if (address == 0) address = TMemory.ScanRel(ProcessInstance, 3, "48 8B 05 ???????? 48 3B C? 48 0F 44 C? 48 89 05 ???????? E8");
							}

							else if (dataNameLower == "fnamepool" || dataNameLower == "fnames")
							{
                                if (address == 0)
                                {
                                    try
                                    {
                                        address = TMemory.ScanRel2(ProcessInstance, "48 8D 05 ???????? EB ?? 48 8D 0D ???????? E8 ???????? C6 05");
                                    }
                                    catch { }
                                }

                                if (address == 0)
                                {
                                    try
                                    {
                                        address = TMemory.ScanRel2(ProcessInstance, "8B D9 74 ?? 48 8D 15 ???????? EB", offset: 4);
                                    }
                                    catch { }
                                }

                                if (address == 0)
                                {
                                    do
                                    {
                                        ulong result = TMemory.ScanSingle(ProcessInstance, "E8 ???????? 4C 8B C8 41 8B ?? 99 81 E2 FF 3F 00 00");
                                        if (result == 0) break;

                                        {
                                            int value = TMemory.ReadMemory<int>(ProcessInstance, result + 1);
                                            result = (ulong)((long)result + value + 5);

                                            byte[] bytes = TMemory.ReadMemoryBytes(ProcessInstance, result, 50);
                                            if (bytes == null) break;

                                            Instruction[] instrs = TInstruction.GetInstructions2(bytes, result);
                                            if (instrs == null || instrs.Length < 2) break;

                                            if (instrs[1].Length != 7) break;
                                            if (!instrs[1].ToString().Contains("rax, [")) break;

                                            result = instrs[1].Offset;

                                            value = TMemory.ReadMemory<int>(ProcessInstance, result + 3);
                                            if (value == 0) break;

                                            address = (ulong)((long)result + value + 7);
                                        }
                                    }
                                    while (false);
                                }
                            }

							else if (dataNameLower == "gsync" || dataNameLower == "gsyncload" || dataNameLower == "gsyncloadcount")
							{
								address = TMemory.ScanRel(ProcessInstance, 5, "89 43 60 8B 05");
							}

							else
							{
								TUtils.Print(DebugClass + "." + GetType().Name + "." +
								MethodBase.GetCurrentMethod().Name + " | " + "Data name not supported: " + dataName);
								return IntPtr.Zero;
							}

							// ---
							if (address == 0) TUtils.Print(DebugClass + "." + GetType().Name + "." +
									MethodBase.GetCurrentMethod().Name + " | " + "Couldn't find data: " + dataName);

							return (IntPtr)address;
						}
						catch { }
						return IntPtr.Zero;
					}
				}
			}
		}
	}
}