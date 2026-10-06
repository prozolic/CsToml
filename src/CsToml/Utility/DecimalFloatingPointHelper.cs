#if NET11_0_OR_GREATER

using System.Globalization;
using System.Numerics;

namespace CsToml.Utility;

internal static class DecimalFloatingPointHelper
{
    public static T ConvertFromDouble<T>(double doubleValue)
        where T : IDecimalFloatingPointIeee754<T>
    {
        // Same approach as DecimalFormatter.ConvertToDecimal: a cast would expose the binary expansion of the
        // double (e.g. 3.14 -> 3.140000000000000124344978758017533 for Decimal128) instead of the TOML text digits.

        if (double.IsFinite(doubleValue))
        {
            Span<byte> utf8 = stackalloc byte[32];
            if (doubleValue.TryFormat(utf8, out var bytesWritten, "G", CultureInfo.InvariantCulture) &&
                T.TryParse(utf8.Slice(0, bytesWritten), NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }
        }

        // inf and nan are representable, unlike System.Decimal.
        return T.CreateSaturating(doubleValue);
    }
}

#endif
