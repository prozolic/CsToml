using CsToml.Error;
using System.Numerics;

namespace CsToml.Tests;

public class ComplexTest
{
    [Test]
    public void Deserialize_InvalidShape_Throws()
    {
        var document = CsTomlSerializer.Deserialize<TomlDocument>("short = [ 1.0 ]\nlong = [ 1.0, 2.0, 3.0 ]\n"u8);

        Should.Throw<CsTomlException>(() => document.RootNode["short"].GetValue<Complex>());
        Should.Throw<CsTomlException>(() => document.RootNode["long"].GetValue<Complex>());
        Should.Throw<CsTomlException>(() => document.RootNode["short"].GetValue<Complex?>());
        Should.Throw<CsTomlException>(() => document.RootNode["long"].GetValue<Complex?>());
    }
}