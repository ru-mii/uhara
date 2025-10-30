using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static ScanUtility.UnrealEngine;

public partial class Tools : MainShared
{
	public partial class UnrealEngine
	{
		public partial class Default
		{
			public partial class Utilities
			{
				public class TextReader
				{
					bool Loaded = false;
					ulong FNamePool = 0;

					internal string FNameToString(uint fName)
					{
						try
						{
							do
							{
								var nameIdx = (fName & 0x000000000000FFFF) >> 0x00;
								var chunkIdx = (fName & 0x00000000FFFF0000) >> 0x10;
								var number = (fName & 0xFFFFFFFF00000000) >> 0x20;

								IntPtr chunk = (IntPtr)TMemory.ReadMemory<ulong>(ProcessInstance,
									FNamePool + (ulong)(0x10 + (int)chunkIdx * 0x8));

								IntPtr entry = chunk + (int)nameIdx * sizeof(short);
								int length = TMemory.ReadMemory<short>(ProcessInstance, entry) >> 6;
								if (length > byte.MaxValue || length <= 0) break;

								string toReturn = TUtils.MultibyteToString(TMemory.ReadMemoryBytes(ProcessInstance, entry + sizeof(short), length));
								return number == 0 ? toReturn : toReturn + "_" + number;
							}
							while (false);
						}
						catch { }
						return null;
					}

					internal string FNameToShortString(uint fName)
					{
						try
						{
							do
							{
								string name = FNameToString(fName);
								if (string.IsNullOrEmpty(name)) break;

								int dot = name.LastIndexOf('.');
								int slash = name.LastIndexOf('/');

								return name.Substring(Math.Max(dot, slash) + 1);
							}
							while (false);
						}
						catch { }
						return null;
					}

					internal string FNameToShortString2(uint fName)
					{
						try
						{
							do
							{
								string name = FNameToString(fName);
								if (string.IsNullOrEmpty(name)) break;

								int under = name.LastIndexOf('_');
								return name.Substring(0, under + 1);
							}
							while (false);
						}
						catch { }
						return null;
					}

					public TextReader()
					{
						try
						{
							do
							{
								FNamePool = TMemory.ScanRel(ProcessInstance, 7, "8B D9 74 ?? 48 8D 15 ???????? EB");
								if (FNamePool == 0) FNamePool = TMemory.ScanRel(ProcessInstance, 3, "48 8D 05 ???????? EB ?? 48 8D 0D ???????? E8 ???????? C6 05");
								if (FNamePool == 0) FNamePool = TMemory.ScanRel(ProcessInstance, 3, "48 8B ?????????? 41 0F B7 C4");
								if (FNamePool == 0) break;

								// ---
								Loaded = true;
							}
							while (false);
						}
						catch { }
					}
				}
			}
		}
	}
}