
using System.Runtime.CompilerServices;

namespace CsToml.Extensions.Utility;

internal static class Utf8Helper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainBOM(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length < 3) return false;

        return bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf;
    }
}