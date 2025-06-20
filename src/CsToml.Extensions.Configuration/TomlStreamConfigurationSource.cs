using Microsoft.Extensions.Configuration;

namespace CsToml.Extensions.Configuration;

public class TomlStreamConfigurationSource : StreamConfigurationSource
{
    public CsTomlSerializerOptions? SerializerOptions { get; set; }

    public override IConfigurationProvider Build(IConfigurationBuilder builder)
        => new TomlStreamConfigurationProvider(this, SerializerOptions);
}