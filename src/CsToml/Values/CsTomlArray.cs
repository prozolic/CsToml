using CsToml.Debugger;
using CsToml.Formatter;
using CsToml.Utility;
using System.Collections;
using System.Diagnostics;

namespace CsToml.Values;

[DebuggerTypeProxy(typeof(CsTomlArrayDebugView))]
[DebuggerDisplay("CsTomlArray: Count={Count}")]
internal class CsTomlArray : CsTomlValue, IEnumerable<CsTomlValue>
{
    private List<CsTomlValue> values = [];

    public int Count => values.Count;

    public IEnumerable<CsTomlValue> Value => values;

    public override CsTomlValue this[int index] => values[index];

    public CsTomlArray() : base(CsTomlType.Array)
    {}

    internal override bool ToTomlString(ref Utf8Writer writer)
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
