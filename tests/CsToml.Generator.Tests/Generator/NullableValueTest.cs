using Shouldly;
using CsToml.Generator.Other;

namespace CsToml.Generator.Tests;

public class NullableValueTest
{
    [Fact]
    public void Deserialize()
    {
        {
            var value = CsTomlSerializer.Deserialize<A>(@"
Value = 12345

[B]
Name = ""This is B""
"u8);
            value.Value.ShouldBe(12345);
            value.B.ShouldNotBeNull();
            value.B!.Name.ShouldBe("This is B");
        }
        {
            var value = CsTomlSerializer.Deserialize<A>(@""u8);

            value.ShouldNotBeNull();
            value.Value.ShouldBeNull();
            value.B.ShouldBeNull();
        }
    }
}
