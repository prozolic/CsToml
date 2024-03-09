using System;
using System.Buffers;

namespace ConsoleApp;

public class Segment<T> : ReadOnlySequenceSegment<T>
{
    public int Length => Memory.Length;

    public Segment(ReadOnlyMemory<T> memory)
    {
        Memory = memory;
    }

    public void SetRunningIndex(long runningIndex)
    {
        RunningIndex = runningIndex;
    }

    public void SetNext(ReadOnlySequenceSegment<T>? next)
    {
        Next = next;
    }
}
