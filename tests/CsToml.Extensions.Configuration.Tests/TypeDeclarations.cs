
namespace CsToml.Extensions.Configuration.Tests;

public record Test
{
    public string? Key { get; init; }
    public Physical? Physical { get; init; }
    public int Int1 { get; init; }
    public double Flt2 { get; init; }
    public bool Bool1 { get; init; }
    public DateTimeOffset Odt2 { get; init; }
    public DateTime Ldt2 { get; init; }
    public DateOnly Ld1 { get; init; }
    public TimeOnly Lt1 { get; init; }
    public int[] Integers { get; init; }
    public Table? Table { get; init; }
    public Dictionary<string, Fruit>? Fruit { get; init; }
    public List<Product>? Products { get; init; }

}

public record Physical
{
    public string? Color { get; init; }
}

public record Fruit
{
    public string? Color { get; init; }
    public bool Sweet { get; init; }
}

public record Table
{
    public string? Key1 { get; init; }
    public int Key2 { get; init; }
}

public record Product
{
    public string? Name { get; init; }
}
