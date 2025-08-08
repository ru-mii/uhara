using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static USignature;

internal class Signatures
{
    public enum GameEngine
    {
        Unity,
        UnrealEngine
    }

    internal class UnrealEngine
    {
        internal static AdvancedSignature Get(Enum identifier, string version)
        {
            if (identifier is Function function)
            {
                if (function == Function.StaticConstructObject_Internal)
                {
                    if (false) { }
                    else if (version == "4.27.2.0") return new AdvancedSignature("48 89 5C 24 10 48 89 74 24 18 55 57 41 54 41 56 41 57 48 8D AC 24 50 FF FF FF 48");
                    else if (version == "5.1.1.0") return new AdvancedSignature("48 89 5C 24 18 55 56 57 41 56 41 57 48 8D AC 24 80 FE FF FF 48 81 EC");
                    else if (version == "5.5.4.0") return new AdvancedSignature("4C 8B DC 49 89 5B 20 55 56 57 41 55 41 56 49 8D AB 48 FE FF FF 48");
                    else if (version == "5.6.0.0") return new AdvancedSignature("4C 8B DC 55 53 41 56 49 8D AB 28 FE FF FF 48 81 EC C0 02 00 00 48");
                }
                else if (function == Function.UObjectBeginDestroy)
                {
                    if (false) { }
                    else if (version == "4.27.2.0") return new AdvancedSignature("40 53 48 83 EC 30 8B 41 08 48 8B D9 C1 E8 0F");
                    else if (version == "5.1.1.0") return new AdvancedSignature("40 53 48 83 EC 30 8B 41 08 48 8B D9 C1 E8 0F");
                    else if (version == "5.5.4.0") return new AdvancedSignature("40 53 48 83 EC 30 8B 41 08 48 8B D9 C1 E8 0F");
                    else if (version == "5.6.0.0") return new AdvancedSignature("40 53 48 83 EC 30 8B 41 08 48 8B D9 C1 E8 0F");
                }
            }

            else if (identifier is Data data)
            {
                if (data == Data.FNamePool)
                {
                    if      (version == "4.27.2.0") return new AdvancedSignature("48 8D 05 ???????? EB ?? 48 8D 0D ???????? E8 ???????? C6 05", true);
                    else if (version == "5.1.1.0") return new AdvancedSignature("8B D9 74 ?? 48 8D 15 ???????? EB", true, 4);
                    else if (version == "5.5.4.0") return new AdvancedSignature("8B D9 74 ?? 48 8D 15 ???????? EB", true, 4);
                    else if (version == "5.6.0.0") return new AdvancedSignature("8B D9 74 ?? 48 8D 15 ???????? EB", true, 4);
                }
            }

            return null;
        }

        public enum Function
        {
            StaticConstructObject_Internal,
            UObjectBeginDestroy
        }

        public enum Data
        {
            FNamePool,
        }
    }
}