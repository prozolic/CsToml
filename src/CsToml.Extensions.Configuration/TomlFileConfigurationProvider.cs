using Microsoft.Extensions.Configuration;

namespace CsToml.Extensions.Configuration;

public class TomlFileConfigurationProvider(TomlFileConfigurationSource source, CsTomlSerializerOptions? serializerOptions) : FileConfigurationProvider(source)
{
    public CsTomlSerializerOptions? SerializerOptions { get; init; } = serializerOptions;

    public TomlFileConfigurationProvider(TomlFileConfigurationSource source) : this(source, null)
    { }

    public override void Load(Stream stream)
    {
        Data = new TomlStreamConfigurationParser().Parse(stream, SerializerOptions);
    }
}
