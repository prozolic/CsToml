using CsToml.Debugger;
using CsToml.Utility;
using System.Buffers;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
