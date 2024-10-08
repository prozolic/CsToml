using CsToml.Error;
using CsToml.Extensions;
using FluentAssertions.Execution;

namespace CsToml.Tests;

/// <summary>
/// Verify with the verification Toml files.
/// The 'toml-test' repository is borrowed for the verification Toml files.
/// https://github.com/toml-lang/toml-test
/// </summary>
public class TomlTest
{
    private static readonly string tomlTestDirectoryPath = "./../../../toml-test/";
    private static readonly string ValidDirectory = "valid";
    private static readonly string InvalidDirectory = "invalid";
    private static readonly string TomlExtension = ".toml";
    private static readonly string TomlFilesVer100 = "files-toml-1.0.0";

    [Theory, MemberData(nameof(ValidTomlFile))]
    public void ValidTest(string tomlFile)
    {
        try
        {
            var document = CsTomlFileSerializer.Deserialize<TomlDocument>(tomlFile);
        }
        catch (CsTomlSerializeException ctse)
        {
            foreach(var ce in ctse.Exceptions!)
            {
                Execute.Assertion.FailWith($"TomlFile:{tomlFile} Message:{ce}");
            }
        }
        catch (Exception e)
        {
            Execute.Assertion.FailWith($"TomlFile:{tomlFile} Message:{e}");
        }
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
            Execute.Assertion.FailWith($"TomlFile:{tomlFile} Message:{e}");
        }
        Execute.Assertion.FailWith($"TomlFile:{tomlFile} Message:Incorrect syntax was not detected.");
    }

    public static IEnumerable<object[]> ValidTomlFile()
    {
        var filesToml = Path.Combine(tomlTestDirectoryPath, TomlFilesVer100);
        var files = File.ReadAllLines(filesToml);
        foreach (var file in files)
        {
            var directoryName = file.Split('/')[0];
            var tomlFile = new FileInfo(Path.Combine(tomlTestDirectoryPath, file));
            if (tomlFile.Exists && tomlFile.Extension == TomlExtension && directoryName == ValidDirectory)
            {
                yield return new object[] { Path.Combine(tomlTestDirectoryPath, file) };
            }
        }
    }

    public static IEnumerable<object[]> InvalidTomlFile()
    {
        var filesToml = Path.Combine(tomlTestDirectoryPath, TomlFilesVer100);
        var files = File.ReadAllLines(filesToml);
        foreach (var file in files)
        {
            var directoryName = file.Split('/')[0];
            var tomlFile = new FileInfo(Path.Combine(tomlTestDirectoryPath, file));
            if (tomlFile.Exists && tomlFile.Extension == TomlExtension && directoryName == InvalidDirectory)
            {
                yield return new object[] { Path.Combine(tomlTestDirectoryPath, file) };

            }
        }
    }
}
