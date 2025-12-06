using Shouldly;
using System.Collections;
using Utf8StringInterpolation;

namespace CsToml.Generator.Tests;

public class TypeBuiltinTest()
{
    [Fact]
    public void Serialize()
    {
        var typeBuiltin = new TypeBuiltin()
        {
            TimeSpan = new TimeSpan(1, 2, 3, 4, 5),
            Guid = Guid.Parse("c9da6455-213d-4a7b-8f1a-4d6d1f5c5e9f"),
            Uri = new Uri("https://github.com/prozolic/CsToml"),
            Version = new Version(1, 2, 3, 4),
            BitArray = new BitArray(new[] { true, false, true, false }),
            Type = typeof(TypeBuiltinTest),
            Complex = new System.Numerics.Complex(12, 6)
        };

        using var bytes = CsTomlSerializer.Serialize(typeBuiltin);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("TimeSpan = 937840050000");
        writer.AppendLine("Guid = \"c9da6455-213d-4a7b-8f1a-4d6d1f5c5e9f\"");
        writer.AppendLine("Version = \"1.2.3.4\"");
        writer.AppendLine("Uri = \"https://github.com/prozolic/CsToml\"");
        writer.AppendLine("BitArray = [ true, false, true, false ]");
        writer.AppendLine("Type = \"CsToml.Generator.Tests.TypeBuiltinTest, CsToml.Generator.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\"");
        writer.AppendLine("Complex = [ 12.0, 6.0 ]");
        writer.Flush();

        var expected = buffer.ToArray();
        bytes.ByteSpan.ToArray().ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("TimeSpan = 937840050000");
        writer.AppendLine("Guid = \"c9da6455-213d-4a7b-8f1a-4d6d1f5c5e9f\"");
        writer.AppendLine("Version = \"1.2.3.4\"");
        writer.AppendLine("Uri = \"https://github.com/prozolic/CsToml\"");
        writer.AppendLine("BitArray = [ true, false, true, false ]");
        writer.AppendLine("Type = \"CsToml.Generator.Tests.TypeBuiltinTest, CsToml.Generator.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\"");
        writer.AppendLine("Complex = [ 12.0, 6.0 ]");
        writer.Flush();

        var typeBuiltin = CsTomlSerializer.Deserialize<TypeBuiltin>(buffer.WrittenSpan);
        typeBuiltin.TimeSpan.ShouldBe(new TimeSpan(1, 2, 3, 4, 5));
        typeBuiltin.Guid.ShouldBe(Guid.Parse("c9da6455-213d-4a7b-8f1a-4d6d1f5c5e9f"));
        typeBuiltin.Uri.ShouldBe(new Uri("https://github.com/prozolic/CsToml"));
        typeBuiltin.Version.ShouldBe(new Version(1, 2, 3, 4));
        typeBuiltin.BitArray.ShouldBe(new BitArray(new[] { true, false, true, false }));
        typeBuiltin.Type.ShouldBe(typeof(TypeBuiltinTest));
        typeBuiltin.Complex.ShouldBe(new System.Numerics.Complex(12, 6));
    }
}

