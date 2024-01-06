using BenchmarkDotNet.Attributes;
using CsToml;
using System.Text;

namespace Benchmark;

[Config(typeof(BenchmarkConfig))]
public class Benchmark
{
    private static byte[] tomlUtf8Text;
    private static string tomlUtf16Text;

    [GlobalSetup]
    public void GlobalSetup()
    {
        tomlUtf8Text = @"
# This is a TOML document.

title = ""TOML Example""

[owner]
name = ""Tom Preston-Werner""
dob = 1979-05-27T07:32:00-08:00 # First class dates

[database]
server = ""192.168.1.1""
ports = [ 8000, 8001, 8002 ]
connection_max = 5000
enabled = true

[servers]

  # Indentation (tabs and/or spaces) is allowed but not required
  [servers.alpha]
  ip = ""10.0.0.1""
  dc = ""eqdc10""

  [servers.beta]
  ip = ""10.0.0.2""
  dc = ""eqdc10""

[clients]
data = [ [""gamma"", ""delta""], [1, 2] ]

# Line breaks are OK when inside arrays
hosts = [
  ""alpha"",
  ""omega""
]"u8.ToArray();

        tomlUtf16Text = Encoding.UTF8.GetString(tomlUtf8Text);
    }


    [BenchmarkCategory("Sample"), Benchmark]
    public void TestCsToml()
    {
        var package = new CsTomlPackage();
        CsTomlSerializer.Deserialize(tomlUtf8Text, ref package);
    }

    [BenchmarkCategory("Sample"), Benchmark]
    public void TestTommy()
    {
        using var reader = new StringReader(tomlUtf16Text);
        var table = Tommy.TOML.Parse(reader);                                                                                             
    }


}

