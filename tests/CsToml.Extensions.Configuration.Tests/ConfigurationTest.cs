using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
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

    [Fact]
    public void EqualsTest()
    {
        var toml = @"
key = ""value""
physical.color = ""orange""
int1 = +99
flt2 = 3.1415
bool1 = true
odt2 = 1979-05-27T00:32:00-07:00
ldt2 = 1979-05-27T00:32:00.999999
ld1 = 1979-05-27
lt1 = 07:32:00
integers = [ 1, 2, 3 ]

[Table]
key1 = ""some string""
key2 = 123

[fruit]
apple.color = ""red""
apple.sweet = true

[[products]]
name = ""Hammer""

[[products]]  # empty table within the array

[[products]]
name = ""Tom""

"u8;
        var ms = new MemoryStream(toml.ToArray());
        var conf = new ConfigurationBuilder().AddTomlStream(ms).Build();

        var actual = new Test();
        conf.Bind(actual);

        var expected = new Test()
        {
            Key = "value",
            Physical = new Physical() { Color = "orange" },
            Int1 = 99,
            Flt2 = 3.1415,
            Bool1 = true,
            Odt2 = new DateTimeOffset(1979, 05, 27, 0, 32, 0, new TimeSpan(-7, 0, 0)),
            Ldt2 = new DateTime(1979, 05, 27, 0, 32, 0, 999, 999),
            Ld1 = new DateOnly(1979, 05, 27),
            Lt1 = new TimeOnly(7, 32, 0),
            Integers = [1, 2, 3],
            Table = new Table() { Key1 = "some string", Key2 = 123 },
            Fruit = new Dictionary<string, Fruit>()
            {
                ["apple"] = new Fruit() { Color = "red", Sweet = true }
            },
            Products = new List<Product>()
            {
                new Product() { Name = "Hammer" },
                new Product() { Name = "Tom" }
            },
        };

        actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public void EqualsTestWithSerializerOptions()
    {
        var toml = @"
key = ""value""
physical.color = ""orange""
int1 = +99
flt2 = 3.1415
bool1 = true
odt2 = 1979-05-27T00:32:00-07:00
ldt2 = 1979-05-27T00:32:00.999999
ld1 = 1979-05-27
lt1 = 07:32:00
integers = [ 1, 2, 3 ]

[Table]
key1 = ""some string""
key2 = 123

[fruit]
apple.color = ""red""
apple.sweet = true

[[products]]
name = ""Hammer""

[[products]]  # empty table within the array

[[products]]
name = ""Tom""

"u8;
        var ms = new MemoryStream(toml.ToArray());
        var conf = new ConfigurationBuilder().AddTomlStream(ms, Options.TomlSpecVersion110).Build();

        var actual = new Test();
        conf.Bind(actual);

        var expected = new Test()
        {
            Key = "value",
            Physical = new Physical() { Color = "orange" },
            Int1 = 99,
            Flt2 = 3.1415,
            Bool1 = true,
            Odt2 = new DateTimeOffset(1979, 05, 27, 0, 32, 0, new TimeSpan(-7, 0, 0)),
            Ldt2 = new DateTime(1979, 05, 27, 0, 32, 0, 999, 999),
            Ld1 = new DateOnly(1979, 05, 27),
            Lt1 = new TimeOnly(7, 32, 0),
            Integers = [1, 2, 3],
            Table = new Table() { Key1 = "some string", Key2 = 123 },
            Fruit = new Dictionary<string, Fruit>()
            {
                ["apple"] = new Fruit() { Color = "red", Sweet = true }
            },
            Products = new List<Product>()
            {
                new Product() { Name = "Hammer" },
                new Product() { Name = "Tom" }
            },
        };

        actual.ShouldBeEquivalentTo(expected);
    }
}