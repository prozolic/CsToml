using CsToml.Extensions;
using Shouldly;

namespace CsToml.Generator.Tests;

public class FileSerializerTest : IDisposable
{
    private readonly string tempDir;

    public FileSerializerTest()
    {
        tempDir = Path.Combine(Path.GetTempPath(), $"CsToml_GeneratorTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir, true);
        }
    }

    private static FileSerializerClass CreateClass() => new()
    {
        Value = 1,
        Name = "name"
    };

    private static void ShouldBeCreatedClass(FileSerializerClass value)
    {
        value.Value.ShouldBe(1);
        value.Name.ShouldBe("name");
    }

    [Test]
    public void SerializeAndDeserializeClass()
    {
        var filePath = Path.Combine(tempDir, "class.toml");

        CsTomlFileSerializer.Serialize(filePath, CreateClass());
        var value = CsTomlFileSerializer.Deserialize<FileSerializerClass>(filePath);

        ShouldBeCreatedClass(value);
    }

    [Test]
    public async ValueTask SerializeAsyncAndDeserializeAsyncClass(CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(tempDir, "class.toml");

        await CsTomlFileSerializer.SerializeAsync(filePath, CreateClass(), cancellationToken: cancellationToken);
        var value = await CsTomlFileSerializer.DeserializeAsync<FileSerializerClass>(filePath, cancellationToken: cancellationToken);

        ShouldBeCreatedClass(value);
    }

    [Test]
    public void SerializeAndDeserializeStruct()
    {
        var filePath = Path.Combine(tempDir, "struct.toml");

        CsTomlFileSerializer.Serialize(filePath, new FileSerializerStruct() { Value = 5, Values = [6, 7] });
        var value = CsTomlFileSerializer.Deserialize<FileSerializerStruct>(filePath);

        value.Value.ShouldBe(5);
        value.Values.ShouldBe([6, 7]);
    }

    [Test]
    public async ValueTask SerializeAsyncAndDeserializeAsyncStruct(CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(tempDir, "struct.toml");

        await CsTomlFileSerializer.SerializeAsync(filePath, new FileSerializerStruct() { Value = 5, Values = [6, 7] }, cancellationToken: cancellationToken);
        var value = await CsTomlFileSerializer.DeserializeAsync<FileSerializerStruct>(filePath, cancellationToken: cancellationToken);

        value.Value.ShouldBe(5);
        value.Values.ShouldBe([6, 7]);
    }
}
