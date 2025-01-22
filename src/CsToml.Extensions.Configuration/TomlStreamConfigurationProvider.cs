using Microsoft.Extensions.Configuration;

namespace CsToml.Extensions.Configuration;

public class TomlStreamConfigurationProvider(TomlStreamConfigurationSource source) : StreamConfigurationProvider(source)
{
    public override void Load(Stream stream)
    {
        Data = new TomlStreamConfigurationParser().Parse(stream);
    }
}