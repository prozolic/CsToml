
using CsToml.Error;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

namespace CsToml.Values;

internal partial class TomlArray
{
    public override bool CanGetValue(TomlValueFeature feature)
        => ((TomlValueFeature.String | TomlValueFeature.Array | TomlValueFeature.Object) & feature) == feature;

    public override ReadOnlyCollection<TomlValue> GetArray()
        => count == values.Length
            ? new ReadOnlyCollection<TomlValue>(values)
            : new ReadOnlyCollection<TomlValue>(new ArraySegment<TomlValue>(values, 0, count));

    public override TomlValue GetArrayValue(int index)
    {
        if ((uint)index >= (uint)Count)
        {
            ExceptionHelper.ThrowArgumentOutOfRangeWhenOutsideTheBoundsOfTheArray();
        }
        return this[index];
    }

    public override ImmutableArray<TomlValue> GetImmutableArray()
        => ImmutableArray.Create(values, 0, count);

    public override string GetString()
        => ToString();

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
