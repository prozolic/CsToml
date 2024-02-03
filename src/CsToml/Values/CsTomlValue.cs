using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

public abstract partial class CsTomlValue
{
    public static readonly CsTomlValue Empty = new CsTomlEmpty();

    public CsTomlType Type { get;}

    public virtual CsTomlValue this[int index] => Empty;

    protected CsTomlValue(CsTomlType type)
    {
        this.Type = type;
    }

    public bool HasValue()
        => this.Type != CsTomlType.None;

    internal virtual bool ToTomlString(ref Utf8Writer writer)
        => false;

    [DebuggerDisplay(nameof(CsTomlEmpty))]
    private sealed class CsTomlEmpty : CsTomlValue
    {
        public CsTomlEmpty() : base(CsTomlType.None) { }
    }
}
