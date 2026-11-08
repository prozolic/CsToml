
namespace CsToml.Extensions.Configuration.Tests;

// Setters, not init: with PublishAot the SDK switches Bind() to the configuration binding source generator,
// which cannot assign init-only properties of an existing instance.
public record Test
{
    public string? Key { get; set; }
    public Physical? Physical { get; set; }
    public int Int1 { get; set; }
    public double Flt2 { get; set; }
    public bool Bool1 { get; set; }
    public DateTimeOffset Odt2 { get; set; }
    public DateTime Ldt2 { get; set; }
    public DateOnly Ld1 { get; set; }
    public TimeOnly Lt1 { get; set; }
    public int[]? Integers { get; set; }
    public Table? Table { get; set; }
    public Dictionary<string, Fruit>? Fruit { get; set; }
    public List<Product>? Products { get; set; }

}

public record Physical
{
    public string? Color { get; set; }
}

public record Fruit
{
    public string? Color { get; set; }
    public bool Sweet { get; set; }
}

public record Table
{
    public string? Key1 { get; set; }
    public int Key2 { get; set; }
}

public record Product
{
    public string? Name { get; set; }
}
