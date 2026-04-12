using SharpDisasm;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public partial class Tools
{
	public partial class Unity
	{
		public partial class Utilities
		{
			private class SceneManager
			{
				#region VARIABLES
				bool IsLoaded = false;

				ulong SceneManagerPtr = 0;
				string LastCurrentSceneName = null;
				string LastLoadingSceneName = null;
                int ConfirmedNameOffset = -1;
                #endregion

                #region INTERNAL_API
				internal string[] GetAllSceneNames()
				{
                    try
                    {
                        do
                        {
							string exePath = Main.ProcessInstance.MainModule.FileName;
                            if (string.IsNullOrEmpty(exePath)) break;

                            string exeDir = Path.GetDirectoryName(exePath);

							string globalgamemanagersPath = TPath.FindFile(exeDir, "globalgamemanagers");
							if (string.IsNullOrEmpty(globalgamemanagersPath)) break;

							byte[] globalgamemanagersBytes = File.ReadAllBytes(globalgamemanagersPath);

							byte[] searchStartBytes = TSignature.GetBytes("41 73 73 65 74 73 2F 53 63 65 6E 65 73 2F");
							byte[] searchEndBytes = TSignature.GetBytes("2E 75 6E 69 74 79");

							HashSet<string> sceneNames = new HashSet<string>();

							int offset = 0;
							while (true)
							{
								offset = TMemory.FindInArray(globalgamemanagersBytes, searchStartBytes, startPosition: offset);
								if (offset < 1) break;

								int offsetEnd = TMemory.FindInArray(globalgamemanagersBytes, searchEndBytes, startPosition: offset);
								if (offsetEnd < 1)
								{
                                    offset += searchStartBytes.Length;
									continue;
                                }

								if (offsetEnd <= offset) break;

								byte[] extractNameBytes = TArray.Extract(globalgamemanagersBytes, offset, offsetEnd - offset);
								string sceneNamePath = TUtils.MultibyteToString(extractNameBytes);
								string sceneName = Path.GetFileNameWithoutExtension(sceneNamePath);

								sceneNames.Add(sceneName);
								offset += searchStartBytes.Length;
                            }

							return sceneNames.ToArray();
                        }
                        while (false);
                    }
                    catch { }
					return null;
                }

                internal string GetCurrentSceneName()
				{
					try
					{
						do
						{
							if (!IsLoaded) break;

							ulong address = TMemory.ReadMemory<ulong>(Main.ProcessInstance, SceneManagerPtr);
							if (address == 0) break;

							address = TMemory.DerefPointer(Main.ProcessInstance, SceneManagerPtr, 0x50, 0x0);
							if (address == 0) break;

							string name = ReadSceneName(address);
							if (string.IsNullOrEmpty(name)) break;

							LastCurrentSceneName = name;
							return name;
						}
						while (false);
					}
					catch { }
					return LastCurrentSceneName;
				}

				internal string GetLoadingSceneName()
				{
					try
					{
						do
						{
							if (!IsLoaded) break;

							ulong address = TMemory.ReadMemory<ulong>(Main.ProcessInstance, SceneManagerPtr);
							if (address == 0) break;

							int loadingIndex = TMemory.ReadMemory<int>(Main.ProcessInstance, address + 0x18);
							if (loadingIndex <= 0) break;

							loadingIndex -= 1;

							address = TMemory.ReadMemory<ulong>(Main.ProcessInstance, address + 0x8);
							if (address == 0) break;

							address = TMemory.ReadMemory<ulong>(Main.ProcessInstance, address + (ulong)(loadingIndex * 8));
							if (address == 0) break;

							string name = ReadSceneName(address);
							if (string.IsNullOrEmpty(name)) break;

							LastLoadingSceneName = name;
							return name;
						}
						while (false);
					}
					catch { }
					return LastLoadingSceneName;
				}
				#endregion
				#region PRIVATE_API
				private string ReadSceneName(ulong scene)
				{
					do
					{
						string name = null;

						if (ConfirmedNameOffset == -1)
						{
							name = ReadSceneName(scene, 0x10);
							if (name != null) ConfirmedNameOffset = 0x10;
							else
							{
                                name = ReadSceneName(scene, 0x18);
                                if (name != null) ConfirmedNameOffset = 0x18;
                            }
                        }
						else name = ReadSceneName(scene, (uint)ConfirmedNameOffset);

						// ---
						return name;
					}
					while (false);
				}

                private string ReadSceneName(ulong scene, uint nameOffset)
                {
                    do
                    {
                        if (scene == 0) break;

                        // Assets/
                        byte[] assetsBytes = new byte[] { 0x41, 0x73, 0x73, 0x65, 0x74, 0x73, 0x2F };
						ulong namePtr = scene + nameOffset;

                        byte[] readBytes = TMemory.ReadMemoryBytes(Main.ProcessInstance, namePtr, assetsBytes.Length);
                        if (readBytes == null || readBytes.Length != assetsBytes.Length) break;

                        if (!assetsBytes.SequenceEqual(readBytes))
                        {
                            namePtr = TMemory.ReadMemory<ulong>(Main.ProcessInstance, namePtr);
							if (namePtr < 0x1000) break;

                            readBytes = TMemory.ReadMemoryBytes(Main.ProcessInstance, namePtr, assetsBytes.Length);
                            if (readBytes == null || readBytes.Length != assetsBytes.Length) break;
                            if (!assetsBytes.SequenceEqual(readBytes)) break;
                        }

                        // ---
                        string name = ConvertToShortName(TMemory.ReadMemoryString(Main.ProcessInstance, namePtr, 128));
                        if (string.IsNullOrEmpty(name)) break;

                        // ---
                        return name;
                    }
                    while (false);
                    return null;
                }

                private string ConvertToShortName(string name)
				{
					try
					{
						do
						{
							if (!IsLongNameCorrect(name)) break;
							name = name.Substring(name.LastIndexOf("/") + 1);

							if (name.EndsWith(".unity"))
							{
								int indexOf = name.LastIndexOf(".unity");
								name = name.Remove(indexOf);
							}

							return name;
						}
						while (false);
					}
					catch { }
					return null;
				}

				private bool IsLongNameCorrect(string name)
				{
					try
					{
						do
						{
							if (string.IsNullOrEmpty(name)) break;
							if (!name.StartsWith("Assets/")) break;

							return true;
						}
						while (false);
					}
					catch { }
					return false;
				}
				#endregion

				#region FIND_SCENE_MANAGER
				private bool FindSceneManager()
				{
					bool success = false;
					try
					{
						if (!success)
						{
							try
							{
								do
								{
									ulong result = TMemory.ScanSingle(Main.ProcessInstance,
										"48 C7 43 ?? 00 00 80 3F 48 8B 5C 24 30 48 83 C4 20 5F C3", "UnityPlayer.dll", 0x20);

                                    if (result == 0) result = TMemory.ScanSingle(Main.ProcessInstance,
                                        "48 C7 43 ?? 00 00 80 3F 48 8B 5C 24 30 48 83 C4 20 5F C3", null, 0x20);

                                    if (result == 0) break;
									result = TMemory.GetFunctionStart(Main.ProcessInstance, result);

									// ---
									{
										byte[] checkBytes1 = TMemory.ReadMemoryBytes(Main.ProcessInstance, result, 13);
										byte[] checkBytes2 = new byte[] { 0x48, 0x89, 0x5C, 0x24, 0x08, 0x57, 0x48, 0x83, 0xEC, 0x20, 0x48, 0x8B, 0xD9 };
										if (!checkBytes1.SequenceEqual(checkBytes2)) break;
									}

									// ---
									{
										result += 13;
										Instruction ins = TInstruction.GetInstruction2(Main.ProcessInstance, result);

										if (ins.ToString().Contains(", [") && ins.Bytes.Length == 7)
										{
											int value = TMemory.ReadMemory<int>(Main.ProcessInstance, result + 3);
											SceneManagerPtr = (ulong)((long)result + value + 7);

											// ---
											success = true;
											break;
										}

										else if (ins.ToString().StartsWith("call") && ins.Length == 5)
										{
											int value = TMemory.ReadMemory<int>(Main.ProcessInstance, result + 1);
											result = (ulong)((long)result + value + 5);

											ins = TInstruction.GetInstruction2(Main.ProcessInstance, result);
											if (!ins.ToString().Contains(", [") || ins.Bytes.Length != 7) break;

											value = TMemory.ReadMemory<int>(Main.ProcessInstance, result + 3);
											SceneManagerPtr = (ulong)((long)result + value + 7);

											// ---
											success = true;
											break;
										}
									}
								}
								while (false);
							}
							catch { }
						}

						if (!success)
						{
							try
							{
								do
								{
									ulong result = TMemory.ScanSingle(Main.ProcessInstance, "48 8B 05 ?? ?? ?? ?? 48 8B D1 48 83 78 48 00 74 0A 48 8B 40 48", "UnityPlayer.dll", 0x20);
									if (result == 0) break;

									// ---
									int value = TMemory.ReadMemory<int>(Main.ProcessInstance, result + 3);
									SceneManagerPtr = (ulong)((long)result + value + 7);

									// ---
									success = true;
									break;
								}
								while (false);
							}
							catch { }
						}
					}
					catch { }

                    if (success) TUtils.Print("Unity.Utils | SceneManager loaded successfuly");
                    else TUtils.Print("Unity.Utils | SceneManager loading failed");
                    return success;
				}
				#endregion

				#region CONSTRUCTOR
				public SceneManager()
				{
					try
					{
						do
						{
							if (!FindSceneManager()) break;

							IsLoaded = true;
						}
						while (false);
					}
					catch { }
				}
				#endregion
			}
		}
	}
}