using CsToml.Error;
using Microsoft.Extensions.Configuration;

namespace CsToml.Extensions.Configuration;

internal sealed class TomlFileConfigurationProvider(FileConfigurationSource source) : FileConfigurationProvider(source)
{
    public override void Load(Stream stream)
    {
        Span<byte> s = stackalloc byte[100];
        var ss = stream.Read(s);
        stream.Seek(0, SeekOrigin.Begin);
        Data = new TomlFileConfigurationParser().Parse(stream);
    }
}
