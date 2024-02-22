using System.IO.Hashing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace CsToml.Utility;

internal class ByteArrayHash
{
    private static readonly int Seed;

    static ByteArrayHash()
    {
        Span<byte> seedBuffer = stackalloc byte[4];
        RandomNumberGenerator.Fill(seedBuffer);
        Seed = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference<byte>(seedBuffer));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToInt32(ReadOnlySpan<byte> span)
        => (int)XxHash32.HashToUInt32(span, Seed);
}

