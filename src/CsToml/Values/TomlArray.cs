using CsToml.Debugger;
using CsToml.Utility;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CsToml.Values;

[DebuggerTypeProxy(typeof(TomlArrayDebugView))]
[DebuggerDisplay("Array[{Count}]")]
internal partial class TomlArray : TomlValue, IEnumerable<TomlValue>
{
    private TomlValue[] values;
    private int count;

    public int Count => count;

    public override bool HasValue => true;

    public override TomlValueType Type => TomlValueType.Array;

    public TomlValue LastValue => count > 0 ? values[count - 1] : Empty;

    public TomlValue this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual((uint)index, (uint)count, nameof(index));
            return values[index];
        }
    }

    public TomlArray(int capacity)
    {
        values = capacity > 0 ? new TomlValue[capacity] : [];
    }

    public TomlArray() : this(4)
    { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<TomlValue> AsSpan()
        => values.AsSpan(0, count);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<TomlValue> SetCount(int count)
    {
        var span = values.AsSpan(0, count);
        this.count = count;
        return span;
    }

    internal override void ToTomlString<TBufferWriter>(ref Utf8TomlDocumentWriter<TBufferWriter> writer)
    {
        writer.BeginArray();
        if (Count == 0)
        {
            writer.EndArray();
            return;
        }

        values[0].ToTomlString(ref writer);
        if (Count == 1)
        {
            writer.WriteSpace();
            writer.EndArray();
            return;
        }

        for (int i = 1; i < Count; i++)
        {
            writer.Write(TomlCodes.Symbol.COMMA);
            writer.WriteSpace();
            values[i].ToTomlString(ref writer);
        }
        writer.WriteSpace();
        writer.EndArray();
        return;
    }

    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        var written = 0;

        if (destination.Length == 0)
        {
            charsWritten = 0;
            return false;
        }
        destination[written++] = '[';

        var arraySpan = AsSpan();
        for (int i = 0; i < arraySpan.Length - 1; i++)
        {
            if (arraySpan[i].TryFormat(destination.Slice(written), out var charsWritten2, format, provider))
            {
                written += charsWritten2;

                if (destination.Length - written >= 3)
                {
                    destination[written++] = ',';
                    destination[written++] = ' ';
                }
                continue;
            }

            charsWritten = 0;
            return false;
        }

        if (arraySpan[arraySpan.Length - 1].TryFormat(destination.Slice(written), out var charsWritten3, format, provider))
        {
            written += charsWritten3;
            if (destination.Length - written == 0)
            {
                charsWritten = 0;
                return false;
            }
            destination[written++] = ']';
            charsWritten = written;
            return true;
        }
        charsWritten = 0;
        return false;
    }

    public override string ToString(string? format, IFormatProvider? formatProvider)
    {
        var length = 1024;
        var bufferWriter = RecycleArrayPoolBufferWriter<char>.Rent();
        try
        {
            var conflictCount = 0;
            var charsWritten = 0;
            while (!TryFormat(bufferWriter.GetSpan(length), out charsWritten, format.AsSpan(), formatProvider))
            {
                if (++conflictCount >= 20)
                {
                    break;
                }
                length *= 2;
            }

            bufferWriter.Advance(charsWritten);
            return new string(bufferWriter.WrittenSpan);
        }
        finally
        {
            RecycleArrayPoolBufferWriter<char>.Return(bufferWriter);
        }
    }

    public override string ToString() => ToString(null, null);

    protected void AddCore(TomlValue tomlValue)
    {
        if (count == values.Length)
        {
            Array.Resize(ref values, count == 0 ? 4 : count * 2);
        }
        values[count++] = tomlValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator()
        => new Enumerator(this);

    IEnumerator<TomlValue> IEnumerable<TomlValue>.GetEnumerator()
        => new IEnumerableEnumerator(this);

    IEnumerator IEnumerable.GetEnumerator()
        => ((IEnumerable<TomlValue>)this)!.GetEnumerator();

    public ref struct Enumerator(TomlArray array)
    {
        private readonly ReadOnlySpan<TomlValue> tomlArraySpan = array.AsSpan();
        private int index = 0;

        public TomlValue? Current { get; private set; }

        public bool MoveNext()
        {
            var array = tomlArraySpan;
            if ((uint)index >= (uint)array.Length) return false;

            this.Current = array[index];
            index++;
            return true;
        }
    }

    private struct IEnumerableEnumerator(TomlArray array) : IEnumerator<TomlValue?>
    {
        private readonly TomlArray tomlArray = array;
        private int index = 0;

        public TomlValue? Current { get; private set; }

        object? IEnumerator.Current => this.Current;

        public readonly IEnumerableEnumerator GetEnumerator() => this;

        public void Dispose()
            => Reset();

        public bool MoveNext()
        {
            var array = tomlArray;
            if ((uint)index >= (uint)array.Count) return false;

            this.Current = array[index];
            index++;
            return true;
        }

        public void Reset()
        {
            index = 0;
        }
    }
}
