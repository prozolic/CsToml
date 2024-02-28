using CsToml.Error;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsToml.Utility;

// -----------------------------------------------------------------------------------------------------------------------------
// The original code for NativeByteMemoryArray is from Cysharp/NativeMemoryArray(MIT license), Please check the original license.
//
// [Cysharp/NativeMemoryArray]
// https://github.com/Cysharp/NativeMemoryArray
//
// [Cysharp.Collections.NativeMemoryArray]
// https://github.com/Cysharp/NativeMemoryArray/blob/master/src/NativeMemoryArray/NativeMemoryArray.cs
// -----------------------------------------------------------------------------------------------------------------------------
internal unsafe class NativeByteMemoryArray : IDisposable
{
    private readonly byte* ptr;
    private readonly long arrayLength;
    private bool disposed;

    public ref byte this[long index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((ulong)index >= (ulong)arrayLength) ExceptionHelper.ThrowArgumentOutOfRangeWhenOutsideTheBoundsOfTheArray();
            return ref Unsafe.AsRef<byte>(ptr + index);
        }
    }

    public NativeByteMemoryArray(long length)
    {
        arrayLength = length;
        ptr = (byte*)(length == 0 ?
            Unsafe.AsPointer(ref Unsafe.AsRef<byte>(null)) :
            NativeMemory.AllocZeroed((nuint)length, sizeof(byte)));
    }

    ~NativeByteMemoryArray()
    {
        DisposeCore();
    }

    public IReadOnlyList<Memory<byte>> AsMemoryList(int start)
    {
        if (arrayLength == 0) return [];

        var array = new Memory<byte>[(long)arrayLength <= int.MaxValue ? 1 : (long)arrayLength / int.MaxValue + 1];
        var index = 0;
        var memory = new NativeByteMemorySequence(this, start);
        foreach (var item in memory)
        {
            array[index++] = item;
        }

        return array;
    }

    internal Memory<byte> AsMemory(long start, int length)
        => new NativeByteMemoryManager(ptr + start, length).Memory;

    public void Dispose()
    {
        DisposeCore();
        GC.SuppressFinalize(true);
    }

    private void DisposeCore()
    {
        if (disposed) return;
        disposed = true;

        if (arrayLength != 0)
        {
            NativeMemory.Free(ptr);
        }
    }

    private struct NativeByteMemorySequence
    {
        private readonly NativeByteMemoryArray array;
        private long index;
        private long start;

        internal NativeByteMemorySequence(NativeByteMemoryArray nativeArray, long arrayStart)
        {
            array = nativeArray;
            index = 0;
            start = arrayStart;
        }

        public readonly NativeByteMemorySequence GetEnumerator() => this;

        public readonly Memory<byte> Current
            => array.AsMemory(start, (int)Math.Min(int.MaxValue, array.arrayLength - start));

        public bool MoveNext()
        {
            if (index < array.arrayLength)
            {
                start = index;
                index += int.MaxValue;
                return true;
            }
            return false;
        }
    }

    private unsafe class NativeByteMemoryManager(byte* ptr, int length) : MemoryManager<byte>
    {
        private readonly byte* arrayPtr = ptr;
        private readonly int arrayLength = length;

        public override Span<byte> GetSpan()
            => new(arrayPtr, arrayLength);

        public override MemoryHandle Pin(int elementIndex = 0)
            => new(arrayPtr + elementIndex, default, this);

        public override void Unpin() { }

        protected override void Dispose(bool disposing) { }
    }
}
