using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class TConvert
{
    public static T Parse<T>(string number, bool forceHex = false)
    {
        try
        {
            if (string.IsNullOrEmpty(number)) return default;

            NumberStyles numberStyle = NumberStyles.Number;
            number = number.ToLower().Replace(",", ".").Replace(" ", "");

            int multiplier = 1;
            if (number[0] == '-')
            {
                multiplier = -1;
                number = number.Remove(0, 1);
            }
            else if (number[0] == '+')
            {
                number = number.Remove(0, 1);
            }

            if (number.StartsWith("0x"))
            {
                numberStyle = NumberStyles.HexNumber;
                number = number.Replace("0x", "");
            }

            if (forceHex) numberStyle = NumberStyles.HexNumber;

            if (typeof(T) == typeof(sbyte))
            {
                sbyte val = 0; if (sbyte.TryParse(number, numberStyle, CultureInfo.InvariantCulture, out val))
                    return (T)(object)(val * multiplier);
            }
            else if (typeof(T) == typeof(byte))
            {
                byte val = 0; if (byte.TryParse(number, numberStyle, CultureInfo.InvariantCulture, out val))
                    return (T)(object)(val * multiplier);
            }
            else if (typeof(T) == typeof(int))
            {
                int val = 0; if (int.TryParse(number, numberStyle, CultureInfo.InvariantCulture, out val))
                    return (T)(object)(val * multiplier);
            }
            else if (typeof(T) == typeof(uint))
            {
                uint val = 0; if (uint.TryParse(number, numberStyle, CultureInfo.InvariantCulture, out val))
                    return (T)(object)(uint)(val * multiplier);
            }
            else if (typeof(T) == typeof(long))
            {
                long val = 0; if (long.TryParse(number, numberStyle, CultureInfo.InvariantCulture, out val))
                    return (T)(object)(val * multiplier);
            }
            else if (typeof(T) == typeof(ulong))
            {
                ulong val = 0; if (ulong.TryParse(number, numberStyle, CultureInfo.InvariantCulture, out val))
                    return (T)(object)(ulong)((long)val * multiplier);
            }
            else if (typeof(T) == typeof(float))
            {
                float val = 0; if (float.TryParse(number, numberStyle, CultureInfo.InvariantCulture, out val))
                    return (T)(object)(val * multiplier);
            }
            else if (typeof(T) == typeof(double))
            {
                double val = 0; if (double.TryParse(number, numberStyle, CultureInfo.InvariantCulture, out val))
                    return (T)(object)(val * multiplier);
            }
        }
        catch { }
        return (T)Convert.ChangeType(0, typeof(T));
    }
}
