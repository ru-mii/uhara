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
                    if      (version == "5.5.4.0") return new AdvancedSignature("4C 8B DC 49 89 5B 20 55 56 57 41 55 41 56 49");
                    else if (version == "5.6.0.0") return new AdvancedSignature("4C 8B DC 55 53 41 56 49 8D AB 28 FE FF FF 48");
                }
                else if (function == Function.UObjectBeginDestroy)
                {
                    if      (version == "5.5.4.0") return new AdvancedSignature("40 53 48 83 EC 30 8B 41 08 48 8B D9 C1 E8 0F");
                    else if (version == "5.6.0.0") return new AdvancedSignature("40 53 48 83 EC 30 8B 41 08 48 8B D9 C1 E8 0F");
                }
            }

            else if (identifier is Data data)
            {
                if (data == Data.FNamePool)
                {
                    if      (version == "5.5.4.0") return new AdvancedSignature("8B D9 74 ?? 48 8D 15 ???????? EB", true, 4);
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