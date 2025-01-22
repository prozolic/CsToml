using Microsoft.Extensions.Configuration;
using Shouldly;

namespace CsToml.Extensions.Configuration.Tests;


public class ConfigurationTest
{
    [Fact]
    public void ConfigurationBuilderTest()
    {
        Should.NotThrow(() =>
        {
            var conf = new ConfigurationBuilder().AddTomlFile("test.toml").Build();
        });
        Should.NotThrow(() =>
        {
            using var fs = new FileStream("test.toml", FileMode.Open);
            var conf = new ConfigurationBuilder().AddTomlStream(fs).Build();
        });
    }
}