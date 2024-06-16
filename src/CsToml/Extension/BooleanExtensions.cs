using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CsToml.Extension;

internal static class BooleanExtensions
{
    public static bool TryParse(ReadOnlySpan<byte> bytes, out bool value)
    {
        if (bytes.Length == 4)
        {
            var trueValue = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference<byte>(bytes));
            if (trueValue == 1702195828) // true
            {
                value = true;
                return true;
            }
        }
        if (bytes.Length == 5)
        {
            var falseValue = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference<byte>(bytes));
            if (falseValue == 1936482662 // fals
                && bytes[4] == CsTomlSyntax.AlphaBet.e) // e
            {
                value = false;
                return true;
            }
        }

        if (bytes.Length < 4)
        {
            value = default;
            return false;
        }

        var trimValue = bytes.TrimWhiteSpace();
        if (trimValue.Length == 4)
        {
            var trueValue = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference<byte>(trimValue));
            if (trueValue == 1702195828) // true
            {
                value = true;
                return true;
            }
        }
        if (trimValue.Length == 5)
        {
            var falseValue = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference<byte>(trimValue));
            if (falseValue == 1936482662 // fals
                && trimValue[4] == CsTomlSyntax.AlphaBet.e) // e
            {
                value = false;
                return true;
            }
        }

        value = default;
        return false;
    }

}

