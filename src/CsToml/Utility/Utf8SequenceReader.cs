using CsToml.Extension;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CsToml.Utility;

// -----------------------------------------------------------------------------------------------------------------------------
// The original code for Utf8SequenceReader is from dotnet/runtime(MIT license), Please check the original license.
//
// [dotnet/runtime]
// https://github.com/dotnet/runtime
//
// [System.Buffers.SequenceReader]
// https://github.com/dotnet/runtime/blob/main/src/libraries/System.Memory/src/System/Buffers/SequenceReader.cs
// -----------------------------------------------------------------------------------------------------------------------------
internal ref struct Utf8SequenceReader
{
    private SequencePosition currentPosition;
    private SequencePosition nextPosition;
    private bool moreData;

    public readonly ReadOnlySpan<byte> UnreadSpan
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => CurrentSpan[CurrentSpanIndex..];
    }

    public ReadOnlySpan<byte> CurrentSpan { get; private set; }

    public ReadOnlySequence<byte> Sequence { get; }

    public readonly SequencePosition Position
        => Sequence.GetPosition(CurrentSpanIndex, currentPosition);

    public int CurrentSpanIndex { get; private set; }

    public long Consumed { get; private set; }

    public readonly long Remaining => Length - Consumed;

    public readonly long Length { get; }

    public readonly bool End => !moreData;

    public readonly bool IsFullSpan { get; init; }

    public Utf8SequenceReader(ReadOnlySpan<byte> span)
    {
        CurrentSpanIndex = 0;
        Consumed = 0;
        Sequence = default;
        Length = span.Length;

        nextPosition = default;
        CurrentSpan = span;
        moreData = span.Length > 0;
        IsFullSpan = true;
    }

    public Utf8SequenceReader(ReadOnlySequence<byte> sequence)
    {
        CurrentSpanIndex = 0;
        Consumed = 0;
        Sequence = sequence;
        currentPosition = sequence.Start;
        Length = Sequence.Length;

        ReadOnlySpan<byte> first = sequence.First.Span;
        nextPosition = sequence.GetPosition(first.Length);
        CurrentSpan = first;
        moreData = first.Length > 0;
        IsFullSpan = false;

        if (!moreData && !sequence.IsSingleSegment)
        {
            moreData = true;
            GetNextSpan();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(long count)
    {
        const long TooBigOrNegative = unchecked((long)0xFFFFFFFF80000000);
        if ((count & TooBigOrNegative) == 0 && CurrentSpan.Length - CurrentSpanIndex > (int)count)
        {
            CurrentSpanIndex += (int)count;
            Consumed += count;
        }
        else
        {
            AdvanceToNextSpan(count);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryPeek(out byte value)
    {
        if (moreData)
        {
            value = CurrentSpan.At(CurrentSpanIndex);
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryPeek(long offset, out byte value)
    {
        if (Remaining <= offset)
        {
            value = default;
            return false;
        }

        if (CurrentSpanIndex + offset <= CurrentSpan.Length - 1)
        {
            value = CurrentSpan.At(CurrentSpanIndex + (int)offset);
            return true;
        }

        return TryPeekSlow(offset, out value);
    }

    private readonly bool TryPeekSlow(long offset, out byte value)
    {
        var remainingOffset = offset;
        var nextPosition = Position;
        ReadOnlyMemory<byte> currentMemory;

        while (Sequence.TryGet(ref nextPosition, out currentMemory, true))
        {
            if (currentMemory.Length == 0)
                continue;

            if (remainingOffset >= currentMemory.Length)
                remainingOffset -= currentMemory.Length;
            else
                break;
        }

        value = currentMemory.Span.At((int)remainingOffset);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Rewind(long count)
    {
        if (count == 0)
        {
            return;
        }

        Consumed -= count;

        if (CurrentSpanIndex >= count)
        {
            CurrentSpanIndex -= (int)count;
            moreData = true;
        }
        else
        {
            var comsumed = Consumed;
            ResetReader();
            Advance(comsumed);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AdvanceCurrentSpan(long count)
    {
        Debug.Assert(count >= 0);

        Consumed += count;
        CurrentSpanIndex += (int)count;
        if (CurrentSpanIndex >= CurrentSpan.Length)
            GetNextSpan();
    }

    public bool TryFullSpan(long length, out ReadOnlySpan<byte> values)
    {
        if (IsFullSpan)
        {
            if (length <= Remaining)
            {
                values = CurrentSpan.Slice(CurrentSpanIndex, (int)length);
                Advance(length);
                return true;
            }
        }
        else
        {
            if (length <= UnreadSpan.Length)
            {
                values = UnreadSpan[..(int)length];
                Advance(length);
                return true;
            }
        }

        values = default;
        return false;
    }

    public bool TryGetbytes(long length, IBufferWriter<byte> writer)
    {
        if (Length < Consumed + length)
            return false;

        if (length <= UnreadSpan.Length)
        {
            UnreadSpan[..(int)length].CopyTo(writer.GetSpan((int)length));
            writer.Advance((int)length);
            Advance((int)length);
            return true;
        }

        var copiedSize = UnreadSpan.Length;
        UnreadSpan.CopyTo(writer.GetSpan(copiedSize));
        writer.Advance(copiedSize);
        Advance(copiedSize);

        var unreadSize = length - copiedSize;
        while (unreadSize > 0)
        {
            var unreadSpan = UnreadSpan;
            if (unreadSize > unreadSpan.Length)
            {
                UnreadSpan.CopyTo(writer.GetSpan(unreadSpan.Length));
                writer.Advance(unreadSpan.Length);
                Advance(unreadSpan.Length);
                unreadSize -= unreadSpan.Length;
            }
            else
            {
                var size = (int)unreadSize;
                UnreadSpan[..size].CopyTo(writer.GetSpan(size));
                writer.Advance(size);
                Advance(size);
                unreadSize = 0;
            }
        }

        return true;
    }

    public bool TryGetbytes(long length, scoped Span<byte> span)
    {
        if (length != span.Length)
            return false;
        if (Length < Consumed + length)
            return false;

        if (length <= UnreadSpan.Length)
        {
            UnreadSpan[..(int)length].CopyTo(span);
            Advance((int)length);
            return true;
        }

        var copiedSize = UnreadSpan.Length;
        UnreadSpan.CopyTo(span.Slice(0, copiedSize));
        Advance(copiedSize);

        var startIndex = copiedSize;
        var unreadSize = length - copiedSize;
        while (unreadSize > 0)
        {
            var unreadSpan = UnreadSpan;
            if (unreadSize > unreadSpan.Length)
            {
                UnreadSpan.CopyTo(span.Slice(startIndex, unreadSpan.Length));
                Advance(unreadSpan.Length);
                unreadSize -= unreadSpan.Length;
                startIndex += unreadSpan.Length;
            }
            else
            {
                var size = (int)unreadSize;
                UnreadSpan[..size].CopyTo(span.Slice(startIndex, size));
                Advance(size);
                unreadSize = 0;
                startIndex += size;
            }
        }

        return true;
    }


    private void ResetReader()
    {
        CurrentSpanIndex = 0;
        Consumed = 0;
        currentPosition = Sequence.Start;
        nextPosition = currentPosition;

        if (Sequence.TryGet(ref nextPosition, out var memory, advance: true))
        {
            moreData = true;

            if (memory.Length == 0)
            {
                CurrentSpan = default;
                GetNextSpan();
            }
            else
            {
                CurrentSpan = memory.Span;
            }
        }
        else
        {
            moreData = false;
            CurrentSpan = default;
        }
    }

    private void GetNextSpan()
    {
        if (!Sequence.IsSingleSegment)
        {
            SequencePosition previousNextPosition = nextPosition;
            while (Sequence.TryGet(ref nextPosition, out var memory, advance: true))
            {
                currentPosition = previousNextPosition;
                if (memory.Length > 0)
                {
                    CurrentSpan = memory.Span;
                    CurrentSpanIndex = 0;
                    return;
                }
                else
                {
                    CurrentSpan = default;
                    CurrentSpanIndex = 0;
                    previousNextPosition = nextPosition;
                }
            }
        }
        moreData = false;
    }

    private void AdvanceToNextSpan(long count)
    {
        Consumed += count;
        while (moreData)
        {
            int remaining = CurrentSpan.Length - CurrentSpanIndex;

            if (remaining > count)
            {
                CurrentSpanIndex += (int)count;
                count = 0;
                break;
            }

            CurrentSpanIndex += remaining;
            count -= remaining;
            GetNextSpan();

            if (count == 0)
            {
                break;
            }
        }

        if (count != 0)
        {
            Consumed -= count;
            throw new Exception();
        }
    }

    private bool IsNextSlow(scoped ReadOnlySpan<byte> next, bool advancePast)
    {
        ReadOnlySpan<byte> currentSpan = UnreadSpan;

        int fullLength = next.Length;
        SequencePosition nextPosition = this.nextPosition;

        while (next.StartsWith(currentSpan))
        {
            if (next.Length == currentSpan.Length)
            {
                // Fully matched
                if (advancePast)
                {
                    Advance(fullLength);
                }
                return true;
            }

            // Need to check the next segment
            while (true)
            {
                if (!Sequence.TryGet(ref nextPosition, out ReadOnlyMemory<byte> nextSegment, advance: true))
                {
                    // Nothing left
                    return false;
                }

                if (nextSegment.Length > 0)
                {
                    next = next.Slice(currentSpan.Length);
                    currentSpan = nextSegment.Span;
                    if (currentSpan.Length > next.Length)
                    {
                        currentSpan = currentSpan.Slice(0, next.Length);
                    }
                    break;
                }
            }
        }

        return false;
    }

}
