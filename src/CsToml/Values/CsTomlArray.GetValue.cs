
using CsToml.Error;
using System.Collections.ObjectModel;

namespace CsToml.Values;

internal partial class CsTomlArray 
{
    public override bool CanGetValue(CsTomlValueFeature feature)
        => (CsTomlValueFeature.Array & feature) == feature;

    public override ReadOnlyCollection<CsTomlValue> GetArray()
        => values.AsReadOnly();

    public override CsTomlValue GetArrayValue(int index)
    {
        if ((uint)index >= (uint)Count)
        {
            ExceptionHelper.ThrowArgumentOutOfRangeWhenOutsideTheBoundsOfTheArray();
        }
        return this[index];
    }
}
