
using System.Buffers;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadSequenceWithoutBOMResult TryReadSequenceWithoutBOM(ReadOnlySequence<byte> sequence, out ReadOnlySequence<byte> sequenceWithoutBOM)
    {
        sequenceWithoutBOM = default;
        if (sequence.Length < 3) return ReadSequenceWithoutBOMResult.Failure;

        var reader = new SequenceReader<byte>(sequence);
        if (!reader.TryRead(out var b1) || b1 != 0xef)
        {
            reader.Rewind(1);
            return ReadSequenceWithoutBOMResult.NeverExisted;
        }
        if (!reader.TryRead(out var b2) || b2 != 0xbb)
        {
            reader.Rewind(2);
            return ReadSequenceWithoutBOMResult.NeverExisted;
        }
        if (!reader.TryRead(out var b3) || b3 != 0xbf)
        {
            reader.Rewind(3);
            return ReadSequenceWithoutBOMResult.NeverExisted;
        }

        sequenceWithoutBOM = reader.UnreadSequence;
        return ReadSequenceWithoutBOMResult.Existed;
    }

    internal enum ReadSequenceWithoutBOMResult : byte
    {
        Existed,
        NeverExisted,
        Failure,
    }

}