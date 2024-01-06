
using CsToml;
using System.Text;

Console.WriteLine("Hello, World!");

var package = new CsTomlPackage();
CsTomlSerializer.ReadAndDeserialize("./../../../Toml/test.toml", ref package);

//var trueValue = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference<byte>("true"u8)); // 1702195828
//var falseValue = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference<byte>("fals"u8)); // 1936482662

//var fnfValue = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference<byte>("inf"u8)); // 1702195828
//var nanValue = Unsafe.ReadUnaligned<int>(ref MemoryMarshal.GetReference<byte>("nan"u8)); // 1936482662

var package2 = new CsTomlPackage();
CsTomlSerializer.ReadAndDeserialize("./../../../Toml/test2.toml", ref package2);
var toml2 = CsTomlSerializer.Serialize(ref package2);
var tomlUTf16 = Encoding.UTF8.GetString(toml2);

var package3 = new CsTomlPackage();
package3.IsThrowCsTomlException = true;
CsTomlSerializer.Deserialize(toml2, ref package3);
var toml3 = CsTomlSerializer.Serialize(ref package3);
var toml2UTf16 = Encoding.UTF8.GetString(toml3);

Console.WriteLine("END");

