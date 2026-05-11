using CsToml.Extensions;

namespace CsToml.Tests;

public class CsTomlFileSerializerExtensionPolicyTest : IDisposable
{
    private readonly string _tempDir;

    public CsTomlFileSerializerExtensionPolicyTest()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"CsToml_Test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    private string CreateTomlFile(string fileName, string content = "key = \"value\"")
    {
        var filePath = Path.Combine(_tempDir, fileName);
        File.WriteAllBytes(filePath, System.Text.Encoding.UTF8.GetBytes(content));
        return filePath;
    }

    private string GetNonExistentPath(string fileName)
    {
        return Path.Combine(_tempDir, fileName);
    }

    [Fact]
    public void Deserialize_Strict_WithTomlExtension_Succeeds()
    {
        var filePath = CreateTomlFile("test.toml");

        var doc = CsTomlFileSerializer.Deserialize<TomlDocument>(filePath, CsTomlSerializerOptions.Default, TomlFileExtensionPolicy.Strict);

        doc.ShouldNotBeNull();
    }

    [Fact]
    public void Deserialize_Strict_WithNonTomlExtension_ThrowsFormatException()
    {
        var filePath = CreateTomlFile("test.conf");

        Should.Throw<FormatException>(() =>
            CsTomlFileSerializer.Deserialize<TomlDocument>(filePath, CsTomlSerializerOptions.Default, TomlFileExtensionPolicy.Strict));
    }

    [Fact]
    public void Deserialize_Relaxed_WithTomlExtension_Succeeds()
    {
        var filePath = CreateTomlFile("test.toml");

        var doc = CsTomlFileSerializer.Deserialize<TomlDocument>(filePath, CsTomlSerializerOptions.Default, TomlFileExtensionPolicy.Relaxed);

        doc.ShouldNotBeNull();
    }

    [Fact]
    public void Deserialize_Relaxed_WithNonTomlExtension_Succeeds()
    {
        var filePath = CreateTomlFile("test.conf");

        var doc = CsTomlFileSerializer.Deserialize<TomlDocument>(filePath, CsTomlSerializerOptions.Default, TomlFileExtensionPolicy.Relaxed);

        doc.ShouldNotBeNull();
    }

    [Fact]
    public void Deserialize_DefaultOverload_WithTomlExtension_Succeeds()
    {
        var filePath = CreateTomlFile("test.toml");

        var doc = CsTomlFileSerializer.Deserialize<TomlDocument>(filePath);

        doc.ShouldNotBeNull();
    }

    [Fact]
    public void Deserialize_DefaultOverload_WithNonTomlExtension_ThrowsFormatException()
    {
        var filePath = CreateTomlFile("test.conf");

        Should.Throw<FormatException>(() =>
            CsTomlFileSerializer.Deserialize<TomlDocument>(filePath));
    }

    [Fact]
    public async ValueTask DeserializeAsync_Strict_WithTomlExtension_Succeeds()
    {
        var filePath = CreateTomlFile("test.toml");

        var doc = await CsTomlFileSerializer.DeserializeAsync<TomlDocument>(filePath, CsTomlSerializerOptions.Default, TomlFileExtensionPolicy.Strict, cancellationToken: TestContext.Current.CancellationToken);

        doc.ShouldNotBeNull();
    }

    [Fact]
    public async ValueTask DeserializeAsync_Strict_WithNonTomlExtension_ThrowsFormatException()
    {
        var filePath = CreateTomlFile("test.conf");

        await Should.ThrowAsync<FormatException>(async () =>
            await CsTomlFileSerializer.DeserializeAsync<TomlDocument>(filePath, CsTomlSerializerOptions.Default, TomlFileExtensionPolicy.Strict, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async ValueTask DeserializeAsync_Relaxed_WithTomlExtension_Succeeds()
    {
        var filePath = CreateTomlFile("test.toml");
        var doc = await CsTomlFileSerializer.DeserializeAsync<TomlDocument>(filePath, CsTomlSerializerOptions.Default, TomlFileExtensionPolicy.Relaxed, cancellationToken: TestContext.Current.CancellationToken);
        doc.ShouldNotBeNull();
    }

    [Fact]
    public async ValueTask DeserializeAsync_Relaxed_WithNonTomlExtension_Succeeds()
    {
        var filePath = CreateTomlFile("test.conf");
        var doc = await CsTomlFileSerializer.DeserializeAsync<TomlDocument>(filePath, CsTomlSerializerOptions.Default, TomlFileExtensionPolicy.Relaxed, cancellationToken: TestContext.Current.CancellationToken);
        doc.ShouldNotBeNull();
    }

    [Fact]
    public async ValueTask DeserializeAsync_DefaultOverload_WithTomlExtension_Succeeds()
    {
        var filePath = CreateTomlFile("test.toml");
        var doc = await CsTomlFileSerializer.DeserializeAsync<TomlDocument>(filePath, cancellationToken: TestContext.Current.CancellationToken);
        doc.ShouldNotBeNull();
    }

    [Fact]
    public async ValueTask DeserializeAsync_DefaultOverload_WithNonTomlExtension_ThrowsFormatException()
    {
        var filePath = CreateTomlFile("test.conf");

        await Should.ThrowAsync<FormatException>(async () =>
            await CsTomlFileSerializer.DeserializeAsync<TomlDocument>(filePath, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Serialize_Strict_WithTomlExtension_Succeeds()
    {
        var filePath = GetNonExistentPath("output.toml");
        var doc = CsTomlSerializer.Deserialize<TomlDocument>("key = \"value\""u8);

        Should.NotThrow(() =>
            CsTomlFileSerializer.Serialize(filePath, doc, CsTomlSerializerOptions.Default, TomlFileExtensionPolicy.Strict));

        File.Exists(filePath).ShouldBeTrue();
    }

    [Fact]
    public void Serialize_Strict_WithNonTomlExtension_ThrowsFormatException()
    {
        var filePath = GetNonExistentPath("output.conf");
        var doc = CsTomlSerializer.Deserialize<TomlDocument>("key = \"value\""u8);

        Should.Throw<FormatException>(() =>
            CsTomlFileSerializer.Serialize(filePath, doc, CsTomlSerializerOptions.Default, TomlFileExtensionPolicy.Strict));
    }

    [Fact]
    public void Serialize_Relaxed_WithTomlExtension_Succeeds()
    {
        var filePath = GetNonExistentPath("output.toml");
        var doc = CsTomlSerializer.Deserialize<TomlDocument>("key = \"value\""u8);

        Should.NotThrow(() =>
            CsTomlFileSerializer.Serialize(filePath, doc, CsTomlSerializerOptions.Default, TomlFileExtensionPolicy.Relaxed));

        File.Exists(filePath).ShouldBeTrue();
    }

    [Fact]
    public void Serialize_Relaxed_WithNonTomlExtension_Succeeds()
    {
        var filePath = GetNonExistentPath("output.conf");
        var doc = CsTomlSerializer.Deserialize<TomlDocument>("key = \"value\""u8);

        Should.NotThrow(() =>
            CsTomlFileSerializer.Serialize(filePath, doc, CsTomlSerializerOptions.Default, TomlFileExtensionPolicy.Relaxed));

        File.Exists(filePath).ShouldBeTrue();
    }

    [Fact]
    public void Serialize_DefaultOverload_WithTomlExtension_Succeeds()
    {
        var filePath = GetNonExistentPath("output.toml");
        var doc = CsTomlSerializer.Deserialize<TomlDocument>("key = \"value\""u8);

        Should.NotThrow(() =>
            CsTomlFileSerializer.Serialize(filePath, doc));

        File.Exists(filePath).ShouldBeTrue();
    }

    [Fact]
    public void Serialize_DefaultOverload_WithNonTomlExtension_ThrowsFormatException()
    {
        var filePath = GetNonExistentPath("output.conf");
        var doc = CsTomlSerializer.Deserialize<TomlDocument>("key = \"value\""u8);

        Should.Throw<FormatException>(() =>
            CsTomlFileSerializer.Serialize(filePath, doc));
    }

    [Fact]
    public async ValueTask SerializeAsync_Strict_WithTomlExtension_Succeeds()
    {
        var filePath = GetNonExistentPath("output.toml");
        var doc = CsTomlSerializer.Deserialize<TomlDocument>("key = \"value\""u8);

        await Should.NotThrowAsync(async () =>
            await CsTomlFileSerializer.SerializeAsync(filePath, doc, CsTomlSerializerOptions.Default, TomlFileExtensionPolicy.Strict, cancellationToken: TestContext.Current.CancellationToken));

        File.Exists(filePath).ShouldBeTrue();
    }

    [Fact]
    public async ValueTask SerializeAsync_Strict_WithNonTomlExtension_ThrowsFormatException()
    {
        var filePath = GetNonExistentPath("output.conf");
        var doc = CsTomlSerializer.Deserialize<TomlDocument>("key = \"value\""u8);

        await Should.ThrowAsync<FormatException>(async () =>
            await CsTomlFileSerializer.SerializeAsync(filePath, doc, CsTomlSerializerOptions.Default, TomlFileExtensionPolicy.Strict, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async ValueTask SerializeAsync_Relaxed_WithTomlExtension_Succeeds()
    {
        var filePath = GetNonExistentPath("output.toml");
        var doc = CsTomlSerializer.Deserialize<TomlDocument>("key = \"value\""u8);

        await Should.NotThrowAsync(async () =>
            await CsTomlFileSerializer.SerializeAsync(filePath, doc, CsTomlSerializerOptions.Default, TomlFileExtensionPolicy.Relaxed, cancellationToken: TestContext.Current.CancellationToken));

        File.Exists(filePath).ShouldBeTrue();
    }

    [Fact]
    public async ValueTask SerializeAsync_Relaxed_WithNonTomlExtension_Succeeds()
    {
        var filePath = GetNonExistentPath("output.conf");
        var doc = CsTomlSerializer.Deserialize<TomlDocument>("key = \"value\""u8);

        await Should.NotThrowAsync(async () =>
            await CsTomlFileSerializer.SerializeAsync(filePath, doc, CsTomlSerializerOptions.Default, TomlFileExtensionPolicy.Relaxed, cancellationToken: TestContext.Current.CancellationToken));

        File.Exists(filePath).ShouldBeTrue();
    }

    [Fact]
    public async ValueTask SerializeAsync_DefaultOverload_WithTomlExtension_Succeeds()
    {
        var filePath = GetNonExistentPath("output.toml");
        var doc = CsTomlSerializer.Deserialize<TomlDocument>("key = \"value\""u8);

        await Should.NotThrowAsync(async () =>
            await CsTomlFileSerializer.SerializeAsync(filePath, doc, cancellationToken: TestContext.Current.CancellationToken));

        File.Exists(filePath).ShouldBeTrue();
    }

    [Fact]
    public async ValueTask SerializeAsync_DefaultOverload_WithNonTomlExtension_ThrowsFormatException()
    {
        var filePath = GetNonExistentPath("output.conf");
        var doc = CsTomlSerializer.Deserialize<TomlDocument>("key = \"value\""u8);

        await Should.ThrowAsync<FormatException>(async () =>
            await CsTomlFileSerializer.SerializeAsync(filePath, doc, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("config.cfg")]
    [InlineData("settings.ini")]
    [InlineData("data.txt")]
    [InlineData("noextension")]
    public void Deserialize_Relaxed_WithVariousExtensions_Succeeds(string fileName)
    {
        var filePath = CreateTomlFile(fileName);

        var doc = CsTomlFileSerializer.Deserialize<TomlDocument>(filePath, CsTomlSerializerOptions.Default, TomlFileExtensionPolicy.Relaxed);

        doc.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("config.cfg")]
    [InlineData("settings.ini")]
    [InlineData("data.txt")]
    [InlineData("noextension")]
    public void Serialize_Relaxed_WithVariousExtensions_Succeeds(string fileName)
    {
        var filePath = GetNonExistentPath(fileName);
        var doc = CsTomlSerializer.Deserialize<TomlDocument>("key = \"value\""u8);

        Should.NotThrow(() =>
            CsTomlFileSerializer.Serialize(filePath, doc, CsTomlSerializerOptions.Default, TomlFileExtensionPolicy.Relaxed));

        File.Exists(filePath).ShouldBeTrue();
    }

    [Fact]
    public void Deserialize_UndefinedPolicy_ThrowsArgumentOutOfRangeException()
    {
        var filePath = CreateTomlFile("test.toml");

        Should.Throw<ArgumentOutOfRangeException>(() =>
            CsTomlFileSerializer.Deserialize<TomlDocument>(filePath, CsTomlSerializerOptions.Default, (TomlFileExtensionPolicy)99));
    }

    [Fact]
    public async ValueTask DeserializeAsync_UndefinedPolicy_ThrowsArgumentOutOfRangeException()
    {
        var filePath = CreateTomlFile("test.toml");

        await Should.ThrowAsync<ArgumentOutOfRangeException>(async () =>
            await CsTomlFileSerializer.DeserializeAsync<TomlDocument>(filePath, CsTomlSerializerOptions.Default, (TomlFileExtensionPolicy)99, cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public void Serialize_UndefinedPolicy_ThrowsArgumentOutOfRangeException()
    {
        var filePath = GetNonExistentPath("output.toml");
        var doc = CsTomlSerializer.Deserialize<TomlDocument>("key = \"value\""u8);

        Should.Throw<ArgumentOutOfRangeException>(() =>
            CsTomlFileSerializer.Serialize(filePath, doc, CsTomlSerializerOptions.Default, (TomlFileExtensionPolicy)99));
    }

    [Fact]
    public async ValueTask SerializeAsync_UndefinedPolicy_ThrowsArgumentOutOfRangeException()
    {
        var filePath = GetNonExistentPath("output.toml");
        var doc = CsTomlSerializer.Deserialize<TomlDocument>("key = \"value\""u8);

        await Should.ThrowAsync<ArgumentOutOfRangeException>(async () =>
            await CsTomlFileSerializer.SerializeAsync(filePath, doc, CsTomlSerializerOptions.Default, (TomlFileExtensionPolicy)99, cancellationToken: TestContext.Current.CancellationToken));
    }

}