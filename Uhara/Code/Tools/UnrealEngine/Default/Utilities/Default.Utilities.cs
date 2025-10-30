using System;
using System.Collections.Generic;
using System.Linq;
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
				internal static string DebugClass = "Utilities";
				internal static string ToolUniqueID = "UCyEljVfhjUoJhDU";

				private DataRetriever dataRetriever = new DataRetriever();
				private TextReader textReader = new TextReader();

                #region PUBLIC_API
                public string FNameToStringLegacy(uint fName)
                {
                    try
                    {
                        return textReader.FNameToStringLegacy(fName);
                    }
                    catch { }
                    return null;
                }

                public string FNameToShortStringLegacy(uint fName)
                {
                    try
                    {
                        return textReader.FNameToShortStringLegacy(fName);
                    }
                    catch { }
                    return null;
                }

                public string FNameToShortStringLegacy2(uint fName)
                {
                    try
                    {
                        return textReader.FNameToShortStringLegacy2(fName);
                    }
                    catch { }
                    return null;
                }

                public string FNameToString(uint fName)
				{
					try
					{
						return textReader.FNameToString(fName);
					}
					catch { }
					return null;
				}

                public string FNameToShortString(uint fName)
                {
                    try
                    {
                        return textReader.FNameToShortString(fName);
                    }
                    catch { }
                    return null;
                }

                public string FNameToShortString2(uint fName)
                {
                    try
                    {
                        return textReader.FNameToShortString2(fName);
                    }
                    catch { }
                    return null;
                }

                IntPtr _GEngine = IntPtr.Zero;
				public IntPtr GEngine
				{
					get
					{
						if (_GEngine != IntPtr.Zero) return _GEngine;
						else
						{
							_GEngine = dataRetriever.FindData("GEngine");
							return _GEngine;
						}
					}
				}

				IntPtr _GWorld = IntPtr.Zero;
				public IntPtr GWorld
				{
					get
					{
						if (_GWorld != IntPtr.Zero) return _GWorld;
						else
						{
							_GWorld = dataRetriever.FindData("GWorld");
							return _GWorld;
						}
					}
				}

				IntPtr _FNames = IntPtr.Zero;
				public IntPtr FNames
				{
					get
					{
						if (_FNames != IntPtr.Zero) return _FNames;
						else
						{
							_FNames = dataRetriever.FindData("FNames");
							return _FNames;
						}
					}
				}

				IntPtr _GSync = IntPtr.Zero;
				public IntPtr GSync
				{
					get
					{
						if (_GSync != IntPtr.Zero) return _GSync;
						else
						{
							_GSync = dataRetriever.FindData("GSync");
							return _GSync;
						}
					}
				}

				public IntPtr FindData(string dataName)
				{
					try
					{
						return dataRetriever.FindData(dataName);
					}
					catch { }
					return IntPtr.Zero;
				}
				#endregion

				public Utilities()
				{
					if (!ReloadProcess()) throw new Exception();
					MemoryManager.ClearMemory(ToolUniqueID);
				}
			}
		}
	}
}