using Shouldly;
using System.Collections;
using System.Text;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class NullableReferenceTypesTest
{
    [Fact]
    public void Deserialize()
    {
        var types = CsTomlSerializer.Deserialize<NullableReferenceTypes>(""u8);
        types.Str.ShouldBeNull();
        types.NullableStr.ShouldBeNull();
        types.Uri.ShouldBeNull();
        types.NullableUri.ShouldBeNull();
        types.Version.ShouldBeNull();
        types.NullableVersion.ShouldBeNull();
        types.StringBuilder.ShouldBeNull();
        types.NullableStringBuilder.ShouldBeNull();
        types.Type.ShouldBeNull();
        types.NullableType.ShouldBeNull();
        types.BitArray.ShouldBeNull();
        types.NullableBitArray.ShouldBeNull();

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Str = \"I'm a string.\"");
        writer.AppendLine("NullableStr = \"I'm a string.\"");
        writer.AppendLine("Uri = 'https://github.com/prozolic/CsToml'");
        writer.AppendLine("NullableUri = 'https://github.com/prozolic/CsToml'");
        writer.AppendLine("Version = \"1.3.1\"");
        writer.AppendLine("NullableVersion = \"1.3.1\"");
        writer.AppendLine("StringBuilder = \"I'm a StringBuilder.\"");
        writer.AppendLine("NullableStringBuilder = \"I'm a StringBuilder.\"");
        writer.AppendLine("Type = \"System.String\"");
        writer.AppendLine("NullableType = \"System.String\"");
        writer.AppendLine("BitArray = [true, false, true]");
        writer.AppendLine("NullableBitArray = [true, false, true]");
        writer.Flush();

        var types2 = CsTomlSerializer.Deserialize<NullableReferenceTypes>(buffer.WrittenSpan);
        types2.Str.ShouldBe("I'm a string.");
        types2.NullableStr!.ShouldBe("I'm a string.");
        types2.Uri.ShouldBe(new Uri("https://github.com/prozolic/CsToml"));
        types2.NullableUri!.ShouldBe(new Uri("https://github.com/prozolic/CsToml"));
        types2.Version.ShouldBe(new Version("1.3.1"));
        types2.NullableVersion!.ShouldBe(new Version("1.3.1"));
        types2.StringBuilder.ToString().ShouldBe("I'm a StringBuilder.");
        types2.NullableStringBuilder!.ToString().ShouldBe("I'm a StringBuilder.");
        types2.Type.ShouldBe(typeof(string));
        types2.NullableType!.ShouldBe(typeof(string));
        types2.BitArray[0].ShouldBeTrue();
        types2.BitArray[1].ShouldBeFalse();
        types2.BitArray[2].ShouldBeTrue();
        types2.BitArray.Length.ShouldBe(3);
        types2.NullableBitArray![0].ShouldBeTrue();
        types2.NullableBitArray![1].ShouldBeFalse();
        types2.NullableBitArray![2].ShouldBeTrue();
        types2.NullableBitArray!.Length.ShouldBe(3);
    }
}
