
using CsToml.Utility;
using System.Diagnostics;

namespace CsToml.Values;

public abstract partial class CsTomlValue
{
    public static readonly CsTomlValue Empty = new CsTomlEmpty();

    public CsTomlType Type { get;}

    protected CsTomlValue(CsTomlType type)
    {
        this.Type = type;
    }

    internal virtual bool ToTomlString(ref Utf8Writer writer)
    {
        
        return false;
    }

    [DebuggerDisplay(nameof(CsTomlEmpty))]
    private sealed class CsTomlEmpty : CsTomlValue
    {
        public CsTomlEmpty() : base(CsTomlType.None) { }
    }
}
