using CsToml.Debugger;
using CsToml.Utility;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CsToml.Values;

[DebuggerTypeProxy(typeof(TomlArrayDebugView))]
[DebuggerDisplay("Count = {Count}")]
internal sealed partial class TomlArray(int capacity) : 
    TomlValue(), 
    IEnumerable<TomlValue>
{
    private readonly List<TomlValue> values = new(capacity);

    public int Count => values.Count;

    public IEnumerable<TomlValue> Values => values;

    public override bool HasValue => true;

    public TomlValue LastValue => Count > 0 ? values[Count - 1] : Empty;

    public TomlValue this[int index] => values[index];

    public TomlArray() : this(4)
    {}

    internal override bool ToTomlString<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer)
    {
        writer.Write(TomlCodes.Symbol.LEFTSQUAREBRACKET);
        writer.Write(TomlCodes.Symbol.SPACE);

        for (int i = 0; i < Count; i++)
        {
            values[i].ToTomlString(ref writer);
            if (i != Count - 1)
            {
                writer.Write(TomlCodes.Symbol.COMMA);
            }
            writer.Write(TomlCodes.Symbol.SPACE);
        }
        writer.Write(TomlCodes.Symbol.RIGHTSQUAREBRACKET);
        return true;
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

        var arraySpan = CollectionsMarshal.AsSpan(values);
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
        => ToString();

    public override string ToString()
    {
        var length = 1024;
        var bufferWriter = RecycleArrayPoolBufferWriter<char>.Rent();
        try
        {
            var conflictCount = 0;
            var charsWritten = 0;
            while (!TryFormat(bufferWriter.GetSpan(length), out charsWritten))
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

    public void Add(TomlValue tomlValue)
        => values.Add(tomlValue);

    public IEnumerator<TomlValue> GetEnumerator()
        => new Enumerator(this);

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    private struct Enumerator(TomlArray array) : IEnumerator<TomlValue>
    {
        private readonly TomlArray tomlArray = array;
        private int index = 0;

        public TomlValue Current { get; private set; }

        object IEnumerator.Current => this.Current;

        public readonly Enumerator GetEnumerator() => this;

        public void Dispose()
            => Reset();

        public bool MoveNext()
        {
            var array = tomlArray;
            if (array.Count <= index) return false;

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
