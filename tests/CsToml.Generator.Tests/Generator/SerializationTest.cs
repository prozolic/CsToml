using CsToml.Error;
using CsToml.Formatter.Resolver;
using Shouldly;
using Utf8StringInterpolation;
using CsToml.Generator.Other;

namespace CsToml.Generator.Tests;

// https://github.com/prozolic/CsToml/issues/24
public class Issue24Test
{
    [Fact]
    public void DeserializeAndSerialize()
    {
        TomlValueFormatterResolver.Register(new SpecialHashFormatter());
        var entity = CsTomlSerializer.Deserialize<Entity>("Name = 12345"u8);
        var entity2 = CsTomlSerializer.Deserialize<Entity>("Name = \"This is String\""u8);
        entity.Name.ShouldBe(new SpecialHash(12345));
        entity2.Name.ShouldBe(new SpecialHash(3955703026));

        using var entityBytes = CsTomlSerializer.Serialize(entity);
        using var entityBytes2 = CsTomlSerializer.Serialize(entity2);

        using var buffer = Utf8String.CreateWriter(out var writer);
        writer.AppendLine("Name = 12345");
        writer.Flush();
        entityBytes.ToString().ShouldBe(buffer.ToString());

        using var buffer2 = Utf8String.CreateWriter(out var writer2);
        writer2.AppendLine("Name = 3955703026");
        writer2.Flush();
        entityBytes2.ToString().ShouldBe(buffer2.ToString());
    }
}

// https://github.com/prozolic/CsToml/issues/70
public class Issue70Test
{
    [Fact]
    public void Test()
    {
        var interchangeChallenge = new InterchangeChallenge
        {
            Title = "Crypto Challenge",
            Content = "Decrypt this",
            Flags = new FlagsSection
            {
                Static = [new() { Value = "flag{crypto_master}" }]
            }
        };

        {
            using var bytes = CsTomlSerializer.Serialize(interchangeChallenge);
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("title = \"Crypto Challenge\"");
            writer.AppendLine("content = \"Decrypt this\"");
            writer.AppendLine("flags = {static = [ {value = \"flag{crypto_master}\"} ]}");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);

            var deserialized = CsTomlSerializer.Deserialize<InterchangeChallenge>(bytes.ByteSpan);
            deserialized.ShouldNotBeNull();
            deserialized.Title.ShouldBe("Crypto Challenge");
            deserialized.Content.ShouldBe("Decrypt this");
            deserialized.Flags.ShouldNotBeNull();
            deserialized.Flags!.Static.ShouldNotBeNull();
            deserialized.Flags!.Static!.Count.ShouldBe(1);
            deserialized.Flags.Static[0].ShouldNotBeNull();
            deserialized.Flags.Static[0].Value.ShouldBe("flag{crypto_master}");
        }
        {
            using var bytes = CsTomlSerializer.Serialize(interchangeChallenge, options: Option.Header);
            using var buffer = Utf8String.CreateWriter(out var writer);
            writer.AppendLine("title = \"Crypto Challenge\"");
            writer.AppendLine("content = \"Decrypt this\"");
            writer.AppendLine("[flags]");
            writer.AppendLine("static = [ {value = \"flag{crypto_master}\"} ]");
            writer.Flush();

            var expected = buffer.ToArray();
            bytes.ByteSpan.ToArray().ShouldBe(expected);

            var deserialized = CsTomlSerializer.Deserialize<InterchangeChallenge>(bytes.ByteSpan);
            deserialized.ShouldNotBeNull();
            deserialized.Title.ShouldBe("Crypto Challenge");
            deserialized.Content.ShouldBe("Decrypt this");
            deserialized.Flags.ShouldNotBeNull();
            deserialized.Flags!.Static.ShouldNotBeNull();
            deserialized.Flags!.Static!.Count.ShouldBe(1);
            deserialized.Flags.Static[0].ShouldNotBeNull();
            deserialized.Flags.Static[0].Value.ShouldBe("flag{crypto_master}");
        }


    }
}
