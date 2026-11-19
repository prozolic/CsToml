using System.Globalization;
using System.Text;

namespace CsToml.Tests;

public class CultureInvariantTest
{
    private const string Toml = "ld = 1979-05-27\nlt = 07:32:00.5\nldt = 1979-05-27T07:32:00.5\nodt = 1979-05-27T07:32:00.5Z\n";

    [Test]
    [Arguments("th-TH")]
    [Arguments("fi-FI")]
    public void FormatsIndependentlyOfCurrentCulture(string cultureName)
    {
        var current = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(cultureName);
            var document = CsTomlSerializer.Deserialize<TomlDocument>(Encoding.UTF8.GetBytes(Toml));

            using var serialized = CsTomlSerializer.Serialize(document);
            Encoding.UTF8.GetString(serialized.ByteSpan).ShouldBe(Toml.Replace("\n", Environment.NewLine));
            document.RootNode["ld"u8].GetTomlValue().ToString().ShouldBe("1979-05-27");
            document.RootNode["lt"u8].GetTomlValue().ToString().ShouldBe("07:32:00.5000000");
            document.RootNode["ldt"u8].GetTomlValue().ToString().ShouldBe("1979-05-27T07:32:00.5000000");
        }
        finally
        {
            CultureInfo.CurrentCulture = current;
        }
    }

    [Test]
    [SkipOnNativeAot("Dictionary<double, int> has no pre-registered formatter.")]
    public void ParsesKeysIndependentlyOfCurrentCulture()
    {
        var current = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            CsTomlSerializer.Deserialize<Dictionary<double, int>>("'1.5' = 1"u8).Keys.ShouldBe([1.5]);
        }
        finally
        {
            CultureInfo.CurrentCulture = current;
        }
    }
}
