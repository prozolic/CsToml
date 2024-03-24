using System.IO.Hashing;
using System.Runtime.CompilerServices;

namespace CsToml.Utility;

internal static class ByteArrayHash
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToInt32(ReadOnlySpan<byte> span)
        => unchecked((int)XxHash3.HashToUInt64(span));
}

