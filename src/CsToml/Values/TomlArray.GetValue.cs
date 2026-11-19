
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace CsToml.Values;

internal partial class TomlArray
{
    public override bool TryGetArray(out ImmutableArray<TomlValue> value)
    {
        if (values.Length != count)
        {
            Array.Resize(ref values, count);
        }
        value = ImmutableCollectionsMarshal.AsImmutableArray(values);
        return true;
    }

    public override bool TryGetArraySpan(out ReadOnlySpan<TomlValue> value)
    {
        value = AsSpan();
        return true;
    }

    public override object GetObject()
    {
        var array = new object[Count];
        for (int i = 0; i < Count; i++)
        {
            array[i] = values[i].GetObject();
        }
        return array;
    }
}
