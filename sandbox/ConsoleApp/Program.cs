
using CsToml;
using System.Text;

Console.WriteLine("Hello, World!");

var package = new CsTomlPackage();
CsTomlSerializer.ReadAndDeserialize(ref package, "./../../../Toml/test.toml");

////var trueValue = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference<byte>("true"u8)); // 1702195828
////var falseValue = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference<byte>("fals"u8)); // 1936482662

////var fnfValue = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference<byte>("inf"u8)); // 1702195828
////var nanValue = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference<byte>("nan"u8)); // 1936482662

var package2 = new CsTomlPackage();
CsTomlSerializer.ReadAndDeserialize(ref package2, "./../../../Toml/test2.toml");
var toml2 = CsTomlSerializer.Serialize(package2);
var tomlUTf16 = Encoding.UTF8.GetString(toml2);

var package3 = new CsTomlPackage();
package3.IsThrowCsTomlException = true;
CsTomlSerializer.Deserialize(ref package3, toml2);
var toml3 = CsTomlSerializer.Serialize(package3);
var toml2UTf16 = Encoding.UTF8.GetString(toml3);

Console.WriteLine("END");

