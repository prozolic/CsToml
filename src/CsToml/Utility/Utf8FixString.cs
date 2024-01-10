using CsToml.Debugger;
using CsToml.Formatter;
using System.Diagnostics;
using System.IO.Hashing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace CsToml.Utility;

[DebuggerTypeProxy(typeof(Utf8FixStringDebugView))]
[DebuggerDisplay("{Utf16String}")]
internal readonly struct Utf8FixString : IEquatable<Utf8FixString>
{
    private readonly byte[] bytes;

    internal Span<byte> BytesSpan => bytes.AsSpan();

    public int Length => BytesSpan.Length;

    public string Utf16String
    {
        get
        {
            var tempReader = new Utf8Reader(BytesSpan);
            return StringFormatter.Deserialize(ref tempReader, tempReader.Length);
        }
    }

    public Utf8FixString(ReadOnlySpan<byte> rawBytes)
    {
        bytes = rawBytes.ToArray();
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        return Equals((Utf8FixString)obj);
    }

    public bool Equals(Utf8FixString other)
    {
        if (Length != other.Length) return false;
        if (Length == 0) return true;

        return BytesSpan.SequenceEqual(other.BytesSpan);
    }

    public bool Equals(ReadOnlySpan<byte> other)
    {
        if (other == null) return false;
        if (Length != other.Length) return false;
        if (Length == 0) return true;

        return BytesSpan.SequenceEqual(other);
    }

    public override int GetHashCode()
        => Hash.ToUInt32(BytesSpan);

    private readonly struct Hash
    {
        private static readonly int Seed;

        static Hash()
        {
            Span<byte> seedBuffer = stackalloc byte[4];
            RandomNumberGenerator.Fill(seedBuffer);
            Seed = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference<byte>(seedBuffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToUInt32(ReadOnlySpan<byte> span)
            => (int)XxHash32.HashToUInt32(span, Seed);
    }
}

