
using CsToml.Error;
using CsToml.Values.Internal;
using CsToml.Values;
using System.Buffers;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Utility;

#if NET10_0_OR_GREATER
// https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.inlinearray16-1?view=net-10.0
// InlineArray16 is dened in the .NET 10.0, but not in the .NET 8.0 and 9.0.
#else
[InlineArray(16)]
internal struct InlineArray16<T>
{
    T item;
}
#endif

// https://speakerdeck.com/neuecc/cedec-2023-modanhaipahuomansuc-number-2023-edition?slide=47
// length is from 32 to 1,073,741,824
[InlineArray(26)]
internal struct InlineArray26<T>
{
    T item;

    public Span<T> AsSpan()
    {
        return MemoryMarshal.CreateSpan(ref item, 26);

    }
}

[StructLayout(LayoutKind.Auto)]
internal ref struct InlineArrayBuilder<T>
{
    private Span<T> initialArray;
    private InlineArray26<T[]> segments;
    private int segmentsIndex;
    private Span<T> current;
    private int currentIndex;
    private int count;
    private T? lastValue;

    public Span<T[]> DebugSegments => segments.AsSpan();

    public readonly int Count => count;

    public readonly T? LastValue => lastValue;

    public InlineArrayBuilder(Span<T> initialSpan)
    {
        initialArray = initialSpan;
        current = initialSpan;
        currentIndex = 0;
        segmentsIndex = -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T value)
    {
        if ((uint)currentIndex < (uint)current.Length)
        {
            current[currentIndex++] = value;
        }
        else
        {
            this.AddAndEnsureCapacity(value);
        }
        lastValue = value;
        checked { count++; }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyToAndReturn(Span<T> destination)
    {
        if (count == 0)
            return;

        if (segmentsIndex > -1)
        {
            var initialArray = this.initialArray;
            initialArray.CopyTo(destination);
            destination = destination[initialArray.Length..];

            if (segmentsIndex > 0)
            {
                foreach (var segment in ((Span<T[]>)segments)[..(segmentsIndex)])
                {
                    segment.AsSpan().CopyTo(destination);
                    destination = destination[segment.Length..];
                    ArrayPool<T>.Shared.Return(segment, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
                }
            }

            var lastSegment = segments[segmentsIndex];
            var lastSegmentSpan = lastSegment.AsSpan().Slice(0, currentIndex);
            lastSegmentSpan.CopyTo(destination);
            ArrayPool<T>.Shared.Return(lastSegment, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        }
        else
        {
            var initialArray = this.initialArray.Slice(0, currentIndex);
            initialArray.CopyTo(destination);
        }

        current = initialArray;
        count = 0;
        currentIndex = 0;
        segmentsIndex = -1;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddAndEnsureCapacity(T value)
    {
        if (segmentsIndex == 26) // fill 
        {
            ExceptionHelper.ThrowOutOfMemory();
        }
        Span<T[]> segment = segments;
        segment[++segmentsIndex] = ArrayPool<T>.Shared.Rent(Math.Min(current.Length * 2, Array.MaxLength));
        current = segment[segmentsIndex];
        currentIndex = 0;
        current[currentIndex++] = value;
    }
}

