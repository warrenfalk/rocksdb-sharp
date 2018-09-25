using System;

namespace RocksDbSharp
{
    public class NativePackage
    {
        public static string GetCodeBase() => AppContext.BaseDirectory;
    }
}
