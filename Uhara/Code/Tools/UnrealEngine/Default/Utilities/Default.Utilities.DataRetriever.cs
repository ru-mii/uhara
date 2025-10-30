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
							ulong data = 0;

							if (false) { }

							else if (dataNameLower == "gengine" || dataNameLower == "engine" || dataNameLower == "gameengine")
							{
								data = TMemory.ScanRel(ProcessInstance, 3, "48 89 05 ???????? 48 85 C9 74 ?? E8 ???????? 48 8D 4D");
								if (data == 0) TMemory.ScanRel(ProcessInstance, 8, "E8 ???????? 48 8B 0D ???????? 49 8B ?? 48 8B 01 FF 90 ???????? 48 8D");
							}

							else if (dataNameLower == "gworld" || dataNameLower == "world")
							{
								data = TMemory.ScanRel(ProcessInstance, 3, "48 8B 05 ???????? 48 3B C? 48 0F 44 C? 48 89 05 ???????? E8");
							}

							else if (dataNameLower == "fnamepool" || dataNameLower == "fnames")
							{
								data = TMemory.ScanRel(ProcessInstance, 7, "8B D9 74 ?? 48 8D 15 ???????? EB");
								if (data == 0) data = TMemory.ScanRel(ProcessInstance, 3, "48 8D 05 ???????? EB ?? 48 8D 0D ???????? E8 ???????? C6 05");
								if (data == 0) data = TMemory.ScanRel(ProcessInstance, 3, "48 8B ?????????? 41 0F B7 C4");
							}

							else if (dataNameLower == "gsync" || dataNameLower == "gsyncload" || dataNameLower == "gsyncloadcount")
							{
								data = TMemory.ScanRel(ProcessInstance, 5, "89 43 60 8B 05");
							}

							else
							{
								TUtils.Print(DebugClass + "." + GetType().Name + "." +
								MethodBase.GetCurrentMethod().Name + " | " + "Data name not supported: " + dataName);
								return IntPtr.Zero;
							}

							// ---
							if (data == 0) TUtils.Print(DebugClass + "." + GetType().Name + "." +
									MethodBase.GetCurrentMethod().Name + " | " + "Couldn't find data: " + dataName);

							return (IntPtr)data;
						}
						catch { }
						return IntPtr.Zero;
					}
				}
			}
		}
	}
}