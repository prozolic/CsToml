
using System.Buffers;

namespace CsToml.Values;

internal sealed class CsTomlValueResolver
{
    public static bool TryResolve(ref readonly dynamic? value, out CsTomlValue? csTomlValue)
    {
        if (value == null)
        {
            csTomlValue = default;
            return false;
        }

        switch (value.GetType())
        {
            case var t when t == typeof(bool):
                csTomlValue = new CsTomlBool(value);
                return true;
            case var t when t == typeof(byte):
            case var t2 when t2 == typeof(sbyte):
            case var t3 when t3 == typeof(int):
            case var t4 when t4 == typeof(uint):
            case var t5 when t5 == typeof(long):
            case var t6 when t6 == typeof(ulong):
            case var t7 when t7 == typeof(short):
            case var t8 when t8 == typeof(ushort):
                csTomlValue = new CsTomlInt64(value);
                return true;
            case var t when t == typeof(double):
                csTomlValue = new CsTomlDouble(value);
                return true;
            case var t when t == typeof(DateTime):
                csTomlValue = new CsTomlLocalDateTime(value);
                return true;
            case var t when t == typeof(DateTimeOffset):
                csTomlValue = new CsTomlOffsetDateTime(value, true);
                return true;
            case var t when t == typeof(DateOnly):
                csTomlValue = new CsTomlLocalDate(value);
                return true;
            case var t when t == typeof(TimeOnly):
                csTomlValue = new CsTomlLocalTime(value);
                return true;
            case var t when t == typeof(ReadOnlySpan<char>):
            case var t2 when t2 == typeof(string):
                csTomlValue = CsTomlString.Parse(value);
                return true;
        }

        csTomlValue = default;
        return false;
    }
}

