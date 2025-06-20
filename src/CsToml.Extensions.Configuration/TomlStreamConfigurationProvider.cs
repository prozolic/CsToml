using Microsoft.Extensions.Configuration;

namespace CsToml.Extensions.Configuration;

public class TomlStreamConfigurationProvider(TomlStreamConfigurationSource source, CsTomlSerializerOptions? serializerOptions) : StreamConfigurationProvider(source)
{
    public CsTomlSerializerOptions? SerializerOptions { get; init; } = serializerOptions;

    public TomlStreamConfigurationProvider(TomlStreamConfigurationSource source) : this(source, null)
    {}

    public override void Load(Stream stream)
    {
        Data = new TomlStreamConfigurationParser().Parse(stream, SerializerOptions);
    }
}