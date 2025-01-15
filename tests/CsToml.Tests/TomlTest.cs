using CsToml.Error;
using CsToml.Extensions;
using System.Text.Json.Nodes;

namespace CsToml.Tests;

/// <summary>
/// Verify with the verification Toml files.
/// The 'toml-test' repository is borrowed for the verification Toml files.
/// https://github.com/toml-lang/toml-test
/// </summary>
public class TomlTest
{
    private static readonly string TomlTestDirectoryPath = "./../../../toml-test/";
    private static readonly string ValidDirectory = "valid";
    private static readonly string InvalidDirectory = "invalid";
    private static readonly string TomlExtension = ".toml";
    private static readonly string TomlFilesVer100 = "files-toml-1.0.0";

    [Theory, MemberData(nameof(ValidTomlFile))]
    public void ValidTest(string tomlFile, string jsonFile)
    {
        TomlDocument document = null;
        try
        {
            document = CsTomlFileSerializer.Deserialize<TomlDocument>(tomlFile);
        }
        catch (CsTomlSerializeException ctse)
        {
            foreach(var ce in ctse.Exceptions!)
            {
                Should.Throw<CsTomlSerializeException>(static () => {}, $"TomlFile:{tomlFile} Message:{ce}");
            }
        }
        catch (Exception e)
        {
            Should.Throw<Exception>(static () => { }, $"TomlFile:{tomlFile} Message:{e}");
        }

        var jsonNode = JsonNode.Parse(File.ReadAllText(jsonFile))!;
        var tomlDocumentJsonNode = document!.ToJsonObject();

        JsonNodeExtensions.DeepEqualsForTomlFormat(jsonNode, tomlDocumentJsonNode).ShouldBeTrue();
        //JsonNode.DeepEquals(tomlDocumentJsonNode, jsonNode).ShouldBeTrue();
    }

    [Theory, MemberData(nameof(InvalidTomlFile))]
    public void InvalidTest(string tomlFile)
    {
        try
        {
            var document = CsTomlFileSerializer.Deserialize<TomlDocument>(tomlFile);
        }
        catch (CsTomlSerializeException)
        {
            return;
        }
        catch (Exception e)
        {
            Should.Throw<Exception>(static () => { }, $"TomlFile:{tomlFile} Message:{e}");
        }

        Should.Throw<Exception>(static () => { }, $"TomlFile:{tomlFile} Message:Incorrect syntax was not detected.");
    }

    [Theory, MemberData(nameof(ValidTomlFile))]
    public void ValidTestForStream(string tomlFile, string jsonFile)
    {
        TomlDocument document = null;
        try
        {
            using var fs = new FileStream(tomlFile, FileMode.Open, FileAccess.Read);
            document = CsTomlSerializer.Deserialize<TomlDocument>(fs);
        }
        catch (CsTomlSerializeException ctse)
        {
            foreach (var ce in ctse.Exceptions!)
            {
                Should.Throw<CsTomlSerializeException>(static () => { }, $"TomlFile:{tomlFile} Message:{ce}");
            }
        }
        catch (Exception e)
        {
            Should.Throw<Exception>(static () => { }, $"TomlFile:{tomlFile} Message:{e}");
        }

        var jsonNode = JsonNode.Parse(File.ReadAllText(jsonFile))!;
        var tomlDocumentJsonNode = document!.ToJsonObject();

        JsonNodeExtensions.DeepEqualsForTomlFormat(jsonNode, tomlDocumentJsonNode).ShouldBeTrue();
        //JsonNode.DeepEquals(tomlDocumentJsonNode, jsonNode).ShouldBeTrue();
    }

    [Theory, MemberData(nameof(ValidTomlFile))]
    public async Task ValidTestForStreamAsync(string tomlFile, string jsonFile)
    {
        TomlDocument document = null;
        try
        {
            using var fs = new FileStream(tomlFile, FileMode.Open, FileAccess.Read);
            document = await CsTomlSerializer.DeserializeAsync<TomlDocument>(fs);
        }
        catch (CsTomlSerializeException ctse)
        {
            foreach (var ce in ctse.Exceptions!)
            {
                Should.Throw<CsTomlSerializeException>(static () => { }, $"TomlFile:{tomlFile} Message:{ce}");
            }
        }
        catch (Exception e)
        {
            Should.Throw<Exception>(static () => { }, $"TomlFile:{tomlFile} Message:{e}");
        }

        var jsonNode = JsonNode.Parse(File.ReadAllText(jsonFile))!;
        var tomlDocumentJsonNode = document!.ToJsonObject();

        JsonNodeExtensions.DeepEqualsForTomlFormat(jsonNode, tomlDocumentJsonNode).ShouldBeTrue();
        //JsonNode.DeepEquals(tomlDocumentJsonNode, jsonNode).ShouldBeTrue();
    }

    [Theory, MemberData(nameof(InvalidTomlFile))]
    public void InvalidTestForStream(string tomlFile)
    {
        try
        {
            using var fs = new FileStream(tomlFile, FileMode.Open, FileAccess.Read);
            var document = CsTomlSerializer.Deserialize<TomlDocument>(fs);
        }
        catch (CsTomlSerializeException)
        {
            return;
        }
        catch (Exception e)
        {
            Should.Throw<Exception>(static () => { }, $"TomlFile:{tomlFile} Message:{e}");
        }
        Should.Throw<Exception>(static () => { }, $"TomlFile:{tomlFile} Message:Incorrect syntax was not detected.");
    }

    public static IEnumerable<object[]> ValidTomlFile()
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
            yield return new object[] {
                Path.Combine(TomlTestDirectoryPath, Path.ChangeExtension(file, "toml")),
                Path.Combine(TomlTestDirectoryPath, Path.ChangeExtension(file, "json")),
            };
        }
    }

    public static IEnumerable<object[]> InvalidTomlFile()
    {
        var filesToml = Path.Combine(TomlTestDirectoryPath, TomlFilesVer100);
        var files = File.ReadAllLines(filesToml);
        foreach (var file in files)
        {
            var directoryName = file.Split('/')[0];
            var tomlFile = new FileInfo(Path.Combine(TomlTestDirectoryPath, file));
            if (tomlFile.Exists && tomlFile.Extension == TomlExtension && directoryName == InvalidDirectory)
            {
                yield return new object[] { Path.Combine(TomlTestDirectoryPath, file) };
            }
        }
    }

}
