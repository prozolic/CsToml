using CsToml.Debugger;
using CsToml.Utility;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CsToml.Values;

[DebuggerTypeProxy(typeof(CsTomlArrayDebugView))]
[DebuggerDisplay("Count = {Count}")]
internal partial class CsTomlArray(int capacity) : 
    CsTomlValue(), 
    IEnumerable<CsTomlValue>
{
    private readonly List<CsTomlValue> values = new(capacity);

    public int Count => values.Count;

    public IEnumerable<CsTomlValue> Values => values;

    public override bool HasValue => true;

    public CsTomlValue LastValue => Count > 0 ? values[Count - 1] : Empty;

    public CsTomlValue this[int index] => values[index];

    public CsTomlArray() : this(4)
    {}

    internal override bool ToTomlString<TBufferWriter>(ref Utf8Writer<TBufferWriter> writer)
    {
        writer.Write(CsTomlSyntax.Symbol.LEFTSQUAREBRACKET);
        writer.Write(CsTomlSyntax.Symbol.SPACE);

        for (int i = 0; i < Count; i++)
        {
            values[i].ToTomlString(ref writer);
            if (i != Count - 1)
            {
                writer.Write(CsTomlSyntax.Symbol.COMMA);
            }
            writer.Write(CsTomlSyntax.Symbol.SPACE);
        }
        writer.Write(CsTomlSyntax.Symbol.RIGHTSQUAREBRACKET);
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
        using var bufferWriter = new ArrayPoolBufferWriter<char>(length);

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

    public void Add(CsTomlValue tomlValue)
        => values.Add(tomlValue);

    public IEnumerator<CsTomlValue> GetEnumerator()
        => new Enumerator(this);

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    private struct Enumerator(CsTomlArray array) : IEnumerator<CsTomlValue>
    {
        private readonly CsTomlArray tomlArray = array;
        private int index = 0;

        public CsTomlValue Current { get; private set; }

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
