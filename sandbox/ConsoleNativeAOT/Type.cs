
using CsToml;
using System.Buffers;

namespace ConsoleNativeAOT;

public enum Color
{
    Red,
    Green,
    Blue
}

public partial struct TestStruct2
{
    public long Value { get; set; }

}

[TomlSerializedObject]
public partial class Sample
{
    [TomlValueOnSerialized]
    public Custom Key { get; set; }
}


public struct Custom
{
    public long Value { get; set; }

    public Custom(long value)
    {
        Value = value;
    }
    public Custom(string str)
    {
        if (long.TryParse(str.AsSpan(), out var value))
        {
            Value = value;
        }
        else
        {
            Value = 0;
        }
    }
}
