using CsToml.Error;
using CsToml.Extensions;
using System.Reflection.Metadata;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace CsToml.Tests;

/// <summary>
/// Verify with the verification Toml files.
/// The 'toml-test' repository is borrowed for the verification Toml files.
/// https://github.com/toml-lang/toml-test
/// </summary>
public class TomlTest
{
    private static readonly string TomlTestDirectoryPath = FindTomlTestDirectory();
    private static readonly string ValidDirectory = "valid";
    private static readonly string InvalidDirectory = "invalid";
    private static readonly string TomlExtension = ".toml";
    private static readonly string TomlFilesVer100 = "files-toml-1.0.0";
    private static readonly string TomlFilesVer110 = "files-toml-1.1.0";

    private static readonly HashSet<string> ExcludedFilesForV110 = new HashSet<string>()
    {
        "invalid/key/special-character.toml"
    };

    // `dotnet test` runs from bin/<config>/<tfm>/ and a NativeAOT build from bin/<config>/<tfm>/<rid>/publish/,
    // so search upward from the executable instead of using a fixed relative path.
    private static string FindTomlTestDirectory()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "toml-test");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }
        throw new DirectoryNotFoundException($"toml-test was not found above {AppContext.BaseDirectory}.");
    }

    // Test arguments stay relative to toml-test/ so that test names do not depend on the checkout location.
    private static string FullPath(string file) => Path.Combine(TomlTestDirectoryPath, file);

    [Test, MethodDataSource(nameof(ValidTomlFileV100))]
    public void ValidTestV100(string tomlFile, string jsonFile)
    {
        Should.NotThrow(() => {
            var document = CsTomlFileSerializer.Deserialize<TomlDocument>(FullPath(tomlFile));

            var jsonNode = JsonNode.Parse(File.ReadAllText(FullPath(jsonFile)))!;
            var tomlDocumentJsonNode = document!.ToJsonObject();
            JsonNodeExtensions.DeepEqualsForTomlFormat(jsonNode, tomlDocumentJsonNode).ShouldBeTrue();
        }, $"TomlFile:{tomlFile}");
    }

    [Test, MethodDataSource(nameof(ValidTomlFileV110))]
    public void ValidTestV110(string tomlFile, string jsonFile)
    {
        Should.NotThrow(() => {
            var document = CsTomlFileSerializer.Deserialize<TomlDocument>(FullPath(tomlFile), Options.TomlSpecVersion110);

            var jsonNode = JsonNode.Parse(File.ReadAllText(FullPath(jsonFile)))!;
            var tomlDocumentJsonNode = document!.ToJsonObject();
            JsonNodeExtensions.DeepEqualsForTomlFormat(jsonNode, tomlDocumentJsonNode).ShouldBeTrue();
        }, $"TomlFile:{tomlFile}");
    }

    [Test, MethodDataSource(nameof(InvalidTomlFileV100))]
    public void InvalidTestV100(string tomlFile)
    {
        Should.Throw<CsTomlSerializeException>(() => {
            var document = CsTomlFileSerializer.Deserialize<TomlDocument>(FullPath(tomlFile));
        }, $"TomlFile:{tomlFile}");
    }

    [Test, MethodDataSource(nameof(InvalidTomlFileV110))]
    public void InvalidTestV110(string tomlFile)
    {
        Should.Throw<CsTomlSerializeException>(() => {
            var document = CsTomlFileSerializer.Deserialize<TomlDocument>(FullPath(tomlFile), Options.TomlSpecVersion110);
        }, $"TomlFile:{tomlFile}");
    }

    [Test, MethodDataSource(nameof(ValidTomlFileV100))]
    public void ValidTestForStream(string tomlFile, string jsonFile)
    {
        Should.NotThrow(() => {
            using var fs = new FileStream(FullPath(tomlFile), FileMode.Open, FileAccess.Read);
            var document = CsTomlSerializer.Deserialize<TomlDocument>(fs);

            var jsonNode = JsonNode.Parse(File.ReadAllText(FullPath(jsonFile)))!;
            var tomlDocumentJsonNode = document!.ToJsonObject();
            JsonNodeExtensions.DeepEqualsForTomlFormat(jsonNode, tomlDocumentJsonNode).ShouldBeTrue();
        }, $"TomlFile:{tomlFile}");
    }

    [Test, MethodDataSource(nameof(ValidTomlFileV100))]
    public async Task ValidTestForStreamAsync(string tomlFile, string jsonFile)
    {
        var task = Should.NotThrowAsync(async () =>
        {
            using var fs = new FileStream(FullPath(tomlFile), FileMode.Open, FileAccess.Read);
            var document = await CsTomlSerializer.DeserializeAsync<TomlDocument>(fs);

            var jsonNode = JsonNode.Parse(File.ReadAllText(FullPath(jsonFile)))!;
            var tomlDocumentJsonNode = document!.ToJsonObject();
            JsonNodeExtensions.DeepEqualsForTomlFormat(jsonNode, tomlDocumentJsonNode).ShouldBeTrue();
        }, $"TomlFile:{tomlFile}");

        await task;
    }

    [Test, MethodDataSource(nameof(InvalidTomlFileV100))]
    public void InvalidTestForStream(string tomlFile)
    {
        Should.Throw<CsTomlSerializeException>(() => {
            using var fs = new FileStream(FullPath(tomlFile), FileMode.Open, FileAccess.Read);
            var document = CsTomlSerializer.Deserialize<TomlDocument>(fs);
        }, $"TomlFile:{tomlFile}");
    }

    [Test, MethodDataSource(nameof(ValidTomlFileV100))]
    public void EncoderTestV100(string tomlFile, string jsonFile)
    {
        Should.NotThrow(() =>
        {
            var documentA = CsTomlFileSerializer.Deserialize<TomlDocument>(FullPath(tomlFile));
            using var result = CsTomlSerializer.Serialize(documentA);
            var documentB = CsTomlSerializer.Deserialize<TomlDocument>(result.ByteSpan);

            var jsonNode = JsonNode.Parse(File.ReadAllText(FullPath(jsonFile)))!;
            var actualJsonNode = documentB!.ToJsonObject();
            JsonNodeExtensions.DeepEqualsForTomlFormat(jsonNode, actualJsonNode).ShouldBeTrue();
        }, $"TomlFile:{tomlFile}");
    }

    [Test, MethodDataSource(nameof(ValidTomlFileV110))]
    public void EncoderTestV110(string tomlFile, string jsonFile)
    {
        Should.NotThrow(() =>
        {
            var documentA = CsTomlFileSerializer.Deserialize<TomlDocument>(FullPath(tomlFile), Options.TomlSpecVersion110);
            using var result = CsTomlSerializer.Serialize(documentA);
            var documentB = CsTomlSerializer.Deserialize<TomlDocument>(result.ByteSpan, Options.TomlSpecVersion110);

            var jsonNode = JsonNode.Parse(File.ReadAllText(FullPath(jsonFile)))!;
            var actualJsonNode = documentB!.ToJsonObject();
            JsonNodeExtensions.DeepEqualsForTomlFormat(jsonNode, actualJsonNode).ShouldBeTrue();
        }, $"TomlFile:{tomlFile}");
    }

    public static IEnumerable<(string TomlFile, string JsonFile)> ValidTomlFileV100()
    {
        var filesToml = Path.Combine(TomlTestDirectoryPath, TomlFilesVer100);
        var files = File.ReadAllLines(filesToml);

        var filePathTable = new HashSet<string>();

        // valid/...
        foreach (var file in files.Where(f => f.Split('/')[0] == ValidDirectory))
        {
            filePathTable.Add(Path.ChangeExtension(file, string.Empty));
        }

        foreach(var file in filePathTable)
        {
            yield return (Path.ChangeExtension(file, "toml"), Path.ChangeExtension(file, "json"));
        }
    }

    public static IEnumerable<(string TomlFile, string JsonFile)> ValidTomlFileV110()
    {
        var filesToml = Path.Combine(TomlTestDirectoryPath, TomlFilesVer110);
        var files = File.ReadAllLines(filesToml);

        var filePathTable = new HashSet<string>();

        // valid/...
        foreach (var file in files.Where(f => f.Split('/')[0] == ValidDirectory))
        {
            filePathTable.Add(Path.ChangeExtension(file, string.Empty));
        }

        foreach (var file in filePathTable)
        {
            yield return (Path.ChangeExtension(file, "toml"), Path.ChangeExtension(file, "json"));
        }
    }

    public static IEnumerable<string> InvalidTomlFileV100()
    {
        var filesToml = Path.Combine(TomlTestDirectoryPath, TomlFilesVer100);
        var files = File.ReadAllLines(filesToml);
        foreach (var file in files)
        {
            var directoryName = file.Split('/')[0];
            var tomlFile = new FileInfo(Path.Combine(TomlTestDirectoryPath, file));
            if (tomlFile.Exists && tomlFile.Extension == TomlExtension && directoryName == InvalidDirectory)
            {
                yield return file;
            }
        }
    }

    public static IEnumerable<string> InvalidTomlFileV110()
    {
        var filesToml = Path.Combine(TomlTestDirectoryPath, TomlFilesVer110);
        var files = File.ReadAllLines(filesToml);
        foreach (var file in files)
        {
            if (ExcludedFilesForV110.Contains(file))
            {
                continue;
            }
            var directoryName = file.Split('/')[0];
            var tomlFile = new FileInfo(Path.Combine(TomlTestDirectoryPath, file));
            if (tomlFile.Exists && tomlFile.Extension == TomlExtension && directoryName == InvalidDirectory)
            {
                yield return file;
            }
        }
    }
}
