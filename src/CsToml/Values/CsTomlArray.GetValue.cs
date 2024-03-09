
using CsToml.Error;
using System.Collections.ObjectModel;

namespace CsToml.Values;

internal partial class CsTomlArray 
{
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
