using CsToml;
using System;
using System.Buffers;

namespace ConsoleApp;


public class TestPackage : CsTomlPackage, ICsTomlPackageCreator<TestPackage>
{
    static TestPackage ICsTomlPackageCreator<TestPackage>.CreatePackage()
    {
        return new TestPackage();
    }

    public string Key 
    { 
        get
        {
            if (TryFind("key"u8, out var value))
                return value!.GetString();
            return string.Empty;
        } 
    }

    public long Number
    {
        get
        {
            if (TryFind("number"u8, out var value))
                return value!.GetInt64();
            return default;
        }
    }
}




